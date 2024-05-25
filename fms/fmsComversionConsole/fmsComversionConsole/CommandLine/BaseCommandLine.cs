using System;

namespace fmsComversionConsole
{
    public class BaseCommandLine
    {
        [CommandLine.Option('h', "HlsFolder", Required = false, HelpText = "HlsFolder")]
        public string FFMPEG_EXE { get; set; } = @"C:\Tools\ffmpeg-4.2.1-win64-static\bin\ffmpeg.exe";

        public string FMS_AZURE_STORAGE_CONNECTION_STRING => Environment.GetEnvironmentVariable("FMS_AZURE_STORAGE_CONNECTION_STRING");

        public string CDN_HOST => Environment.GetEnvironmentVariable("FMS_CDN_HOST");
    }
}
