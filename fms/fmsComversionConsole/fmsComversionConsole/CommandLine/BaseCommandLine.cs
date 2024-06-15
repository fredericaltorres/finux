using System;

namespace fmsComversionConsole
{
    public class BaseCommandLine
    {
        [CommandLine.Option('f', "ffmpeg_exe", Required = false, HelpText = "ffmpeg_exe")]
        public string FFMPEG_EXE { get; set; } = @"C:\Tools\ffmpeg-4.2.1-win64-static\bin\ffmpeg.exe";

        public string FMS_AZURE_STORAGE_CONNECTION_STRING => Environment.GetEnvironmentVariable("FMS_AZURE_STORAGE_CONNECTION_STRING");

        public string CDN_HOST => Environment.GetEnvironmentVariable("FMS_CDN_HOST");

        [CommandLine.Option('o', "outputfolder", Required = false, HelpText = "outputfolder")]
        public string OutputFolder { get; set; } = @"C:\temp\stream\hls";

        [CommandLine.Option('f', "fmsVideoId", Required = false, HelpText = "fmsVideoId")]
        public string fmsVideoId { get; set; } = "";

        [CommandLine.Option('a', "doNotCopyToAzure", Required = false, HelpText = "doNotCopyToAzure")]
        public bool DoNotCopyToAzure { get; set; } = false;

        public bool CopyToAzure => !DoNotCopyToAzure;
    }
}
