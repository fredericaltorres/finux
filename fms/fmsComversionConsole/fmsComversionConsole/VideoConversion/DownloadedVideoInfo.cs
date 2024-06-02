using System.Collections.Generic;
using M3U8Parser.ExtXType;
using System.Text.Json.Serialization;

namespace fms
{
    public class TsFileInfo
    {
        public string Url { get; set; }
        public string LocalFile { get; set; }
        public long FileSize { get; set; }
    }

    public class DownloadedVideoResolutionInfo
    {
        public string m3u8PlayListUrls { get; set; }
        public string ResolutionDefinition { get; set; }
        public List<TsFileInfo> TsLocalFiles { get; set; } = new List<TsFileInfo>();

        public string ToJSON()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

    public class DownloadedVideoInfo
    {
        public string LocalFolder { get; set; }
        public string m3u8MasterUrl { get; set; }
        public string fmsVideoId { get; set; }
        [JsonIgnore]
        public StreamInf Stream { get; set; }
        public List<DownloadedVideoResolutionInfo> Resolutions { get; set; } = new List<DownloadedVideoResolutionInfo>();

        public string ToJSON()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
