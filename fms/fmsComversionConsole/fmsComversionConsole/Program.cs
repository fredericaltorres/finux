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
    // gif to mp4 -- https://unix.stackexchange.com/questions/40638/how-to-do-i-convert-an-animated-gif-to-an-mp4-or-mv4-on-the-command-line
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Logger.TraceToConsole = true;
                Logger.Trace(Environment.CommandLine);
                var cmdParser = new Parser(config => config.HelpWriter = null);
                var parsingTry = cmdParser.ParseArguments<AudioConversionHlsCommandLine, VideoConversionHlsCommandLine, DownloadHlsAssetsCommandLine, VideoInfoCommandLine, ConversionGifToMp4CommandLine> (args);

                var r = parsingTry.MapResult(

                      (AudioConversionHlsCommandLine options) =>
                      {
                          var vc = new fms.VideoManager(options.AudioFileName);
                          var c = vc.ConvertAudioToHls(options.HlsFolder, options.FFMPEG_EXE, options.FMS_AZURE_STORAGE_CONNECTION_STRING, options.CDN_HOST, options.fmsVideoId, options.DeriveFmsVideoId, options.CopyToAzure);
                          return 0;
                      },
                      (VideoConversionHlsCommandLine options) =>
                      {
                          var vc = new fms.VideoManager(options.VideoFileName);
                          var c = vc.ConvertVideoToHls(options.HlsFolder, options.FFMPEG_EXE, options.FMS_AZURE_STORAGE_CONNECTION_STRING, options.ResolutionList, options.CDN_HOST, options.fmsVideoId, options.DeriveFmsVideoId, options.CopyToAzure, options.MaxResolution);
                          return c.Succeeded ? 0 : 1;
                      },
                      (DownloadHlsAssetsCommandLine options) =>
                      {
                          var hlsM = new HlsManager(options.MasterM3U8Url, options.QueryString);

                          Logger.Trace($"DownloadHlsAssets {options.MasterM3U8Url}, resolutions: {hlsM.GetAllResolutionDefinition()}", new { }, replaceCRLF: false);

                          var downloadInfo = hlsM.DownloadHlsAssets(options.OutputFolder, options.fmsVideoId, options.Concat, options.FFMPEG_EXE);
                          Logger.Trace($"{downloadInfo.ToJSON()}", new { }, replaceCRLF: false);
                          //var data = hlsM.GetMasterInfo();
                          //Logger.Trace($"{data}", new { }, replaceCRLF: false);
                          //Trace(data);
                          return downloadInfo.Succeeded ? 0 : 1;
                      },
                      (VideoInfoCommandLine options) =>
                      {
                          var vc = new fms.VideoManager(options.VideoFileName);
                          Logger.Trace(vc.GetVideoInfo());
                          return 0;
                      },
                      (ConversionGifToMp4CommandLine options) =>
                      {
                          var vc = new fms.VideoManager(options.GifFileName);
                          Logger.Trace(vc.GetVideoInfo());
                          var rr = vc.ConvertGifToMp4(options.Mp4FileName, options.BitRateKb, options.FFMPEG_EXE);
                          return rr.Succeeded ? 0 : 1;
                      },

                      errs => 123
                );
            }
            catch (Exception ex)
            {
                Logger.TraceError(ex);
            }
        }
    }
}
