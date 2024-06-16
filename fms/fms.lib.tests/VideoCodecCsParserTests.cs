namespace fms.lib.tests
{
    public class VideoCodecCsParserTests
    {
        [Fact]
        public void Parse()
        {
            var vcp = new VideoCodecCsParser();
            var input = "";
            var result = "";

            input = "avc1.42c00d,mp4a.40.2"; // bitmovin video codec
            result = vcp.Parse(input);

            input = "avc1.4D401F";
            result = vcp.Parse(input);

            input = "avc3.42E01E";
            result = vcp.Parse(input);

            input = "avc1.64001f,mp4a.40.2";
            result = vcp.Parse(input);

            input = "avc1.4d401f,mp4a.40.2";
            result = vcp.Parse(input);

            input = "avc1.64001f,mp4a.40.2";
            result = vcp.Parse(input);

            input = "avc1.640015,mp4a.40.2";
            result = vcp.Parse(input);

            input = "avc1.640028,mp4a.40.2";
            result = vcp.Parse(input);

            input = "avc1.42c01e,mp4a.40.2";
            result = vcp.Parse(input);

            input = "avc1.42c00d,mp4a.40.2";
            result = vcp.Parse(input);
        }
    }
}