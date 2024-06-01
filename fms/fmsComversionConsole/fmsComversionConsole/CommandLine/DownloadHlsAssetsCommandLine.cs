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
    // downloadHlsAssets --masterM3U8Url "https://bskrmsqauswest-uswe.azureedge.net/9d9a063e-af04-407b-a165-dcff73acd10c/s722213.ism/Manifest(format=m3u8-cmaf)" --queryString 03mERGTE5S1ruL13GwbpI4UXgVc6BWRKS6myoQCi0HewMkh1I_59NYUdlMg5Ht3YuvhkZRTc6z6NnzXTWJswrAwVEw
    // downloadHlsAssets --masterM3U8Url "https://bskrmsqauswest-uswe.azureedge.net/97782927-8580-4c89-b4b3-838936c0bf48/s1365307.ism/Manifest(format=m3u8-cmaf)" --queryString OLY0HiJRsWlTMp7hokwLj89-FththlEEUGzD0zsgaL7dzMaJIJpCQ-KakTTI_ZCRJdJvSfS0YjQyxg16DWKpRhjsPQ 

    [Verb("downloadHlsAssets", HelpText = "concatHls")]
    public class DownloadHlsAssetsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('m', "masterM3U8Url", Required = false, HelpText = "MasterM3U8Url")]
        public string MasterM3U8Url { get; set; }

        [CommandLine.Option('f', "folder", Required = false, HelpText = "HlsFolder")]
        public string Folder { get; set; } = "C:\\temp";

        [CommandLine.Option('q', "queryString", Required = false, HelpText = "queryString")]
        public string QueryString { get; set; } 

    }
}
