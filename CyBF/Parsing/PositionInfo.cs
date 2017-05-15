
namespace CyBF.Parsing
{
    public class PositionInfo
    {
        public string Source { get; set; }
        public string Code { get; set; }
        public int Position { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public string Line { get; set; }

        public PositionInfo(string source, string code, int position, int lineNumber, int linePosition, string line)
        {
            this.Source = source;
            this.Code = code;
            this.Position = position;
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
            this.Line = line;
        }
    }
}
