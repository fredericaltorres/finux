using CommandLine;

namespace fmsComversionConsole
{
    /*
    fmsComversionConsole.exe videoInfo --videoFileName "C:\Users\ftorres\Downloads\VictorHugoPresentation.mp4"
    fmsComversionConsole.exe videoInfo --videoFileName "c:\temp\VictorHugoPresentation.mp4"

    fmsComversionConsole.exe videoInfo --videoFileName "C:\Brainshark\Fred.DTA.VDO\webM.noHeader.NoAudio.2.webm"
    fmsComversionConsole.exe videoInfo --videoFileName "C:\Brainshark\Fred.DTA.VDO\webm.withHeader.withNoAudio.webm"
    fmsComversionConsole.exe videoInfo --videoFileName "C:\Brainshark\Fred.DTA.VDO\WebMWithNoHeaderNoDuration.webm"

    fmsComversionConsole.exe videoInfo --videoFileName "C:\Brainshark\Fred.DTA.VDO\Kathie.WebMWithHeader.ThatWorks.webm"
    fmsComversionConsole.exe videoInfo --videoFileName "C:\Brainshark\Fred.DTA.VDO\sirosVariri.06.Video.webm"
    */

    [Verb("videoInfo", HelpText = "videoInfo")]
    public class VideoInfoCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }
    }

    /*
        fmsComversionConsole.exe addAudio --videoFileName "C:\temp\@fAiImages\mystical creature\mystical creature.cgdream.1664x2432.2Kish.mp4" --audioFileName "C:\temp\@fAiImages\mystical creature\audio.wav"
     */
    [Verb("addAudio", HelpText = "addAudio")]
    public class AddAudioCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }

        [CommandLine.Option('a', "audioFileName", Required = false, HelpText = "audioFileName")]
        public string AudioFileName { get; set; }
    }
}
