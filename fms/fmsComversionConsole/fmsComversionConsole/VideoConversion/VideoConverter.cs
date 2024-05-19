using System;
using DynamicSugar;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fms
{
    public class VideoConverter
    {
        public string _videoFileName { get; }

        public VideoConverter(string videoFileName)
        {
            _videoFileName = videoFileName;
        }

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
    }
}
