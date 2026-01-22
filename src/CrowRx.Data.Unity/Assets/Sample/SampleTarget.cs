using CrowRx.Data;

namespace Sample
{
    public class SampleTarget : ITarget<SampleSource>,ITarget<SampleSource4>
    {
        public bool UpdateBy(in SampleSource sourceData)
        {
            return true;
        }

        public bool UpdateBy(in SampleSource4 sourceData)
        {
            return true;
        }
    }
    
    public class SampleTarget2 : ITarget<SampleSource>,ITarget<SampleSource4>
    {
        public bool UpdateBy(in SampleSource sourceData)
        {
            return true;
        }

        public bool UpdateBy(in SampleSource4 sourceData)
        {
            return true;
        }
    }
}