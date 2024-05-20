using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Security.Policy;
using System.IO;
using M3U8Parser;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using M3U8Parser.Attributes;

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

        public string GetResolutionM3u8Info(string m3u8ResolutionUrl, int resolutionIndex, StringBuilder sb)
        {
            var hlsRootUrl = m3u8ResolutionUrl.Replace(".m3u8",""); // remove extention of the resolution .m3u8 file to get the sub directory with the .ts files

            var m3u8MResolutionFile = Path.Combine(@"c:\temp", $"resolution_{resolutionIndex}.m3u8");
            this.DownloadHlsFile(m3u8ResolutionUrl, m3u8MResolutionFile);

            sb.AppendLine($"m3u8 Resolution[{resolutionIndex}] ({m3u8ResolutionUrl})");
            var m = M3U8Parser.MediaPlaylist.LoadFromFile(m3u8MResolutionFile);
            sb.AppendLine($"HlsVersion: {m.HlsVersion}");
            var mediaSegment = m.MediaSegments[0]; // name is confusing segments are really resolutions

            sb.AppendLine($"Segments.Count: {mediaSegment.Segments.Count}");

            foreach (var segment in mediaSegment.Segments)
            {
                var ss = $"[segment:{resolutionIndex}]";
                sb.Append($"{ss} Duration: {segment.Duration}, ");

                // Generally not defined
                //sb.Append($"{ss} ByteRangeLentgh: {segment.ByteRangeLentgh}, ");
                //sb.Append($"{ss} ByteRangeStartSubRange: {segment.ByteRangeStartSubRange}, ");
                //sb.Append($"{ss} Title: {segment.Title}, ");

                sb.Append($"{ss} Uri: {segment.Uri}, ").AppendLine();
                sb.Append($"{ss} FullUri: ({hlsRootUrl}\\{segment.Uri}), ").AppendLine();
            }
            return sb.ToString();
        }

        private string GetMasterRelativePath(string url)
        {
            var x = url.LastIndexOf('/');
            return url.Substring(0, x);
        }


        public void Analyse()
        {
            var hlsRootUrl = GetMasterRelativePath(this._hlsMasterM3U8Url);
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
            sb.AppendLine($"Master ({_hlsMasterM3U8Url})");
            sb.AppendLine($"HlsVersion: {_masterPlayList.HlsVersion}, Resolution.Count: {_masterPlayList.Streams.Count}");
            var resolutionIndex = 0;

            foreach (var s in _masterPlayList.Streams)
            {
                var ss = $"[resolution:{resolutionIndex}]";
                sb.Append($"{ss} Width:{s.Resolution.Width}, ");
                sb.Append($"Height:{s.Resolution.Height}, ");
                sb.Append($"Bandwidth:{s.Bandwidth}, ");
                sb.Append($"FrameRate:{s.FrameRate}, ");
                sb.Append($"Codecs: {s.Codecs}, ");
                sb.Append($"{ss} Uri:{s.Uri}").AppendLine();

                var fullUri = $"{hlsRootUrl}\\{s.Uri}";
                sb.Append($"{ss} ({fullUri})").AppendLine();

                resolutionIndex++;
            }

            sb.AppendLine();
            resolutionIndex = 0;
            foreach (var resolution in ResolutionM3u8Urls)
            {
                GetResolutionM3u8Info(resolution, resolutionIndex, sb);
                resolutionIndex++;
            }

            return sb.ToString();
        }

        static WebClient _client = new WebClient();

        public string DownloadHlsFile(string hlsM3U8Url, string m3u8OutputFile)
        {
            DirectoryService.DeleteFile(m3u8OutputFile);
            var text = _client.DownloadString(hlsM3U8Url);
            File.AppendAllText(m3u8OutputFile, text);
            return m3u8OutputFile;
        }
    }
}
