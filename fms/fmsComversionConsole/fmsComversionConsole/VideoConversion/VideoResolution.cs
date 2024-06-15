using System;
using System.Collections.Generic;
using System.Linq;

namespace fms
{
    // 1280 x 720 pixels 720p
    // 1920 × 1080 pixels 1080p
    public class VideoResolution
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public string BitRate { get; set; } // 5M or 5000K
        public int KeyFrame { get; set; } = 48;
        public string Preset { get; set; } = "medium";
        public int SegmentDurationSeconds { get; set; } = 6;
        public bool IsSquareResolution => Width == Height;

        public bool CanVideoBeConvertedToResolution(int videoWidth, int videoHeight)
        {
            return videoWidth >= Width && videoHeight >= Height;
        }

        // https://ottverse.com/cbr-crf-changing-resolution-using-ffmpeg/   
        public string GetVideoMapCmd(int resolutionIndex)
        {
            return $@"-map ""[v{resolutionIndex}out]"" 
-c:v:0 libx264 
-x264-params ""nal-hrd=cbr:force-cfr=1"" 
-b:v:0 {BitRate} -maxrate:v:0 {BitRate} -minrate:v:0 {BitRate} 
-bufsize:v:0 {BitRate} -preset ""{Preset}"" -g {KeyFrame} -sc_threshold 0 -keyint_min {KeyFrame} ".Replace(Environment.NewLine, "");
        }

        public string GetScaleResolutionCmd(int resolutionIndex, bool isLast)
        {
            return $@"[v{resolutionIndex}]scale=w={Width}:h={Height}[v{resolutionIndex}out]{(isLast ? "":";")}";
        }

        int AudioBitRateAacKb = 128; // 44100*2*2/1024 == 172Kb

        public string GetAudioMapCmd(int resolutionIndex)
        {
            return $@"-map a:0 -c:a:{resolutionIndex} aac -b:a:{resolutionIndex} {AudioBitRateAacKb}k -ac 2 ";
        }

        public string GetFFMPEGStreamMap(int resolutionIndex)
        {
            return $@"v:{resolutionIndex},a:{resolutionIndex} ";
        }

        public static Dictionary<string, VideoResolution> VideoResolutions = new Dictionary<string, VideoResolution>()
        {
            // todo: support kb rate for small resolution.
            ["1080p"]       = new VideoResolution() { Width = 1920, Height = 1080, Name = "1080p",      BitRate = "5M", Preset = "medium", KeyFrame = 48 },
            ["1080x1080p"]  = new VideoResolution() { Width = 1080, Height = 1080, Name = "1080x1080p", BitRate = "5M", Preset = "medium", KeyFrame = 48 },

            ["1024x1024p"]  = new VideoResolution() { Width = 1024, Height = 1024, Name = "1080x1080p", BitRate = "5M", Preset = "medium", KeyFrame = 48 },

            ["960x540"]     = new VideoResolution() { Width = 960,  Height = 540,  Name = "960x540",    BitRate = "3M", Preset = "medium", KeyFrame = 48 },

            ["720p"]        = new VideoResolution() { Width = 1280, Height = 720,  Name = "720p",       BitRate = "2M", Preset = "medium", KeyFrame = 48 },
            ["720x720p"]    = new VideoResolution() { Width = 720,  Height = 720,  Name = "720x720p",   BitRate = "2M", Preset = "medium", KeyFrame = 48 },

            ["630x360p"]    = new VideoResolution() { Width = 630, Height = 360,   Name = "630x360p",   BitRate = "1M", Preset = "medium", KeyFrame = 48 },

            ["480p"]        = new VideoResolution() { Width = 640,  Height = 480,  Name = "480p",       BitRate = "1M", Preset = "medium", KeyFrame = 48 },
            ["480x480p"]    = new VideoResolution() { Width = 480,  Height = 480,  Name = "480x480p",   BitRate = "1M", Preset = "medium", KeyFrame = 48 },

            ["320x240p"]    = new VideoResolution() { Width = 320,  Height = 240,  Name = "320x240p",   BitRate = "500K", Preset = "medium", KeyFrame = 48 },
            ["240x240p"]    = new VideoResolution() { Width = 240,  Height = 240,  Name = "240x240p",   BitRate = "500K", Preset = "medium", KeyFrame = 48 },
        };

        public static List<string> VideoResolutionAvailable = VideoResolutions.Keys.ToList();
    }
}
