namespace CyBF.BFC.Model
{
    public class CyBFString
    {
        public string RawValue { get; private set; }
        public string ProcessedValue { get; private set; }

        public CyBFString(string rawValue, string processedValue)
        {
            this.RawValue = rawValue;
            this.ProcessedValue = processedValue;
        }
    }
}
