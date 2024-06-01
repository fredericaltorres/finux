using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace fmsComversionConsole
{

    // downloadHlsAssets --masterM3U8Url "https://fvideostream.blob.core.windows.net/sirosvariri-05-video-full/master.m3u8" --outputfolder "C:\temp\stream\hls"

    [Verb("downloadHlsAssets", HelpText = "concatHls")]
    public class DownloadHlsAssetsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('m', "masterM3U8Url", Required = false, HelpText = "MasterM3U8Url")]
        public string MasterM3U8Url { get; set; }

        [CommandLine.Option('f', "folder", Required = false, HelpText = "HlsFolder")]
        public string Folder { get; set; } = "C:\\temp";
    }
}
