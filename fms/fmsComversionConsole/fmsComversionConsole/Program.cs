﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace fmsComversionConsole
{
    // convertToHls --videoFileName "C:\Brainshark\Fred.DTA.VDO\FredTrioProJazz.SMALL.mp4"
    [Verb("convertToHls", HelpText = "convertToHls")]
    public class ConversionHlsCommandLine
    {
        [CommandLine.Option('v', "videoFileName", Required = false, HelpText = "VideoFileName")]
        public string VideoFileName { get; set; }

        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string HlsFolder { get; set; } = "C:\\temp\\stream\\hls";
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
                      return 0;
                  },
                  errs => 123
            );
        }
    }
}
