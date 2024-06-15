using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSugar;
using System.Text;
using static System.Net.WebRequestMethods;
using static fms.lib.VideoCodecCsParser;

namespace fms.lib
{
    public class VideoCodecCsParser
    {
        // https://gist.github.com/MinChanSike/34087f75adf695ea9d4ec3ed550c6a2a
        // https://gist.github.com/MinChanSike/34087f75adf695ea9d4ec3ed550c6a2a
        // https://www.rfc-editor.org/rfc/rfc6381#section-3.3
        // Advanced Video Coding(https://en.wikipedia.org/wiki/Advanced_Video_Coding#Profiles)
        //  avc1[PPCCLL] (AVC video)
        // "avc1.64001f,mp4a.40.2"
        // "avc1.640015,mp4a.40.2"
        // "avc1.640028,mp4a.40.2"
        // "avc1.42c01e,mp4a.40.2"
        // "avc1.42c00d,mp4a.40.2"
        // https://developer.mozilla.org/en-US/docs/Web/Media/Formats/codecs_parameter#avc_profiles

        Dictionary<string, string> AdvancedVideoCodingCodec = new Dictionary<string, string>
        {
            ["avc1"] = "H.264",
            ["avc2"] = "H.264",
            ["svc1"] = "Scalable Video Coding 1",
            ["mvc1"] = "Multiview Video Coding 1",
            ["mvc2"] = "Multiview Video Coding 2",
        };

        Dictionary<string, string> AudioCodec = new Dictionary<string, string>
        {
            ["mp4a"] = "MP4 Audio",
        };

        


        // https://developer.mozilla.org/en-US/docs/Web/Media/Formats/codecs_parameter#avc_profiles
        const string AVC_PROFILES_STRING = @"
Constrained Baseline Profile (CBP)
CBP is primarily a solution for scenarios in which resources are constrained, or resource use needs to be controlled to minimize the odds of the media performing poorly.
42
40

Baseline Profile (BP) 
Similar to CBP but with more data loss protections and recovery capabilities. This is not as widely used as it was before CBP was introduced. All CBP streams are considered to also be BP streams.   
42
00

Extended Profile (XP) 
Designed for streaming video over the network, with high compression capability and further improvements to data robustness and stream switching. 
58  
00

Main Profile (MP) 
The profile used for standard-definition digital television being broadcast in MPEG-4 format. Not used for high-definition television broadcasts. This profile's importance has faded since the introduction of the High Profile—which was added for HDTV use—in 2004.	
4D	00

High Profile (HiP) 
Currently, HiP is the primary profile used for broadcast and disc-based HD video; it's used both for HD TV broadcasts and for Blu-Ray video.	
64	
00

Progressive High Profile (PHiP) 
Essentially High Profile without support for field coding.  
64	
08

Constrained High Profile (PHiP)
PHiP, but without support for bi-predictive slices (""B-slices"").	
64	
0C

High 10 Profile(Hi10P)
High Profile, but with support for up to 10 bits per color component.
6E	
00

High 4:2:2 Profile (Hi422P) 
Expands upon Hi10P by adding support for 4:2:2 chroma subsampling along with up to10 bits per color component.	
7A  
00

High 4:4:4 Predictive Profile (Hi444PP) 
In addition to the capabilities included in Hi422P, Hi444PP adds support for 4:4:4 chroma subsampling (in which no color information is discarded). Also includes support for up to 14 bits per color sample and efficient lossless region coding.The option to encode each frame as three separate color planes(that is, each color's data is stored as if it were a single monochrome frame).	
F4	
00

High 10 Intra Profile
High 10 constrained to all-intra-frame use. Primarily used for professional apps.	
6E	
10

High 4:2:2 Intra Profile 
The Hi422 Profile with all-intra-frame use.	
7A  
10

High 4:4:4 Intra Profile 
The High 4:4:4 Profile constrained to use only intra frames.
F4  
10

CAVLC 4:4:4 Intra Profile 
The High 4:4:4 Profile constrained to all-intra use, and to using only CAVLC entropy coding.  
44	
00

Scalable Baseline Profile 
Intended for use with video conferencing as well as surveillance and mobile uses, the SVC Baseline Profile is based on AVC's Constrained Baseline profile. The base layer within the stream is provided at a high quality level, with some number of secondary substreams that offer alternative forms of the same video for use in various constrained environments. These may include any combination of reduced resolution, reduced frame rate, or increased compression levels.	
53	
00

Scalable Constrained Baseline 
Profile Primarily used for real-time communication applications.Not yet supported by WebRTC, but an extension to the WebRTC API to allow SVC is in development.  
53	
04

Scalable High Profile 
Meant mostly for use in broadcast and streaming applications.The base (or highest quality) layer must conform to the AVC High Profile.   
56	
00

Scalable Constrained High Profile 
A subset of the Scalable High Profile designed mainly for real-time communication.	
56	
04

Scalable High Intra Profile 
Primarily useful only for production applications, this profile supports only all-intra usage.	
56
20

Stereo High Profile 
The Stereo High Profile provides stereoscopic video using two renderings of the scene(left eye and right eye). Otherwise, provides the same features as the High profile.  
80	
00

Multiview High Profile 
Supports two or more views using both temporal and MVC inter-view prediction.Does not support field pictures or macroblock-adaptive frame-field coding.	
76	
00

Multiview Depth High Profile 
Based on the High Profile, to which the main substream must adhere.The remaining substreams must match the Stereo High Profile.   
8A
00";

        public class AvcProfile
        {
            public string Name { get; set; }
            public string Descripiton { get; set; }
            public int Code { get; set; }
            public int Constraint { get; set; }
            public string CodeHexa => $"{Code:x}";

            public override string ToString()
            {
                return $"{Name} - {CodeHexa:x} - {Constraint:x}";
            }
        }

        public class AvcProfiles : List<AvcProfile>
        {
            public AvcProfiles()
            {
                var lines = AVC_PROFILES_STRING.SplitByCRLF().Select(s => s.Trim()).ToList();
                for (int i = 0; i < lines.Count; i += 4)
                {
                    this.Add( new AvcProfile {
                        Name = lines[i],
                        Descripiton = lines[i + 1],
                        Code = int.Parse(lines[i + 2], System.Globalization.NumberStyles.HexNumber),
                        Constraint = int.Parse(lines[i + 3], System.Globalization.NumberStyles.HexNumber)
                    });
                }
            }

            public AvcProfile Get(int code, int constraint)
            {
                var r = this.FirstOrDefault(p => p.Code == code && p.Constraint == constraint);
                return r;
            }

            internal bool Is(string codecType)
            {
                return this.Any(p => p.Name == codecType);
            }
        }

        //Dictionary<string, string> PROFILE = new Dictionary<string, string>
        //{


        //    ["0" ] = "No",              //  0 - *** when profile=RCDO and level=0 - "RCDO"  - RCDO bitstream MUST obey to all the constraints of the Baseline profile
        //    ["42"] = "Baseline",        // 66 in-decimal
        //    ["4d"] = "Main",            // 77 in-decimal
        //    ["58"] = "Extended",        // 88 in-decimal
        //    ["64"] = "High",            //100 in-decimal
        //    ["6e"] = "High 10",         //110 in-decimal
        //    ["7a"] = "High 4]= 2]= 2",  //122 in-decimal
        //    ["f4"] = "High 4]= 4]= 4",  //244 in-decimal
        //    ["2c"] = "CAVLC 4]= 4]= 4", // 44 in-decimal

        //    //profiles for SVC - Scalable Video Coding extension to H.264
        //    ["53"] = "Scalable Baseline", // 83 in-decimal
        //    ["56"] = "Scalable High", // 86 in-decimal

        //    //profiles for MVC - Multiview Video Coding extension to H.264
        //    ["80"] = "Stereo High", // 128 in-decimal
        //    ["76"] = "Multiview High", // 118 in-decimal
        //    ["8a"] = "Multiview Depth High", // 138 in-decimal


        //    ["53"] = "Scalable Baseline Profile",
        //    ["56"] = "Scalable High Profile",
        //    ["80"] = "Stereo High Profile",
        //    ["76"] = "Multiview High Profile",
        //    ["86"] = "MFC High Profile",
        //    ["87"] = "MFC Depth High Profile",
        //    ["8A"] = "Multiview Depth High Profile",
        //    ["8B"] = "Enhanced Multiview Depth High Profile",
        //};

        private bool Is(Dictionary<string, string> dictionary, string key)
        {
            return dictionary.ContainsKey(key);
        }
        
        private string Get(Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            return "Unknown";
        }

        public string Parse(string codec)
        {
            var avcProfiles = new AvcProfiles();
            var sb = new StringBuilder();
            var codecs = codec.Split(',');
            foreach (var c in codecs)
            {
                var codecParts = c.Split('.').ToList();
                if (codecParts.Count > 1)
                {
                    // "avc1.64001f,mp4a.40.2"
                    var codecType = codecParts[0];
                    var codecName = codecParts[1];

                    if (avcProfiles.Is(codecType))
                    {
                        var profileCode = int.Parse(codecName.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        var profileLevel = int.Parse(codecName.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 10f;
                        var profileConstraint = int.Parse(codecName.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        var avcRefProfile = avcProfiles.Get(profileCode, profileConstraint);
                        if (avcRefProfile != null)
                        {

                            sb.Append($"Avc: {avcRefProfile}, ");
                            sb.Append($"profileLevel: v {profileLevel}");
                        }
                        else sb.Append($"Avc: {codecType}, with constraint {profileConstraint} not found");
                    }

                    if (Is(AudioCodec, codecType))
                    {
                        sb.Append($"Audio: {Get(AudioCodec, codecType)}, ");
                        sb.Append($"{codecParts[1]}, ");
                        sb.Append($"{codecParts[2]}, ");
                    }
                }
            }
            return sb.ToString();
        }
      
    }
}
