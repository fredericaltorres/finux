using CommandLine;

namespace fmsComversionConsole
{

    // downloadHlsAssets --masterM3U8Url "https://fvideostream.blob.core.windows.net/sirosvariri-05-video-full/master.m3u8" --outputfolder "C:\temp\stream\hls"
    // downloadHlsAssets --masterM3U8Url "https://bskrmsqauswest-uswe.azureedge.net/9d9a063e-af04-407b-a165-dcff73acd10c/s722213.ism/Manifest(format=m3u8-cmaf)" --queryString 03mERGTE5S1ruL13GwbpI4UXgVc6BWRKS6myoQCi0HewMkh1I_59NYUdlMg5Ht3YuvhkZRTc6z6NnzXTWJswrAwVEw
    // downloadHlsAssets --concat --fmsVideoId 6ef06108-313c-4326-36b8-08dc81b38e9d --masterM3U8Url "https://rmsproduswest-uswe.azureedge.net/6ef06108-313c-4326-36b8-08dc81b38e9d/s18081822.ism/Manifest(format=m3u8-cmaf)" --queryString I0JrHTse1DmDC6sRW8nV9AKCdRJZCSzfzqCtqNF0E-LUmUUE6RIQSc6Xkq7iUFlnrBe0yCzVM7ljA9IwtyXoymZboQ

    // downloadHlsAssets --concat --fmsVideoId 882c256a-e23f-4354-fccc-08dc83662584 --masterM3U8Url "https://rmsproduswest-uswe.azureedge.net/882c256a-e23f-4354-fccc-08dc83662584/s18082347.ism/Manifest(format=m3u8-cmaf)" --queryString VzPgNoRBLhDAf0TFHgR4NyozHud9zwjzx5ICuEMEw6OpmPaOpfbuY_95eSN3JjedQ-vYH9w8sT_L4kqoKtPp1L-yww




    // https://rmsproduswest-uswe.azureedge.net/6ef06108-313c-4326-36b8-08dc81b38e9d/s18081822.ism/Manifest(format=m3u8-cmaf)
    // I0JrHTse1DmDC6sRW8nV9AKCdRJZCSzfzqCtqNF0E-LUmUUE6RIQSc6Xkq7iUFlnrBe0yCzVM7ljA9IwtyXoymZboQ

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
