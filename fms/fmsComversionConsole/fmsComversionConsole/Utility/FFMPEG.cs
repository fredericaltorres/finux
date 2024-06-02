using fAI;
using fms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fmsComversionConsole.Utility
{
    public class FFMPEG
    {
        // https://trac.ffmpeg.org/wiki/Concatenate
        public static int Concat(string ffmpegExe, string mp4FileName, string concatFileName)
        {
            int exitCode = 0;
            var sb = new StringBuilder();
            sb = new StringBuilder();
            sb.Append($@"-f concat -safe 0 -i ""{concatFileName}"" -c copy ""{mp4FileName}""");


            Logger.Trace($"FFMPEG.Concat {ffmpegExe} {sb.ToString()}");

            var r = ExecuteProgramUtilty.ExecProgram(ffmpegExe, sb.ToString(), ref exitCode);
            return exitCode;
        }
    }
}
