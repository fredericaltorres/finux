﻿using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Security.Policy;
using System.IO;
using M3U8Parser;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using M3U8Parser.Attributes;
using M3U8Parser.ExtXType;
using System;

namespace fms
{
    public class HlsManager
    {
        private static WebClient _client = new WebClient();
        private readonly string _hlsMasterM3U8Url;

        private MasterPlaylist _masterPlayList;
        
        public string m3u8MasterFile = Path.Combine(@"c:\temp", "master.m3u8");

        public List<string> m3u8PlayListUrls { get; set; } = new List<string>();

        public HlsManager(string hlsMasterM3U8Url)
        {
            this._hlsMasterM3U8Url = hlsMasterM3U8Url;
            DirectoryFileService.DeleteFile(m3u8MasterFile);
            this.DownloadHlsFile(this._hlsMasterM3U8Url, this.m3u8MasterFile);
            this.LoadMasterM3U8();
        }

        public void DownloadM3U8ResolutionFiles(string m3u8PlayListUrl, int resolutionIndex, string outputFolder, DownloadInfo downloadInfo)
        {
            var hlsRootUrl = GetMasterRelativePath(m3u8PlayListUrl);
            var resolutionFileName = GetLastDirectory(m3u8PlayListUrl);

            var m3u8MResolutionFile = Path.Combine(outputFolder, resolutionFileName);
            this.DownloadHlsFile(m3u8PlayListUrl, m3u8MResolutionFile);

            var m = M3U8Parser.MediaPlaylist.LoadFromFile(m3u8MResolutionFile);
            var mediaSegment = m.MediaSegments[0];

            foreach (var segment in mediaSegment.Segments)
            {
                var subFolder = Path.GetDirectoryName(segment.Uri);
                subFolder = Path.Combine(outputFolder, subFolder);
                if(!Directory.Exists(subFolder))
                    DirectoryFileService.CreateDirectory(subFolder);

                var tsFileName = Path.GetFileName(segment.Uri);
                var localSegmentFileName = Path.Combine(subFolder, tsFileName);
                var segmentUrl = $@"{hlsRootUrl}/{segment.Uri}";

                this.DownloadHlsFile(segmentUrl, localSegmentFileName);
                downloadInfo.TsUrlList.Add(segmentUrl);
            }
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
            // todo should extract the query string by using a Uri
            var x = url.LastIndexOf('/');
            return url.Substring(0, x);
        }

        private string GetLastDirectory(string url)
        {
            // todo should extract the query string by using a Uri
            var x = url.LastIndexOf('/');
            return url.Substring(x+1);
        }

        private string GetVideoFmsVideoIdFromMasterUrl(string url)
        {
            var p = GetMasterRelativePath(url);
            var pp = GetLastDirectory(p);
            return pp;
        }

        public void LoadMasterM3U8()
        {
            m3u8PlayListUrls.Clear();
            var hlsRootUrl = GetMasterRelativePath(this._hlsMasterM3U8Url);
            _masterPlayList = MasterPlaylist.LoadFromFile(this.m3u8MasterFile);
            
            foreach (var s in _masterPlayList.Streams)
            {
                m3u8PlayListUrls.Add($"{hlsRootUrl}/{s.Uri}");
            }
        }

        public class DownloadInfo
        {
            public string LocalFolder { get; set; }
            public string m3u8MasterUrl { get; set; }
            public string fmsVideoId { get; set; }
            public List<string> m3u8PlayListUrls { get; set; } = new List<string>();
            public List<string> TsUrlList { get; set; } = new List<string>();

            public string ToJSON()
            {
                return System.Text.Json.JsonSerializer.Serialize(this);
            }
        }

        public DownloadInfo DownloadHlsAssets(string outputFolder)
        {
            var r = new DownloadInfo { m3u8MasterUrl = this._hlsMasterM3U8Url, m3u8PlayListUrls = this.m3u8PlayListUrls };

            this.LoadMasterM3U8();
            r.fmsVideoId = GetVideoFmsVideoIdFromMasterUrl(this._hlsMasterM3U8Url);
            var text = _masterPlayList.ToString();
            var localFolder = r.LocalFolder = Path.Combine(outputFolder, r.fmsVideoId);
            DirectoryFileService.CreateDirectory(localFolder);

            var localMasterFileName = Path.Combine(localFolder, "master.m3u8");
            File.AppendAllText(localMasterFileName, text);

            var resolutionIndex = 0;
            foreach (var resolutionUrl in this.m3u8PlayListUrls)
            {
                DownloadM3U8ResolutionFiles(resolutionUrl, resolutionIndex, localFolder, r);
                resolutionIndex++;
            }
            return r;
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
            foreach (var resolution in m3u8PlayListUrls)
            {
                GetResolutionM3u8Info(resolution, resolutionIndex, sb);
                resolutionIndex++;
            }

            return sb.ToString();
        }

        public string DownloadHlsFile(string hlsM3U8Url, string m3u8OutputFile)
        {
            DirectoryFileService.DeleteFile(m3u8OutputFile);
            _client.DownloadFile(hlsM3U8Url, m3u8OutputFile);
            return m3u8OutputFile;
        }
    }
}
