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
        public int BitRateMegaBit { get; set; }
        public int KeyFrame { get; set; } = 48;
        public string Preset { get; set; } = "medium";
        public int SegmentDurationSeconds { get; set; } = 6;

        public string GetVideoMapCmd(int resolutionIndex)
        {
            return $@"-map ""[v{resolutionIndex}out]"" -c:v:0 libx264 -x264-params ""nal-hrd=cbr:force-cfr=1"" -b:v:0 {BitRateMegaBit}M -maxrate:v:0 {BitRateMegaBit}M -minrate:v:0 {BitRateMegaBit}M -bufsize:v:0 {BitRateMegaBit}M -preset ""{Preset}"" -g {KeyFrame} -sc_threshold 0 -keyint_min {KeyFrame} ";
        }
        public string GetScaleResolutionCmd(int resolutionIndex, bool isLast)
        {
            return $@"[v{resolutionIndex}]scale=w={Width}:h={Height}[v{resolutionIndex}out]{(isLast ? "":";")}";
        }

        public string GetAudioMapCmd(int resolutionIndex)
        {
            return $@"-map a:0 -c:a:{resolutionIndex} aac -b:a:{resolutionIndex} 128k -ac 2 ";
        }

        public string GetFFMPEGStreamMap(int resolutionIndex)
        {
            return $@"v:{resolutionIndex},a:{resolutionIndex} ";
        }

        public bool IsSquareResolution => Width == Height;



        public static Dictionary<string, VideoResolution> VideoResolutions = new Dictionary<string, VideoResolution>()
        {
            ["1080p"] = new VideoResolution() { Width = 1920, Height = 1080, Name = "1080p", BitRateMegaBit = 5, Preset = "medium", KeyFrame = 48 },
            ["1080x1080p"] = new VideoResolution() { Width = 1080, Height = 1080, Name = "1080x1080p", BitRateMegaBit = 5, Preset = "medium", KeyFrame = 48 },

            ["1024x1024p"] = new VideoResolution() { Width = 1024, Height = 1024, Name = "1080x1080p", BitRateMegaBit = 5, Preset = "medium", KeyFrame = 48 },

            ["720p"] = new VideoResolution() { Width = 1280, Height = 720, Name = "720p", BitRateMegaBit = 1, Preset = "medium", KeyFrame = 48 },
            ["720x720p"] = new VideoResolution() { Width = 720, Height = 720, Name = "720x720p", BitRateMegaBit = 1, Preset = "medium", KeyFrame = 48 },
            ["480p"] = new VideoResolution() { Width = 640, Height = 480, Name = "480p", BitRateMegaBit = 1, Preset = "medium", KeyFrame = 48 },
            ["480x480p"] = new VideoResolution() { Width = 480, Height = 480, Name = "480x480p", BitRateMegaBit = 1, Preset = "medium", KeyFrame = 48 },
            ["320x240p"] = new VideoResolution() { Width = 320, Height = 240, Name = "320x240p", BitRateMegaBit = 1, Preset = "medium", KeyFrame = 48 },
            ["240x240p"] = new VideoResolution() { Width = 240, Height = 240, Name = "240x240p", BitRateMegaBit = 1, Preset = "medium", KeyFrame = 48 },
        };

        public static List<string> VideoResolutionAvailable = VideoResolutions.Keys.ToList();
    }
}
