using System;
using System.CodeDom.Compiler;
using DynamicSugar;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using M3U8Parser.Attributes;

namespace fmsComversionConsole
{
    /*
     * 
     
     convertToHls --videoFileName "C:\Users\ftorres\AppData\Local\Temp\s18070541.mp4" --fmsVideoId "webm-2-mp4" --resolutions "1080p,720p,480p"

     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\Fred.TranslatingPowerPointWithGPTApi\Fred.TranslatingPowerPointWithGPTApi\MASTER\Fred.TranslatingPowerPointWithGPTApi\Fred.TranslatingPowerPointWithGPTApi.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion\MASTER\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts\MASTER\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript\MASTER\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch\MASTER\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"


     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\And Your Bird Can Sing.Video\And Your Bird Can Sing.Video\MASTER\And Your Bird Can Sing.Video\And Your Bird Can Sing.Video.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\I Want You.Video\master\I Want You.Video\I Want You.Video.mp4"
     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript\MASTER\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript.mp4"

     fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\I Want You.Video\master\I Want You.Video\I Want You.Video.mp4"

     convertToHls --doNotCopyToAzure --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"

     convertToHls --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "https://fredcloud.blob.core.windows.net/public/IN-BETWEEN-DAYS.2020.11.26.mp4"


     HIGH RESOLUTION
     convertToHls --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p"  --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4K__4133023-uhd_3840_2160_30fps.mp4"
     convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4762563-FHD_4096_2160_24fps.mp4"

    convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4K__7493928-uhd_3840_2160_25fps.mp4"
    convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4k__7710585-FHD_4096_2160_25fps.mp4"

    //
    */
    [Verb("convertToHls", HelpText = "convertToHls")]
    public class VideoConversionHlsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string HlsFolder { get; set; } = "C:\\temp\\stream\\hls";

        // All resolutions: 1080p, 1080x1080p, 720p, 720x720p, 480p, 480x480p, 320x240p, 240x240p
        [CommandLine.Option('r', "resolutions", Required = false, HelpText = "Resolution")]
        public string Resolutions { get; set; } = "1080p,720p,480p";

        [CommandLine.Option('f', "deriveFmsVideoId", Required = false, HelpText = "deriveFmsVideoId")]
        public bool DeriveFmsVideoId { get; set; } = false;

        public List<string> ResolutionList { get { return this.Resolutions.Split(DS.List(",").ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();  } }

        [CommandLine.Option('m', "maxResolution", Required = false, HelpText = "maxResolution")]
        public int MaxResolution { get; set; } = 3;
    }

    // fmsComversionConsole.exe convertAudioToHls  --deriveFmsVideoId --audioFileName "https://fredcloud.blob.core.windows.net/zic/Phil.2022.Instrumental.wav"
    [Verb("convertAudioToHls", HelpText = "convertToHls")]
    public class AudioConversionHlsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('a', "audioFileName", Required = false, HelpText = "audioFileName")]
        public string AudioFileName { get; set; }

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string HlsFolder { get; set; } = "C:\\temp\\stream\\hls";

        [CommandLine.Option('f', "deriveFmsVideoId", Required = false, HelpText = "deriveFmsVideoId")]
        public bool DeriveFmsVideoId { get; set; } = false;
    }
}
