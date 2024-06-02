using CommandLine;

namespace fmsComversionConsole
{

    // downloadHlsAssets --masterM3U8Url "https://fvideostream.blob.core.windows.net/sirosvariri-05-video-full/master.m3u8" --outputfolder "C:\temp\stream\hls"
    // downloadHlsAssets --masterM3U8Url "https://bskrmsqauswest-uswe.azureedge.net/9d9a063e-af04-407b-a165-dcff73acd10c/s722213.ism/Manifest(format=m3u8-cmaf)" --queryString 03mERGTE5S1ruL13GwbpI4UXgVc6BWRKS6myoQCi0HewMkh1I_59NYUdlMg5Ht3YuvhkZRTc6z6NnzXTWJswrAwVEw
    // downloadHlsAssets --concat --fmsVideoId 97782927-8580-4c89-b4b3-838936c0bf48.s1365307 --masterM3U8Url "https://bskrmsqauswest-uswe.azureedge.net/97782927-8580-4c89-b4b3-838936c0bf48/s1365307.ism/Manifest(format=m3u8-cmaf)" --queryString zcCOap4D6cWPDChH-oiA6hZAGDz4uvkYOjKEWqD_RfEXGm12F7p_ykOjKwaqLKe3L02UjBDMN_YwGieE3FuMGr4G1Q

    [Verb("downloadHlsAssets", HelpText = "concatHls")]
    public class DownloadHlsAssetsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('m', "masterM3U8Url", Required = false, HelpText = "MasterM3U8Url")]
        public string MasterM3U8Url { get; set; }

        [CommandLine.Option('f', "folder", Required = false, HelpText = "HlsFolder")]
        public string Folder { get; set; } = "C:\\temp";

        [CommandLine.Option('q', "queryString", Required = false, HelpText = "queryString")]
        public string QueryString { get; set; }

        [CommandLine.Option('c', "concat", Required = false, HelpText = "concat")]
        public bool Concat { get; set; } = false;
    }
}
