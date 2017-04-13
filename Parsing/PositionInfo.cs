
namespace CyBF.Parsing
{
    public class PositionInfo
    {
        public string SourceName { get; set; }
        public string Source { get; set; }
        public int Position { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public string Line { get; set; }

        public PositionInfo(string sourceName, string source, int position, int lineNumber, int linePosition, string line)
        {
            this.SourceName = sourceName;
            this.Source = source;
            this.Position = position;
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
            this.Line = line;
        }
    }
}
