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
    // convertToHls --videoFileName "C:\Fred.DTA.VDO\FredTrioProJazz.SMALL.mp4"
    // convertToHls --1080Only --videoFileName "C:\VIDEO\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion\MASTER\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion.mp4"
    // convertToHls --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
    // convertToHls --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
    [Verb("convertToHls", HelpText = "convertToHls")]
    public class ConversionHlsCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string HlsFolder { get; set; } = "C:\\temp\\stream\\hls";

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string FFMPEG_EXE { get; set; } = @"C:\Tools\ffmpeg-4.2.1-win64-static\bin\ffmpeg.exe";

        [CommandLine.Option('1', "1080Only", Required = false, HelpText = "1080Only")]
        public bool _1080Only { get; set; }

        public string FMS_AZURE_STORAGE_CONNECTION_STRING => Environment.GetEnvironmentVariable("FMS_AZURE_STORAGE_CONNECTION_STRING");
    }

    internal class Program
    {
        static void Trace(string message)
        {
            Console.WriteLine(message);
        }

        static void Main(string[] args)
        {
            var cmdParser = new Parser(config => config.HelpWriter = null);
            var parsingTry = cmdParser.ParseArguments<ConversionHlsCommandLine>(args);
            var r = parsingTry.MapResult(
                  (ConversionHlsCommandLine options) =>
                  {
                      var vc = new fms.VideoConverter(options.VideoFileName);
                      Trace(vc.GetVideoInfo());
                      var c = vc.ConvertToHls(options.HlsFolder, options.FFMPEG_EXE, options.FMS_AZURE_STORAGE_CONNECTION_STRING, options._1080Only);
                      return 0;
                  },
                  errs => 123
            );
        }
    }
}
