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
        public string Preset { get; set; } = "medium"; // https://trac.ffmpeg.org/wiki/Encode/H.264
        public int SegmentDurationSeconds { get; set; } = 6;

        public bool BitRateInMB => BitRate.EndsWith("M");
        public bool BitRateInKB => BitRate.EndsWith("K");
        public int BitRateAsInt => int.Parse(BitRate.Replace("K", "").Replace("M", ""));
        public string MultiplyBitRateBy(int factor)
        {
            var v = BitRateAsInt * factor;
            if(BitRateInMB)
                return $"{v}M";
            else
                return $"{v}K";
        }

        public int FrameRateForDoubleBitRate => 25*2; // At 50 fps the bit rate is double.
        public bool IsSquareResolution => Width == Height;
        public bool IsPortraitResolution => Width < Height;


        public VideoResolution Clone(string newName, bool asPortrait = false)
        {
            var r = new VideoResolution()
            {
                Width = this.Width,
                Height = this.Height,
                Name = newName,
                BitRate = this.BitRate,
                KeyFrame = this.KeyFrame,
                Preset = this.Preset,
                SegmentDurationSeconds = this.SegmentDurationSeconds
            };
            if (asPortrait)
            {
                var w = r.Width;
                r.Width = this.Height;
                r.Height = w;
            }
                
            return r;
        }

        public override string ToString()
        {
            return $"{Name} {Width}x{Height}, IsSquare: {IsSquareResolution},  IsPortrait: {IsPortraitResolution}, BitRate: {this.BitRate}";
        }

        public bool IsWidthHeightPortraitResolution(int videoWidth, int videoHeight)
        {
            return videoWidth < videoHeight;
        }

        public bool CanVideoBeConvertedToResolution(int videoWidth, int videoHeight)
        {
            if(videoWidth >= Width && videoHeight >= Height)
            {
                if(this.IsSquareResolution)
                    return videoWidth == videoHeight;

                
                if (this.IsPortraitResolution && this.IsWidthHeightPortraitResolution(videoWidth, videoHeight))
                    return true;

                if (this.IsPortraitResolution && !this.IsWidthHeightPortraitResolution(videoWidth, videoHeight))
                    return false;

                if (!this.IsPortraitResolution && this.IsWidthHeightPortraitResolution(videoWidth, videoHeight))
                    return false;


                return true;
            }
            return false;
        }

        // https://ottverse.com/cbr-crf-changing-resolution-using-ffmpeg/   
        public string GetVideoMapCmd(int resolutionIndex, float frameRate)
        {
            var bitRate = this.BitRate;
            if(frameRate > this.FrameRateForDoubleBitRate)
                bitRate = this.MultiplyBitRateBy(2);

            return $@"-map ""[v{resolutionIndex}out]"" 
-c:v:0 libx264 
-x264-params ""nal-hrd=cbr:force-cfr=1"" 
-b:v:0 {bitRate} -maxrate:v:0 {bitRate} -minrate:v:0 {bitRate} 
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

        public string GetFFMPEGStreamMap(int resolutionIndex, bool hasAudioStream)
        {
            if (hasAudioStream)
                return $@"v:{resolutionIndex},a:{resolutionIndex} ";
            else 
                return $@"v:{resolutionIndex} ";
        }

        static Dictionary<string, VideoResolution> _videoResolutions = null;

        public static Dictionary<string, VideoResolution> VideoResolutions
        {
            get
            {
                if (_videoResolutions == null)
                {
                    _videoResolutions = new Dictionary<string, VideoResolution>();
                    foreach (var kv in _videoResolutionDefinitions)
                    {
                        _videoResolutions[kv.Key] = kv.Value;
                        var newKey = $"{kv.Key}-Portrait";
                        _videoResolutions[newKey] = kv.Value.Clone(newKey, asPortrait: true);
                    }
                }
                return _videoResolutions;
            }
        }

        const string DEFAULT_PRESET = "slow";
        const int DEFAULT_KEYFRAME = 48;

        // https://support.google.com/youtube/answer/2853702?hl=en

        public static Dictionary<string, VideoResolution> _videoResolutionDefinitions = new Dictionary<string, VideoResolution>()
        {
            ["FHD-4K-2160p"] = new VideoResolution() { Width = 4096, Height = 2160, Name = "FHD-4K-2160p", BitRate = "30M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            ["UHD-4K-2160p"] = new VideoResolution() { Width = 3840, Height = 2160, Name = "UHD-4K-2160p", BitRate = "30M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            ["2Kish-1664x2432p"] = new VideoResolution() { Width = 2432, Height = 1664, Name = "2Kish-1664x2432p", BitRate = "15M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            ["2K-1440p"] = new VideoResolution() { Width = 2560, Height = 1440, Name = "2K-1440p", BitRate = "15M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            // todo: support kb rate for small resolution.
            ["1080p"]       = new VideoResolution() { Width = 1920, Height = 1080, Name = "1080p",      BitRate = "10M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },
            ["1080x1080p"]  = new VideoResolution() { Width = 1080, Height = 1080, Name = "1080x1080p", BitRate = "10M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            ["1024x1024p"]  = new VideoResolution() { Width = 1024, Height = 1024, Name = "1080x1080p", BitRate = "1M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            ["720p"]        = new VideoResolution() { Width = 1280, Height = 720,  Name = "720p",       BitRate = "4M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },
            ["720x720p"]    = new VideoResolution() { Width = 720,  Height = 720,  Name = "720x720p",   BitRate = "4M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },

            ["480p"]        = new VideoResolution() { Width = 640,  Height = 480,  Name = "480p",       BitRate = "2M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },
            ["480x480p"]    = new VideoResolution() { Width = 480,  Height = 480,  Name = "480x480p",   BitRate = "1M", Preset = DEFAULT_PRESET, KeyFrame = DEFAULT_KEYFRAME },
        };

        public static List<string> VideoResolutionAvailable = VideoResolutions.Keys.ToList();
    }
}
