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

namespace fms
{
    public partial class VideoManager
    {
        public string _inputVideoFileNameOrUrl { get; }
        private MediaInfo _mediaInfo { get; set; }
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
            sb.Append($"file: {this._inputVideoFileNameOrUrl}").AppendLine();

            var fileSize = new FileInfo(this._inputVideoFileNameOrUrl).Length;
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


        // https://www.youtube.com/watch?v=xJQBnrJXyv4 Download HLS Streaming Video with PowerShell and FFMPEG
        public HlsConversionResult ConvertToHls(string hlsFolder, string ffmepexe, string azureStorageConnectionString, List<string> resolutionKeys, string cdnHost, string fmsVideoId = null, bool? deriveFmsVideoId = null)
        {
            if (string.IsNullOrEmpty(fmsVideoId))
                fmsVideoId = Guid.NewGuid().ToString();

            if(deriveFmsVideoId.HasValue && deriveFmsVideoId.Value)
            {
                fmsVideoId = MakeNameAzureContainerNameSafe(Path.GetFileNameWithoutExtension(this._inputVideoFileNameOrUrl));
            }

            var parentFolder = Path.Combine(hlsFolder, fmsVideoId);
            var videoFolder = Path.Combine(parentFolder, fmsVideoId);
            var resolutions = new List<VideoResolution>();
            var c = new HlsConversionResult { InputFile = _inputVideoFileNameOrUrl, fmsVideoId = fmsVideoId, LocalFolder = parentFolder, Resolutions = resolutions };

            Logger.Trace($"[CONVERSION] InputFile: {c.InputFile}", this);
            Logger.Trace($"[CONVERSION] fmsVideoId: {c.fmsVideoId}", this);

            new TestFileHelper().DeleteDirectory(parentFolder);
            Directory.CreateDirectory(parentFolder);

            foreach(var rk in resolutionKeys)
            {
                if (VideoResolution.VideoResolutions.ContainsKey(rk))
                {
                    var videoResolution = VideoResolution.VideoResolutions[rk];
                    if (this.Height >= videoResolution.Height)
                        resolutions.Add(videoResolution);
                }
            }

            Logger.Trace($"[RESOLUTIONS]{string.Join(", ", resolutions.Select(rso => rso.Name))}", this);

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
            sb.Append($@"-master_pl_name ""master.m3u8"" ");

            sb.Append($@"-var_stream_map """); //sb.Append($@"-var_stream_map ""v:0,a:0 v:1,a:1"" ");
            for (var re = 0; re < resolutions.Count; re++)
                sb.Append(resolutions[re].GetFFMPEGStreamMap(re));
            sb.Remove(sb.Length - 1, 1);
            sb.Append($@""" ");
            sb.Append($@" ""{videoFolder}-%v.m3u8"" ");
            Logger.Trace($"{sb}", this);
            c.FFMPEGCommandLine = sb.ToString();

            var exitCode = 0;
            var r = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);

            c.Success = r && exitCode == 0;

            if (c.Success)
            {
                FixPathInM3U8(parentFolder, videoFolder, fmsVideoId);
                var thumbnailLocalFile = GetVideoThumbnail(this._inputVideoFileNameOrUrl);
                (c.mu38MasterUrl, c.ThumbnailUrl) = UploadToAzureStorage(this._inputVideoFileNameOrUrl, thumbnailLocalFile, parentFolder, fmsVideoId, azureStorageConnectionString, cdnHost);
                
            }
            else 
            {
                DirectoryService.DeleteDirectory(parentFolder);
            }
            Logger.Trace($"{c.ToJson()}", this);
            Logger.Trace($"[SUMMARY] {DS.Dictionary(new { c.InputFile, c.fmsVideoId, c.Duration, c.mu38MasterUrl }).Format(preFix:"", postFix:"")}", this);
            Logger.Trace($@"[JAVASCRIPT] const cdn_url = ""{c.mu38MasterUrl}""; // {Path.GetFileName(c.InputFile)}", this);
            Logger.Trace($@"mu38MasterUrl (""{c.mu38MasterUrl}"")", this);
            Logger.Trace($@"ThumbnailUrl (""{c.ThumbnailUrl}"")", this);

            return c;
        }

        const int MAX_CONTAINER_NAME_LENGTH = 63;
        private string MakeNameAzureContainerNameSafe(string v)
        {
            var vv = v.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach(var c in vv)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else 
                    sb.Append("-");
            }

            var rr = sb.ToString();
            rr = rr.Replace("--", "-");
            rr = rr.Substring(0, Math.Min(MAX_CONTAINER_NAME_LENGTH, rr.Length));

            return rr;
        }

        public string GetVideoThumbnail(string videoFileName)
        {
            var thumbnailFileName = new TestFileHelper().GetTempFileName("jpg");
            var ffMpeg = new FFMpegConverter();
            ffMpeg.GetVideoThumbnail(videoFileName, thumbnailFileName, 5);
            return thumbnailFileName;
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case ".m3u8": return "application/vnd.apple.mpegurl";//"application/x-mpegURL";
                case ".ts"  : return "video/MP2T";
                default     : return "application/octet-stream";
            }
        }

        private static string ReplaceHostInUri(string originalUrl, string newHost)
        {
            Uri originalUri = new Uri(originalUrl);
            UriBuilder builder = new UriBuilder(originalUri);
            builder.Host = newHost;
            Uri newUri = builder.Uri;
            return newUri.ToString();
        }

        private static string RemoveQueryStringFromUri(string originalUrl)
        {
            Uri originalUri = new Uri(originalUrl);
            UriBuilder builder = new UriBuilder(originalUri);
            builder.Query = string.Empty;
            Uri newUri = builder.Uri;
            return newUri.ToString();
        }

        private (string, string ) UploadToAzureStorage(string orginalVideo, string thumbnailLocalFile, string parentFolder, string fmsVideoId, string azureStorageConnectionString, string cdnHost)
        {
            var thumbnailBlobName = "thumbnail.jpg";
            Logger.Trace($"Uploading to Azure fmsVideoId:{fmsVideoId}", this);
            var containerName = fmsVideoId;
            var r = string.Empty;
            var bm = new BlobManager(azureStorageConnectionString);
            bm.CreateBlobContainer(fmsVideoId).GetAwaiter().GetResult();

            // Upload the original video
            var orginalVideoBlobName = Path.GetFileName(orginalVideo);
            bm.UploadBlobStreamAsync(containerName, orginalVideoBlobName, File.OpenRead(orginalVideo), GetContentType(orginalVideo)).GetAwaiter().GetResult();
            bm.UploadBlobStreamAsync(containerName, thumbnailBlobName, File.OpenRead(thumbnailLocalFile), GetContentType(orginalVideo)).GetAwaiter().GetResult();

            var files = Directory.GetFiles(parentFolder, "*.m3u8").ToList();
            foreach (var f in files)
            {
                var blobName = Path.GetFileName(f);
                bm.UploadBlobStreamAsync(containerName, blobName, File.OpenRead(f), GetContentType(f)).GetAwaiter().GetResult();
            }

            var thumbnailUrl = bm.GetBlobURL(containerName, thumbnailBlobName).ToString();
            var thumbnailUrlFromCDN = ReplaceHostInUri(thumbnailUrl, cdnHost);
            thumbnailUrl = RemoveQueryStringFromUri(thumbnailUrl);

            var masterUrlDirectFromStorage = bm.GetBlobURL(containerName, "master.m3u8").ToString();
            var masterUrlFromCDN = ReplaceHostInUri(masterUrlDirectFromStorage, cdnHost);
            masterUrlFromCDN = RemoveQueryStringFromUri(masterUrlFromCDN);

            var resolutionFolders = files.Select(ff => Path.GetFileNameWithoutExtension(ff)).ToList().Filter(f => !f.Contains("master")).ToList();

            foreach(var rf in resolutionFolders)
            {
                var filesInResolution = Directory.GetFiles(parentFolder, $"{rf}\\*.ts").ToList();
                foreach (var f in filesInResolution)
                {
                    var blobName = Path.Combine(rf, Path.GetFileName(f));
                    bm.UploadBlobStreamAsync(containerName, blobName, File.OpenRead(f), GetContentType(f)).GetAwaiter().GetResult();
                }
            }
            return (masterUrlFromCDN, thumbnailUrlFromCDN);
        }

        public GifToMp4ConversionResult ConvertGifToMp4(string mp4FileName, int bitRateKb, string ffmepexe)
        {
            using (var tfh = new TestFileHelper()) {

                tfh.DeleteFile(mp4FileName);

                var r = new GifToMp4ConversionResult { InputFile = this._inputVideoFileNameOrUrl };
                var sb = new StringBuilder();
                sb.Append($@"-i ""{this._inputVideoFileNameOrUrl}"" -movflags faststart -pix_fmt yuv420p -vf ""scale=trunc(iw/2)*2:trunc(ih/2)*2"" ""{mp4FileName}"" ");

                // sb.Append($@" -f gif -i ""{this._inputVideoFileNameOrUrl}"" ""{mp4FileName}"" ");
                //sb.Append($@" -i ""{this._inputVideoFileNameOrUrl}"" -c:v libvpx -crf 12 -b:v 500K ""{mp4FileName}"" ");
                                //  

                // ffmpeg -f gif -i infile.gif outfile.mp4

                r.FFMPEGCommandLine = sb.ToString();
                var exitCode = 0;
                var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
                r.Success = rr && exitCode == 0;
                r.Mp4FileName = mp4FileName;
                r.Done();

                if (r.Success)
                {
                    var r2 = AddBlankAudioTrack(mp4FileName, ffmepexe, tfh);
                    if (r2.Success)
                    {
                        var r3 = ChangeBitrate(r2.Mp4FileName, bitRateKb, ffmepexe, tfh);
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

        public AddAudioTrackResult AddBlankAudioTrack(string mp4FileName, string ffmepexe, TestFileHelper tfh)
        {
            var mp4FileNameWithAudio = tfh.GetTempFileName("mp4");
            var r = new AddAudioTrackResult { InputFile = mp4FileName };
            var sb = new StringBuilder();
            r.Mp4FileName = mp4FileNameWithAudio;
            sb.Append($@"-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -i ""{mp4FileName}"" -c:v copy -c:a aac -shortest ""{mp4FileNameWithAudio}"" ");
            r.FFMPEGCommandLine = sb.ToString();
            var exitCode = 0;
            var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            r.Success = rr && exitCode == 0;
            r.Done();

            if (r.Success)
            {
            }
            else
            {
            }
            Logger.Trace($"{r.ToJson()}", this);

            return r;
        }

        public ChangeBitRateConversionResult ChangeBitrate(string mp4FileName, int bitRateKb, string ffmepexe, TestFileHelper tfh)
        {
            var mp4FileNameH265 = tfh.GetTempFileName("mp4");
            var r = new ChangeBitRateConversionResult { InputFile = mp4FileName };
            var sb = new StringBuilder();
            r.Mp4FileName = mp4FileNameH265;
            sb.Append($@" -i ""{mp4FileName}""  -b {bitRateKb}k  ""{mp4FileNameH265}"" ");
            r.FFMPEGCommandLine = sb.ToString();
            var exitCode = 0;
            var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            r.Success = rr && exitCode == 0;
            r.Done();
            if (r.Success)
            {
            }
            else
            {
            }
            Logger.Trace($"{r.ToJson()}", this);

            return r;
        }

        public H265ConversionResult ConvertToH265(string mp4FileName, string ffmepexe, TestFileHelper tfh)
        {
            var mp4FileNameH265 = tfh.GetTempFileName("mp4");
            var r = new H265ConversionResult { InputFile = mp4FileName };
            var sb = new StringBuilder();
            r.Mp4FileName = mp4FileNameH265;

            sb.Append($@" -i ""{mp4FileName}"" -vcodec libx265 -crf 28 ""{mp4FileNameH265}"" ");

            r.FFMPEGCommandLine = sb.ToString();
            var exitCode = 0;
            var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            r.Success = rr && exitCode == 0;
            r.Done();

            if (r.Success)
            {
            }
            else
            {
            }
            Logger.Trace($"{r.ToJson()}", this);

            return r;
        }
    }
}
