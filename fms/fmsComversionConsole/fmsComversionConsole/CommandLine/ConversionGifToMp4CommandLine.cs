using CommandLine;

namespace fmsComversionConsole
{
    /* 
         convertGifToMp4 --gifFileName C:\dvt\FREDWEBSITE2024\build\images\AI\VictorHugoPresentation.gif --mp4FileName "c:\temp\VictorHugoPresentation.mp4"
         convertToHls --videoFileName "c:\temp\VictorHugoPresentation.mp4" --fmsVideoId "victor-hugo-presentation" --resolutions "1024x1024p"
    */

    [Verb("convertGifToMp4", HelpText = "convertToHls")]
    public class ConversionGifToMp4CommandLine : BaseCommandLine
    {
        [CommandLine.Option('m', "mp4FileName", Required = false, HelpText = "mp4FileName")]
        public string Mp4FileName { get; set; }

        [CommandLine.Option('g', "gifFileName", Required = false, HelpText = "VideoFileName")]
        public string GifFileName { get; set; }
    }
}
