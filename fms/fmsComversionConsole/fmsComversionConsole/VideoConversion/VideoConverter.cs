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

namespace fms
{
    public class VideoConverter
    {
        public string _videoFileName { get; }

        public VideoConverter(string videoFileName)
        {
            _videoFileName = videoFileName;
        }

        // https://www.nrecosite.com/video_info_net.aspx
        public string GetVideoInfo()
        {
            var ffProbe = new NReco.VideoInfo.FFProbe();
            var v = ffProbe.GetMediaInfo(_videoFileName);
            var sb = new StringBuilder();
            sb.Append($"file: {this._videoFileName}").AppendLine();
            sb.Append(DS.Dictionary(new { v.FormatName, v.Duration, StreamsCount = v.Streams.ToList().Count }).Format()).AppendLine();
            foreach (var s in v.Streams)
            {
                sb.Append(DS.Dictionary(new {s.CodecName, s.CodecType, s.FrameRate, s.Width, s.Height, s.PixelFormat }).Format()).AppendLine();
            }
            return sb.ToString();
        }

        public bool ConvertToHls(string hlsFolder, string ffmepexe)
        {
            var preset = "medium";
            var videoIdName = Guid.NewGuid().ToString();
            var videoFolder = Path.Combine(hlsFolder, videoIdName);
            Directory.CreateDirectory(videoFolder);

            videoFolder = Path.Combine(videoFolder, videoIdName);

            var sb = new StringBuilder();

            sb.Append($@"-i ""{_videoFileName}"" ");
            sb.Append($@"-filter_complex ""[0:v]split=2[v1][v2]; [v1]copy[v1out]; [v2]scale=w=960:h=540[v2out]"" ");
            sb.Append($@"-map ""[v1out]"" -c:v:0 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 5M -preset ""{preset}"" -g 48 -sc_threshold 0 -keyint_min 48 ");
            sb.Append($@"-map ""[v2out]"" -c:v:1 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset ""{preset}"" -g 48 -sc_threshold 0 -keyint_min 48 ");
            sb.Append($@"-map a:0 -c:a:0 aac -b:a:0 128k -ac 2 ");
            sb.Append($@"-map a:0 -c:a:1 aac -b:a:1 128k -ac 2 ");
            sb.Append($@"-f hls -hls_time 2 -hls_playlist_type vod ");
            sb.Append($@"-hls_flags independent_segments -hls_segment_type mpegts ");

            sb.Append($@"-hls_segment_filename ""{videoFolder}-%v/data%04d.ts"" -use_localtime_mkdir 1 ");

            sb.Append($@"-master_pl_name ""master.m3u8"" ");
            sb.Append($@"-var_stream_map ""v:0,a:0 v:1,a:1"" ""{videoFolder}-%v.m3u8""  ");

            var exitCode = 0;
            var r = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);

            return r && exitCode == 0;
        }
    }
}
