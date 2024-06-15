using System;
using System.Collections.Generic;

namespace fms.lib
{
    public class VideoCodecCsParser
    {
        // https://gist.github.com/MinChanSike/34087f75adf695ea9d4ec3ed550c6a2a
        // https://www.rfc-editor.org/rfc/rfc6381#section-3.3
        // "avc1.64001f,mp4a.40.2"
        // "avc1.640015,mp4a.40.2"
        // "avc1.640028,mp4a.40.2"
        // "avc1.42c01e,mp4a.40.2"
        // "avc1.42c00d,mp4a.40.2"

        Dictionary<string, string> ISOAVC_MAP = new Dictionary<string, string>
        {
            ["avc1"] = "H.264",
            ["avc2"] = "H.264",
            ["svc1"] = "Scalable Video Coding 1",
            ["mvc1"] = "Multiview Video Coding 1",
            ["mvc2"] = "Multiview Video Coding 2",
        };

        Dictionary<string, string> PROFILE = new Dictionary<string, string>
        {
            ["0"] = "No", //  0             - *** when profile=RCDO and level=0 - "RCDO"  - RCDO bitstream MUST obey to all the constraints of the Baseline profile
            ["42"] = "Baseline", // 66 in-decimal
            ["4d"] = "Main", // 77 in-decimal
            ["58"] = "Extended", // 88 in-decimal
            ["64"] = "High", //100 in-decimal
            ["6e"] = "High 10", //110 in-decimal
            ["7a"] = "High 4]= 2]= 2", //122 in-decimal
            ["f4"] = "High 4]= 4]= 4", //244 in-decimal
            ["2c"] = "CAVLC 4]= 4]= 4", // 44 in-decimal

            //profiles for SVC - Scalable Video Coding extension to H.264
            ["53"] = "Scalable Baseline", // 83 in-decimal
            ["56"] = "Scalable High", // 86 in-decimal

            //profiles for MVC - Multiview Video Coding extension to H.264
            ["80"] = "Stereo High", // 128 in-decimal
            ["76"] = "Multiview High", // 118 in-decimal
            ["8a"] = "Multiview Depth High", // 138 in-decimal
        };



        public string Parse(string codec)
        {
            string[] codecs = codec.Split(',');
            string videoCodec = codecs[0];
            string audioCodec = codecs[1];

            string videoCodecName = ParseVideoCodec(videoCodec);
            string audioCodecName = ParseAudioCodec(audioCodec);

            return $"{videoCodecName}, {audioCodecName}";
        }

        private string ParseAudioCodec(string audioCodec)
        {
            return "a";
        }

        private string ParseVideoCodec(string videoCodec)
        {
            return "v";
        }
    }
}
