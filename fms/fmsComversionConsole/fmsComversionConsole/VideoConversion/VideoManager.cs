using System;
using DynamicSugar;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.IO;
using fAI;
using NReco.VideoInfo;
using static NReco.VideoInfo.MediaInfo;
using static fms.VideoManager;
using NReco.VideoConverter;
using Azure;
using fmsComversionConsole.Utility;
using M3U8Parser.Attributes;

namespace fms
{
    public partial class VideoManager
    {

        const string masterM3u8Filemame = "master.m3u8";
        const string AudioPlayListM3u8Filemame = "audioplaylist.m3u8";
        

        private MediaInfo _mediaInfo { get; set; }
        private string _inputVideoFileNameOrUrl { get; }
        
        public int Width => GetVideoStream.Width;
        public int Height => GetVideoStream.Height;
        public bool IsSquareResolution => Width == Height;

        public StreamInfo GetVideoStream => _mediaInfo.Streams.Where(s => s.CodecType == "video").FirstOrDefault();
        public StreamInfo GetAudioStream => _mediaInfo.Streams.Where(s => s.CodecType == "audio").FirstOrDefault();

        public VideoManager(string videoFileName)
        {
            _inputVideoFileNameOrUrl = videoFileName;
            var ffProbe = new NReco.VideoInfo.FFProbe();
            _mediaInfo = ffProbe.GetMediaInfo(_inputVideoFileNameOrUrl);
        }
   
        // https://www.nrecosite.com/video_info_net.aspx
        public string GetVideoInfo()
        {
            var sb = new StringBuilder();
            sb.Append($"file: {this._inputVideoFileNameOrUrl}, ").AppendLine();

            var fileSize = -1L;
            if(!DirectoryFileService.IsUrl(this._inputVideoFileNameOrUrl))
                fileSize = new FileInfo(this._inputVideoFileNameOrUrl).Length;

            var v = _mediaInfo;
            sb.Append(DS.Dictionary(new { fileSize, v.FormatName, v.Duration, StreamsCount = v.Streams.ToList().Count }).Format()).AppendLine();

            foreach (var s in v.Streams)
                sb.Append(DS.Dictionary(new {s.CodecName, s.CodecType, s.FrameRate, s.Width, s.Height, s.PixelFormat }).Format()).AppendLine();

            return sb.ToString();
        }

        public void FixPathInM3U8(string videoFolder, string search, string replacement)
        {
            var files = Directory.GetFiles(videoFolder, "*.m3u8");
            foreach(var f in files)
            {
                var text = File.ReadAllText(f);
                text = text.Replace(search, replacement);
                File.WriteAllText(f, text);
            }
        }

        public HlsConversionResult ConvertAudioToHls(string hlsFolder, string ffmepexe, string azureStorageConnectionString, string cdnHost, string fmsVideoId = null, bool? deriveFmsVideoId = null)
        {
            Logger.Trace($"");
            Logger.Trace($"[CONVERSION][AUDIO] Start - {this.GetVideoInfo()}");

            // fmdVideoId initialization, different mode possible
            if (string.IsNullOrEmpty(fmsVideoId))
                fmsVideoId = Guid.NewGuid().ToString();

            if (deriveFmsVideoId.HasValue && deriveFmsVideoId.Value)
            {
                // This is an audio file .wav/.mp3
                fmsVideoId = DirectoryFileService.MakeNameAzureContainerNameSafe(Path.GetFileNameWithoutExtension(this._inputVideoFileNameOrUrl));
            }

            var parentFolder = Path.Combine(hlsFolder, fmsVideoId);
            var audioFolder = parentFolder;// Path.Combine(parentFolder, fmsVideoId);

            var c = new HlsConversionResult { InputFile = _inputVideoFileNameOrUrl, fmsVideoId = fmsVideoId, LocalFolder = parentFolder };

            Logger.Trace($"[CONVERSION] {DS.Dictionary(new { c.InputFile, c.fmsVideoId, audioFolder }).Format()}", this);
            DirectoryFileService.CreateDirectory(parentFolder, deleteIfExists: true);

            // Generate the FFMPEG command line, up to 3 resolutions supported
            var sb = new StringBuilder();
            sb.Append($@"-i ""{_inputVideoFileNameOrUrl}"" -c:a aac -b:a 128k -f segment -segment_time 10 -segment_list ""{audioFolder}\audioplaylist.m3u8"" ");
            //sb.Append($@"-segment_format mpegts output%03d.ts");
            sb.Append($@"-segment_format mpegts ""{audioFolder}/audio%04d.ts""");
            // sb.Append($@"-hls_segment_filename ""{audioFolder}-%v/data%04d.ts"" -use_localtime_mkdir 1 ");
            c.FFMPEGCommandLine = sb.ToString();

            var exitCode = 0;
            var r = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            c.Success = r && exitCode == 0;

            var tsFileSize = 0L;
            if (c.Success)
            {
                // Finalize the HLS conversion
                FixPathInM3U8(parentFolder, audioFolder, fmsVideoId);
                (c.mu38MasterUrl, c.ThumbnailUrl, c.TsFileSize) = UploadToAzureStorage(this._inputVideoFileNameOrUrl, null, parentFolder, fmsVideoId, azureStorageConnectionString, cdnHost);
            }
            else
            {
                DirectoryFileService.DeleteDirectory(parentFolder);
            }

            Logger.Trace($"[SUMMARY] {c.ToJson()}", this);
            Logger.Trace($@"[JAVASCRIPT] const cdn_url = ""{c.mu38MasterUrl}""; // {Path.GetFileName(c.InputFile)}", this);
            Logger.Trace($@"mu38MasterUrl: ({c.mu38MasterUrl})", this);

            return c;
        }

        public const int CONVERSION_MAX_RESOLUTIONS = 3;

        // https://www.youtube.com/watch?v=xJQBnrJXyv4 Download HLS Streaming Video with PowerShell and FFMPEG
        public HlsConversionResult ConvertVideoToHls(string hlsFolder, string ffmepexe, string azureStorageConnectionString, List<string> resolutionKeys, string cdnHost, string fmsVideoId = null, bool? deriveFmsVideoId = null)
        {
            Logger.Trace($"");
            Logger.Trace($"[CONVERSION][VIDEO] Start - {this.GetVideoInfo()}");

            // fmdVideoId initialization, different mode possible
            if (string.IsNullOrEmpty(fmsVideoId))
                fmsVideoId = Guid.NewGuid().ToString();

            if(deriveFmsVideoId.HasValue && deriveFmsVideoId.Value)
            {
                fmsVideoId = DirectoryFileService.MakeNameAzureContainerNameSafe(Path.GetFileNameWithoutExtension(this._inputVideoFileNameOrUrl));
            }

            var parentFolder = Path.Combine(hlsFolder, fmsVideoId);
            var videoFolder = Path.Combine(parentFolder, fmsVideoId);
            var resolutions = new List<VideoResolution>();
            var skippedResolutions = new List<VideoResolution>();
            var c = new HlsConversionResult { InputFile = _inputVideoFileNameOrUrl, fmsVideoId = fmsVideoId, LocalFolder = parentFolder, Resolutions = resolutions };

            Logger.Trace($"[CONVERSION] {DS.Dictionary(new { c.InputFile, c.fmsVideoId, videoFolder }).Format()}", this);
            DirectoryFileService.CreateDirectory(parentFolder, deleteIfExists: true);

            // Analyse which resolution can be applied to video
            foreach(var rk in resolutionKeys)
            {
                if (VideoResolution.VideoResolutions.ContainsKey(rk))
                {
                    var videoResolution = VideoResolution.VideoResolutions[rk];
                    if (videoResolution.CanVideoBeConvertedToResolution(this.Width, this.Height))
                    {
                        resolutions.Add(videoResolution);
                        if(resolutions.Count == CONVERSION_MAX_RESOLUTIONS) // for now we only support 3 resolutions in the ffmpeg command line, could be extended
                            break;
                    }
                    else skippedResolutions.Add(videoResolution);
                }
            }

            Logger.Trace($"[RESOLUTIONS]{string.Join(", ", resolutions.Select(rso => rso.Name))}", this);
            Logger.Trace($"[SKIPPED.RESOLUTIONS]{string.Join(", ", skippedResolutions.Select(rso => rso.Name))}", this);

            // Generate the FFMPEG command line, up to 3 resolutions supported

            var sb = new StringBuilder();
            sb.Append($@"-i ""{_inputVideoFileNameOrUrl}"" ");

            // Define how many resolutions we are going to create
            //sb.Append($@"-filter_complex ""[0:v]split=2[v1][v2]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]"" ");
            if(resolutions.Count == 1)
                sb.Append($@"-filter_complex ""[0:v]split=1[v1]; ");
            else if (resolutions.Count == 2)
                sb.Append($@"-filter_complex ""[0:v]split=2[v1][v2]; ");
            else if (resolutions.Count == 3)
                sb.Append($@"-filter_complex ""[0:v]split=3[v1][v2][v3]; ");

            var resolutionIndex = 1; // Resolution 1 if the video as is
            ///sb.Append($@" [v1]copy[v1out]; ");
            for(var re = 0; re < resolutions.Count; re++)
            {
                resolutionIndex = re+1;
                sb.Append($@" {resolutions[resolutionIndex - 1].GetScaleResolutionCmd(resolutionIndex, isLast: re == resolutions.Count-1)} ");
            }
            sb.Append($@""" ");
            //sb.Append($@"-map ""[v1out]"" -c:v:0 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 5M -preset ""{preset}"" -g 48 -sc_threshold 0 -keyint_min 48 ");
            //sb.Append($@"-map ""[v2out]"" -c:v:1 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset ""{preset}"" -g 48 -sc_threshold 0 -keyint_min 48 ");

            for (var re = 0; re < resolutions.Count; re++)
                sb.Append(resolutions[re].GetVideoMapCmd(re+1));

            for (var re = 0; re < resolutions.Count; re++)
                sb.Append(resolutions[re].GetAudioMapCmd(re + 1)); // todo increase quality of audio
            //sb.Append($@"-map a:0 -c:a:0 aac -b:a:0 128k -ac 2 ");
            //sb.Append($@"-map a:0 -c:a:1 aac -b:a:1 128k -ac 2 ");

            var hls_time = resolutions[0].SegmentDurationSeconds;

            // Hls configuration
            // todo: segment size should be configurable and 10s
            sb.Append($@"-f hls -hls_time {hls_time} -hls_playlist_type vod ");
            sb.Append($@"-hls_flags independent_segments -hls_segment_type mpegts ");
            sb.Append($@"-hls_segment_filename ""{videoFolder}-%v/data%04d.ts"" -use_localtime_mkdir 1 ");
            sb.Append($@"-master_pl_name ""{masterM3u8Filemame}"" ");

            sb.Append($@"-var_stream_map """); //sb.Append($@"-var_stream_map ""v:0,a:0 v:1,a:1"" ");
            for (var re = 0; re < resolutions.Count; re++)
                sb.Append(resolutions[re].GetFFMPEGStreamMap(re));
            sb.Remove(sb.Length - 1, 1);
            sb.Append($@""" ");
            sb.Append($@" ""{videoFolder}-%v.m3u8"" ");
            c.FFMPEGCommandLine = sb.ToString();

            var exitCode = 0;
            var r = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            c.Success = r && exitCode == 0;

            var tsFileSize = 0L;
            if (c.Success)
            {
                // Finalize the HLS conversion
                FixPathInM3U8(parentFolder, videoFolder, fmsVideoId);
                var thumbnailLocalFile = GetVideoThumbnail(this._inputVideoFileNameOrUrl);
                (c.mu38MasterUrl, c.ThumbnailUrl, c.TsFileSize) = UploadToAzureStorage(this._inputVideoFileNameOrUrl, thumbnailLocalFile, parentFolder, fmsVideoId, azureStorageConnectionString, cdnHost);
            }
            else 
            {
                DirectoryFileService.DeleteDirectory(parentFolder);
            }
            
            Logger.Trace($"[SUMMARY] {c.ToJson()}", this);
            Logger.Trace($@"[JAVASCRIPT] const cdn_url = ""{c.mu38MasterUrl}""; // {Path.GetFileName(c.InputFile)}", this);
            Logger.Trace($@"mu38MasterUrl: ({c.mu38MasterUrl})", this);
            Logger.Trace($@"ThumbnailUrl: ({c.ThumbnailUrl})", this);

            return c;
        }

        public string GetVideoThumbnail(string videoFileName)
        {
            var thumbnailFileName = new TestFileHelper().GetTempFileName("jpg");
            var ffMpeg = new FFMpegConverter();
            ffMpeg.GetVideoThumbnail(videoFileName, thumbnailFileName, 5);
            return thumbnailFileName;
        }

        private string TraceUploading(string fileName)
        {
            var fileSize = new FileInfo(fileName).Length;
            Logger.Trace($"Uploading to Azure: {fileName} - {fileSize/1024} KB", this);
            return fileName;
        }

        private (string, string, long) UploadToAzureStorage(string orginalVideo, string thumbnailLocalFile, string parentFolder, string fmsVideoId, string azureStorageConnectionString, string cdnHost)
        {
            var masterM3U8Exists = File.Exists(Path.Combine(parentFolder, masterM3u8Filemame));
            var audioPlayListM3U8Exists = File.Exists(Path.Combine(parentFolder, AudioPlayListM3u8Filemame));
            
            var thumbnailBlobName = "thumbnail.jpg";
            var thumbnailBlobNameExists = File.Exists(thumbnailLocalFile);
            Logger.Trace($"Uploading to Azure fmsVideoId:{fmsVideoId}", this);
            var containerName = fmsVideoId;
            var r = string.Empty;
            var bm = new BlobManager(azureStorageConnectionString);
            bm.DeleteBlobContainer(fmsVideoId, waitAfterDeletion: true).GetAwaiter().GetResult();
            bm.CreateBlobContainer(fmsVideoId).GetAwaiter().GetResult();

            // Upload the original video as url or as local file
            var orginalVideoBlobName = Path.GetFileName(orginalVideo);
            if (DirectoryFileService.IsUrl(orginalVideo))
            {
                var localorginalVideo = DirectoryFileService.DownloadFile(orginalVideo);
                bm.UploadBlobStreamAsync(containerName, orginalVideoBlobName, File.OpenRead(TraceUploading(localorginalVideo)), DirectoryFileService.GetContentType(localorginalVideo)).GetAwaiter().GetResult();
            }
            else
            {
                bm.UploadBlobStreamAsync(containerName, orginalVideoBlobName, File.OpenRead(TraceUploading(orginalVideo)), DirectoryFileService.GetContentType(orginalVideo)).GetAwaiter().GetResult();
            }

            if (thumbnailBlobNameExists) // Upload the thumbnail
            {
                bm.UploadBlobStreamAsync(containerName, thumbnailBlobName, File.OpenRead(TraceUploading(thumbnailLocalFile)), DirectoryFileService.GetContentType(orginalVideo)).GetAwaiter().GetResult();
            }

            // Upload the m3u8 files
            var files = Directory.GetFiles(parentFolder, "*.m3u8").ToList();
            foreach (var f in files)
            {
                var blobName = Path.GetFileName(f);
                bm.UploadBlobStreamAsync(containerName, blobName, File.OpenRead(TraceUploading(f)), DirectoryFileService.GetContentType(f)).GetAwaiter().GetResult();
            }

            var thumbnailUrlFromCDN = string.Empty;
            if (thumbnailBlobNameExists)
            {
                var thumbnailUrl = bm.GetBlobURL(containerName, thumbnailBlobName).ToString(); // Get the URL for the thumbnail
                thumbnailUrlFromCDN = DirectoryFileService.RemoveQueryStringFromUri(DirectoryFileService.ReplaceHostInUri(thumbnailUrl, cdnHost));
            }

            var tsFilesSize = 0L;
            var masterUrlFromCDN = string.Empty;
            if (masterM3U8Exists)
            {
                var masterUrlDirectFromStorage = bm.GetBlobURL(containerName, masterM3u8Filemame).ToString(); // get the URL for the master.m3u8
                masterUrlFromCDN = DirectoryFileService.RemoveQueryStringFromUri(DirectoryFileService.ReplaceHostInUri(masterUrlDirectFromStorage, cdnHost));
                var resolutionFolders = files.Select(ff => Path.GetFileNameWithoutExtension(ff)).ToList().Filter(f => !f.Contains("master")).ToList();

                foreach (var rf in resolutionFolders)
                {
                    var filesInResolution = Directory.GetFiles(parentFolder, $"{rf}\\*.ts").ToList();
                    foreach (var f in filesInResolution)
                    {
                        tsFilesSize += new FileInfo(f).Length;
                        var blobName = Path.Combine(rf, Path.GetFileName(f));
                        bm.UploadBlobStreamAsync(containerName, blobName, File.OpenRead(TraceUploading(f)), DirectoryFileService.GetContentType(f)).GetAwaiter().GetResult();
                    }
                }
            }
            else if(audioPlayListM3U8Exists) // Audio file have 1 resolutions located in the root folder
            {
                var masterUrlDirectFromStorage = bm.GetBlobURL(containerName, AudioPlayListM3u8Filemame).ToString(); // get the URL for the master.m3u8
                masterUrlFromCDN = DirectoryFileService.RemoveQueryStringFromUri(DirectoryFileService.ReplaceHostInUri(masterUrlDirectFromStorage, cdnHost));

                var audioTsFiles = Directory.GetFiles(parentFolder, $"*.ts").ToList();
                foreach (var f in audioTsFiles)
                {
                    tsFilesSize += new FileInfo(f).Length;
                    var blobName = Path.Combine(Path.GetFileName(f));
                    bm.UploadBlobStreamAsync(containerName, blobName, File.OpenRead(TraceUploading(f)), DirectoryFileService.GetContentType(f)).GetAwaiter().GetResult();
                }
            }

            return (masterUrlFromCDN, thumbnailUrlFromCDN, tsFilesSize);
        }

        public GifToMp4ConversionResult ConvertGifToMp4(string mp4FileName, int bitRateKb, string ffmepexe)
        {
            using (var tfh = new TestFileHelper()) 
            {
                tfh.DeleteFile(mp4FileName);

                var r = new GifToMp4ConversionResult { InputFile = this._inputVideoFileNameOrUrl };
                var sb = new StringBuilder();
                sb.Append($@"-i ""{this._inputVideoFileNameOrUrl}"" -movflags faststart -pix_fmt yuv420p -vf ""scale=trunc(iw/2)*2:trunc(ih/2)*2"" ""{mp4FileName}"" ");

                // sb.Append($@" -f gif -i ""{this._inputVideoFileNameOrUrl}"" ""{mp4FileName}"" ");
                //sb.Append($@" -i ""{this._inputVideoFileNameOrUrl}"" -c:v libvpx -crf 12 -b:v 500K ""{mp4FileName}"" ");
                // ffmpeg -f gif -i infile.gif outfile.mp4

                r.FFMPEGCommandLine = sb.ToString();
                var exitCode = 0;
                var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
                r.Success = rr && exitCode == 0;
                r.Mp4FileName = mp4FileName;
                r.Done();

                if (r.Success)
                {
                    var r2 = FFMPEG.AddBlankAudioTrack(mp4FileName, ffmepexe, tfh);
                    if (r2.Success)
                    {
                        var r3 = FFMPEG.ChangeBitrate(r2.Mp4FileName, bitRateKb, ffmepexe, tfh);
                        if(r3.Success)
                        {
                            File.Copy(r3.Mp4FileName, mp4FileName, true);
                        }
                        else
                        {
                            r.Success = false;
                        }
                    }
                    else
                    {
                        r.Success = false;
                    }
                }
                else
                {
                }
                Logger.Trace($"{r.ToJson()}", this);

                return r;
            }
        }

    }
}
