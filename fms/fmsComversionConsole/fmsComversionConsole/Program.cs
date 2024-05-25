using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using DynamicSugar;
using fAI;
using fms;

namespace fmsComversionConsole
{
  
    internal class Program
    {
        static void Trace(string message)
        {
            Console.WriteLine(message);
        }

        static void Main(string[] args)
        {
            var cmdParser = new Parser(config => config.HelpWriter = null);
            var parsingTry = cmdParser.ParseArguments<ConversionHlsCommandLine, ConcatHlsCommandLine>(args);
            var r = parsingTry.MapResult(
                  (ConversionHlsCommandLine options) =>
                  {
                      var vc = new fms.VideoConverter(options.VideoFileName);
                      Trace(vc.GetVideoInfo());
                      var c = vc.ConvertToHls(options.HlsFolder, options.FFMPEG_EXE, options.FMS_AZURE_STORAGE_CONNECTION_STRING, options.ResolutionList, options.CDN_HOST, options.fmsVideoId);
                      return 0;
                  },
                  (ConcatHlsCommandLine options) =>
                  {
                      var hlsM = new HlsManager(options.MasterM3U8Url);
                      var data = hlsM.GetMasterInfo();
                      Logger.Trace($"{data}", new { }, replaceCRLF: false);
                      Trace(data);
                      Console.ReadLine();
                      return 0;
                  },
                  errs => 123
            );
        }
    }
}
