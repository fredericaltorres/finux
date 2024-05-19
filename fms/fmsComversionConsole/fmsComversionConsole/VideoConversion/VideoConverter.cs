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

namespace fms
{
    public class VideoConverter
    {
        public string _videoFileName { get; }
        private MediaInfo _mediaInfo { get; set; }
        public int Width => GetVideoStream.Width;
        public int Height => GetVideoStream.Height;

        public bool IsSquareResolution => Width == Height;

        public StreamInfo GetVideoStream => _mediaInfo.Streams.Where(s => s.CodecType == "video").FirstOrDefault();
        public StreamInfo GetAudioStream => _mediaInfo.Streams.Where(s => s.CodecType == "audio").FirstOrDefault();

        public VideoConverter(string videoFileName)
        {
            _videoFileName = videoFileName;
            var ffProbe = new NReco.VideoInfo.FFProbe();
            _mediaInfo = ffProbe.GetMediaInfo(_videoFileName);
            
        }

        // https://www.nrecosite.com/video_info_net.aspx
        public string GetVideoInfo()
        {
            var sb = new StringBuilder();
            sb.Append($"file: {this._videoFileName}").AppendLine();

            var v = _mediaInfo;
            sb.Append(DS.Dictionary(new { v.FormatName, v.Duration, StreamsCount = v.Streams.ToList().Count }).Format()).AppendLine();
            foreach (var s in v.Streams)
            {
                sb.Append(DS.Dictionary(new {s.CodecName, s.CodecType, s.FrameRate, s.Width, s.Height, s.PixelFormat }).Format()).AppendLine();
            }
            return sb.ToString();
        }

        // 1280 x 720 pixels 720p
        // 1920 × 1080 pixels 1080p

        public class VideoResolution
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string Name { get; set; }
            public int BitRateMegaBit { get; set; }
            public int KeyFrame { get; set; } = 48;
            public string Preset { get; set; } = "medium";

            public string GetFFMPEGMapResolution(int resolutionIndex)
            {
                return $@"-map ""[v{resolutionIndex}out]"" -c:v:0 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:0 {BitRateMegaBit}M -maxrate:v:0 {BitRateMegaBit}M -minrate:v:0 {BitRateMegaBit}M -bufsize:v:0 {BitRateMegaBit}M -preset ""{Preset}"" -g {KeyFrame} -sc_threshold 0 -keyint_min {KeyFrame}";
            }
            public string GetFFMPEGScaleResolution(int resolutionIndex, bool isLast)
            {
                return $@"[v{resolutionIndex}]scale=w={Width}:h={Height}[v{resolutionIndex}out]{(isLast ? "":";")}";
            }
            public bool IsSquareResolution => Width == Height;
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

        public static Dictionary<string, VideoResolution> VideoResolutions = new  Dictionary<string, VideoResolution>() 
        {
            ["1080p"] = new VideoResolution() { Width = 1920, Height = 1080, Name = "1080p", BitRateMegaBit = 5, Preset  = "medium", KeyFrame = 48 },
            ["1080x1080p"] = new VideoResolution() { Width = 1080, Height = 1080, Name = "1080x1080p", BitRateMegaBit = 5, Preset = "medium", KeyFrame = 48 },
            ["720p"]  = new VideoResolution() { Width = 1280, Height = 720, Name = "720p", BitRateMegaBit = 3, Preset  = "medium", KeyFrame = 48  },
            ["720x720p"] = new VideoResolution() { Width = 720, Height = 720, Name = "720x720p", BitRateMegaBit = 3, Preset = "medium", KeyFrame = 48 },
        };

        // https://www.youtube.com/watch?v=xJQBnrJXyv4 Download HLS Streaming Video with PowerShell and FFMPEG
        public bool ConvertToHls(string hlsFolder, string ffmepexe)
        {
            var preset = "medium";
            var videoIdName = Guid.NewGuid().ToString();
            var parentFolder = Path.Combine(hlsFolder, videoIdName);
            Directory.CreateDirectory(parentFolder);

            var videoFolder = Path.Combine(parentFolder, videoIdName);

            var resolutions = new List<VideoResolution>();

            if(this.IsSquareResolution)
            {
                if (this.Height >= 1080)
                    resolutions.Add(VideoResolutions["1080x1080p"]);

                if (this.Height >= 720)
                    resolutions.Add(VideoResolutions["720x720p"]);
            }
            else
            {
                if (this.Height >= 1080)
                    resolutions.Add(VideoResolutions["1080p"]);

                if (this.Height >= 720)
                    resolutions.Add(VideoResolutions["720p"]);
            }

            var sb = new StringBuilder();

            sb.Append($@"-i ""{_videoFileName}"" ");

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
                sb.Append($@" {resolutions[resolutionIndex - 1].GetFFMPEGScaleResolution(resolutionIndex, isLast: re == resolutions.Count-1)} ");
            }
            sb.Append($@""" ");


            sb.Append($@"-map ""[v1out]"" -c:v:0 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 5M -preset ""{preset}"" -g 48 -sc_threshold 0 -keyint_min 48 ");
            sb.Append($@"-map ""[v2out]"" -c:v:1 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset ""{preset}"" -g 48 -sc_threshold 0 -keyint_min 48 ");
            sb.Append($@"-map a:0 -c:a:0 aac -b:a:0 128k -ac 2 ");
            sb.Append($@"-map a:0 -c:a:1 aac -b:a:1 128k -ac 2 ");
            sb.Append($@"-f hls -hls_time 2 -hls_playlist_type vod ");
            sb.Append($@"-hls_flags independent_segments -hls_segment_type mpegts ");

            sb.Append($@"-hls_segment_filename ""{videoFolder}-%v/data%04d.ts"" -use_localtime_mkdir 1 ");

            sb.Append($@"-master_pl_name ""master.m3u8"" ");
            sb.Append($@"-var_stream_map ""v:0,a:0 v:1,a:1"" ""{videoFolder}-%v.m3u8""  ");

            Logger.Trace($"[CONVERTING] {sb}", this);

            var exitCode = 0;
            var r = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);

            var ok = r && exitCode == 0;

            if (ok)
            {
                FixPathInM3U8(parentFolder, videoFolder, videoIdName);
            }
            else 
            {
                DirectoryService.DeleteDirectory(parentFolder);
            }

            return ok;
        }
    }
}
