using System.Collections.Generic;
using M3U8Parser.ExtXType;
using System.Text.Json.Serialization;
using System.Text;
using System;
using System.Runtime.CompilerServices;

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
        public string ResolutionFile { get; set; }
        public string ResolutionDefinition { get; set; }
        public List<TsFileInfo> TsLocalFiles { get; set; } = new List<TsFileInfo>();

        public void CreateFFMPEGConcatFile(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"#fms {DateTime.Now} - ffconcat version 1.0");

            foreach (var ts in TsLocalFiles)
                sb.AppendLine($"file '{ts.LocalFile}'");

            System.IO.File.WriteAllText(fileName, sb.ToString());
        }

        public string ToJSON()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

    public class OperationResult
    {
        public Exception Ex { get; set; } = null;
        public string ExMethodName { get; set; } = string.Empty;
        public bool Succeeded => this.Ex == null;

        public void SetException(Exception ex, [CallerMemberName] string methodName = null)
        {
            this.Ex = ex;
            this.ExMethodName = methodName;
        }

        public string ToJSON()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

    public class DownloadedVideoResult : OperationResult
    {
        public string LocalFolder { get; set; }
        public string m3u8MasterUrl { get; set; }
        public string fmsVideoId { get; set; }
        [JsonIgnore]
        public StreamInf Stream { get; set; }
        public List<DownloadedVideoResolutionInfo> Resolutions { get; set; } = new List<DownloadedVideoResolutionInfo>();
    }
}
