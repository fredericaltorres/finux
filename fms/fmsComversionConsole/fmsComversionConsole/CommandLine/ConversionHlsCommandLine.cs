using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace fmsComversionConsole
{
    // convertToHls --videoFileName "C:\brainshark\Fred.DTA.VDO\FredTrioProJazz.SMALL.mp4"
    // convertToHls --1080Only --videoFileName "C:\VIDEO\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion\MASTER\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion.mp4"
    // convertToHls --1080Only --videoFileName "C:\VIDEO\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts\MASTER\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts.mp4"
    // convertToHls --1080Only --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript\MASTER\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript.mp4"
    // convertToHls --1080Only --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch\MASTER\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch.mp4"
    // convertToHls --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
    // convertToHls --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
    [Verb("convertToHls", HelpText = "convertToHls")]
    public class ConversionHlsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string HlsFolder { get; set; } = "C:\\temp\\stream\\hls";

        [CommandLine.Option('1', "1080Only", Required = false, HelpText = "1080Only")]
        public bool _1080Only { get; set; }

    }

   
}
