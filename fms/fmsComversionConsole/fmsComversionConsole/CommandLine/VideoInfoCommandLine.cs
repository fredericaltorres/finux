using CommandLine;

namespace fmsComversionConsole
{
    // videoInfo --videoFileName "C:\Users\ftorres\AppData\Local\Temp\s18070541.mp4"
    [Verb("videoInfo", HelpText = "videoInfo")]
    public class VideoInfoCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }
    }
}
