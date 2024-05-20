using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace fmsComversionConsole
{

    // concatHls --masterM3U8Url "https://fvideostream.blob.core.windows.net/c0c9e315-527f-4568-87da-4b9156e01021/master.m3u8"

    [Verb("concatHls", HelpText = "concatHls")]
    public class ConcatHlsCommandLine : BaseCommandLine
    {
        [CommandLine.Option('m', "masterM3U8Url", Required = false, HelpText = "MasterM3U8Url")]
        public string MasterM3U8Url { get; set; }

        [CommandLine.Option('f', "folder", Required = false, HelpText = "HlsFolder")]
        public string Folder { get; set; } = "C:\\temp";
    }
}
