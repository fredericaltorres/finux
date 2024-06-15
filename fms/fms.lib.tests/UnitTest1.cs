namespace fms.lib.tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var vcp = new VideoCodecCsParser();
            var result = vcp.Parse("avc1.64001f,mp4a.40.2");
        }
    }
}