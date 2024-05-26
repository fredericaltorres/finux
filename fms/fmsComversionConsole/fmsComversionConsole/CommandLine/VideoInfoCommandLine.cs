using CommandLine;

namespace fmsComversionConsole
{
    // fmsComversionConsole.exe videoInfo --videoFileName "C:\Users\ftorres\Downloads\VictorHugoPresentation.mp4"
    // fmsComversionConsole.exe videoInfo --videoFileName "c:\temp\VictorHugoPresentation.mp4"
    [Verb("videoInfo", HelpText = "videoInfo")]
    public class VideoInfoCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }
    }
}
