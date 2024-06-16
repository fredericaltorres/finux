using DynamicSugar;
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
        public static H265ConversionResult ConvertToH265(string mp4FileName, string ffmepexe, TestFileHelper tfh)
        {
            var mp4FileNameH265 = tfh.GetTempFileName("mp4");
            var r = new H265ConversionResult { InputFile = mp4FileName };
            var sb = new StringBuilder();
            r.Mp4FileName = mp4FileNameH265;

            sb.Append($@" -i ""{mp4FileName}"" -vcodec libx265 -crf 28 ""{mp4FileNameH265}"" ");

            r.FFMPEGCommandLine = sb.ToString();
            var exitCode = 0;
            var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            r.Succeeded = rr && exitCode == 0;
            r.Done();

            if (r.Succeeded)
            {
            }
            else
            {
            }
            Logger.Trace($"{r.ToJson()}");

            return r;
        }

        public static ChangeBitRateConversionResult ChangeBitrate(string mp4FileName, int bitRateKb, string ffmepexe, TestFileHelper tfh)
        {
            var mp4FileNameH265 = tfh.GetTempFileName("mp4");
            var r = new ChangeBitRateConversionResult { InputFile = mp4FileName };
            var sb = new StringBuilder();
            r.Mp4FileName = mp4FileNameH265;
            sb.Append($@" -i ""{mp4FileName}""  -b {bitRateKb}k  ""{mp4FileNameH265}"" ");
            r.FFMPEGCommandLine = sb.ToString();
            var exitCode = 0;
            var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            r.Succeeded = rr && exitCode == 0;
            r.Done();
            if (r.Succeeded)
            {
            }
            else
            {
            }
            Logger.Trace($"{r.ToJson()}");

            return r;
        }

        public static AddAudioTrackResult AddBlankAudioTrack(string mp4FileName, string ffmepexe, TestFileHelper tfh)
        {
            var mp4FileNameWithAudio = tfh.GetTempFileName("mp4");
            var r = new AddAudioTrackResult { InputFile = mp4FileName };
            var sb = new StringBuilder();
            r.Mp4FileName = mp4FileNameWithAudio;
            sb.Append($@"-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -i ""{mp4FileName}"" -c:v copy -c:a aac -shortest ""{mp4FileNameWithAudio}"" ");
            r.FFMPEGCommandLine = sb.ToString();
            var exitCode = 0;
            var rr = ExecuteProgramUtilty.ExecProgram(ffmepexe, sb.ToString(), ref exitCode);
            r.Succeeded = rr && exitCode == 0;
            r.Done();

            if (r.Succeeded)
            {
            }
            else
            {
            }
            Logger.Trace($"{r.ToJson()}");

            return r;
        }

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
