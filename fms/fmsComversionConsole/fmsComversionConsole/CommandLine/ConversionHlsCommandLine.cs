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
    // convertToHls --videoFileName "C:\\Fred.DTA.VDO\FredTrioProJazz.SMALL.mp4" --fmsVideoId "fred-trio-pro-jazz-small" --resolutions "1080p,720p,480p"
    // convertToHls --videoFileName "C:\VIDEO\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion\MASTER\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion.mp4"
    // convertToHls --videoFileName "C:\VIDEO\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts\MASTER\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts.mp4"
    // convertToHls --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript\MASTER\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript.mp4"
    // convertToHls --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch\MASTER\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch.mp4"
    // convertToHls --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
    // convertToHls --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
    [Verb("convertToHls", HelpText = "convertToHls")]
    public class ConversionHlsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string HlsFolder { get; set; } = "C:\\temp\\stream\\hls";

        // All resolutions: 1080p, 1080x1080p, 720p, 720x720p, 480p, 480x480p, 320x240p, 240x240p
        [CommandLine.Option('r', "resolutions", Required = false, HelpText = "Resolution")]
        public string Resolutions { get; set; } = "1080p,720p,480p";

        [CommandLine.Option('f', "fmsVideoId", Required = false, HelpText = "fmsVideoId")]
        public string fmsVideoId { get; set; } = "";

        public List<string> ResolutionList { get { return this.Resolutions.Split(DS.List(",").ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();  } }
    }
}
