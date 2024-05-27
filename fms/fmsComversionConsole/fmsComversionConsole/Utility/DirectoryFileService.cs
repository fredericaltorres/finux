﻿using DynamicSugar;
using System;
using System.IO;

namespace fms
{
    public class DirectoryFileService
    {
        public static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case ".m3u8": return "application/vnd.apple.mpegurl";//"application/x-mpegURL";
                case ".ts": return "video/MP2T";
                default: return "application/octet-stream";
            }
        }

        public static string ReplaceHostInUri(string originalUrl, string newHost)
        {
            Uri originalUri = new Uri(originalUrl);
            UriBuilder builder = new UriBuilder(originalUri);
            builder.Host = newHost;
            Uri newUri = builder.Uri;
            return newUri.ToString();
        }

        public static string RemoveQueryStringFromUri(string originalUrl)
        {
            Uri originalUri = new Uri(originalUrl);
            UriBuilder builder = new UriBuilder(originalUri);
            builder.Query = string.Empty;
            Uri newUri = builder.Uri;
            return newUri.ToString();
        }

        public static string DownloadFile(string url)
        {
            var fileName = Path.GetFileName(url);
            var localFileName = new TestFileHelper().GetTempFileName(fileName);
            var wc = new System.Net.WebClient();
            wc.DownloadFile(url, localFileName);
            return localFileName;
        }

        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);
        }

        public static bool DeleteDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}