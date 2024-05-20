using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Security.Policy;
using System.IO;
using M3U8Parser;
using System.Text;
using System.Collections.Generic;

namespace fms
{
    public class HlsManager
    {
        private readonly string _hlsMasterM3U8Url;
        MasterPlaylist _masterPlayList;
        

        public string m3u8MasterFile = Path.Combine(@"c:\temp", "master.m3u8");

        public List<string> ResolutionM3u8Urls { get; set; } = new List<string>();

        public HlsManager(string hlsMasterM3U8Url)
        {
            this._hlsMasterM3U8Url = hlsMasterM3U8Url;
            DirectoryService.DeleteFile(m3u8MasterFile);
            this.DownloadHlsFile(this._hlsMasterM3U8Url, this.m3u8MasterFile);
            this.Analyse();
        }

        public string GetMasterFileInfo()
        {
            var m = M3U8Parser.MediaPlaylist.LoadFromFile(m3u8MasterFile);
            var sb = new StringBuilder();
            sb.AppendLine($"Master {_hlsMasterM3U8Url}");
            sb.AppendLine($"HlsVersion: {m.HlsVersion}");
            var streamIndex = 0;
            foreach (var resolution in m.MediaSegments)
            {
                var ss = $"[resolution:{streamIndex}]";
                sb.AppendLine($"{ss} key: {resolution.Key}");
                sb.AppendLine($"{ss} Segments.Count: {resolution.Segments.Count}");
            }
            return sb.ToString();
        }


        public void Analyse()
        {
            var hlsRootUrl = Path.GetDirectoryName(_hlsMasterM3U8Url);
            _masterPlayList = MasterPlaylist.LoadFromFile(this.m3u8MasterFile);
            
            foreach (var s in _masterPlayList.Streams)
            {
                ResolutionM3u8Urls.Add($"{hlsRootUrl}\\{s.Uri}");
            }
        }

        public string GetMasterInfo()
        {
            var hlsRootUrl = Path.GetDirectoryName(_hlsMasterM3U8Url);
            var sb = new StringBuilder();
            _masterPlayList = MasterPlaylist.LoadFromFile(this.m3u8MasterFile);
            sb.AppendLine($"Master {_hlsMasterM3U8Url}");
            sb.AppendLine($"HlsVersion: {_masterPlayList.HlsVersion}");
            var resolutionIndex = 0;
            foreach (var s in _masterPlayList.Streams)
            {
                var ss = $"[resolution:{resolutionIndex}]";
                sb.Append($"{ss} Width:{s.Resolution.Width}, ");
                sb.Append($"Height:{s.Resolution.Height}, ");
                sb.Append($"Bandwidth:{s.Bandwidth}, ");
                sb.Append($"FrameRate:{s.FrameRate}, ");
                sb.Append($"Codecs: {s.Codecs}").AppendLine();
                sb.AppendLine($"{ss} Uri:{s.Uri}").AppendLine();

                var fullUri = $"{hlsRootUrl}\\{s.Uri}";
                sb.AppendLine($"{ss} {fullUri}").AppendLine();
            }
            return sb.ToString();
        }

        static WebClient _client = new WebClient();

        public string DownloadHlsFile(string hlsM3U8Url, string m3u8OutputFile)
        {
            var text = _client.DownloadString(hlsM3U8Url);
            File.AppendAllText(m3u8OutputFile, text);
            return m3u8OutputFile;
        }
    }
}
