using DynamicSugar;
using System;
using System.IO;
using System.Text;

namespace fms
{
    public class DirectoryFileService
    {
        const int MAX_CONTAINER_NAME_LENGTH = 63;

        public static string MakeNameAzureContainerNameSafe(string v)
        {
            var vv = v.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (var c in vv)
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

        public static bool IsUrl(string url)
        {
            return (!string.IsNullOrEmpty(url)) && (url.TrimStart().StartsWith("http://") || url.TrimStart().StartsWith("https://"));
        }

        public static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            MimeTypes.MimeTypeMap.TryGetMimeType(ext, out var mimeType);
            return mimeType;

            //switch (ext)
            //{
            //    case ".m3u8": return "application/vnd.apple.mpegurl";//"application/x-mpegURL";
            //    case ".ts": return "video/MP2T";
            //    default: return "application/octet-stream";
            //}
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

        public static bool CreateDirectory(string directoryPath, bool deleteIfExists = false)
        {
            try
            {
                if(deleteIfExists)
                    DeleteDirectory(directoryPath);

                Directory.CreateDirectory(directoryPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
