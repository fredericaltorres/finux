using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;

namespace fms
{
    public class ConversionResultBase
    {
        public string InputFile { get; set; }
        public bool Success { get; set; }
        public string FFMPEGCommandLine { get; set; }
        protected Stopwatch _stopwatch { get; set; }

        public int Duration => (int)(_stopwatch.ElapsedMilliseconds / 1000);

        public ConversionResultBase()
        {
            _stopwatch = Stopwatch.StartNew();
        }
        public void Done()
        {
            _stopwatch.Stop();
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }

    public class GifToMp4ConversionResult : ConversionResultBase
    {
        public string Mp4FileName { get; set; }

        public GifToMp4ConversionResult() : base()
        {
        }
    }

    public class AddAudioTrackResult : ConversionResultBase
    {
        public string Mp4FileName { get; set; }

        public AddAudioTrackResult() : base()
        {
        }
    }

    public class HlsConversionResult : ConversionResultBase
    {
        public string fmsVideoId { get; set; }
        public string LocalFolder { get; set; }
        public List<VideoResolution> Resolutions { get; set; }
        public string mu38MasterUrl { get; set;}

        public HlsConversionResult() : base()
        {
        }
    }
}
