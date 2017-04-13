using System;
using System.Text.RegularExpressions;

namespace CyBF.Parsing
{
    public class Scanner
    {
        public string SourceName { get; private set; }
        public string Source { get; private set; }
        public int Position { get; private set; }
        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }
        public string Line { get; private set; }

        public int CurrentValue { get; private set; }

        public bool EndOfSource { get { return this.CurrentValue == -1; } }

        public Scanner(string sourceName, string source)
        {
            this.SourceName = sourceName;
            this.Source = source;
            this.Position = 0;
            this.LineNumber = 1;
            this.LinePosition = 0;
            this.Line = GetCurrentLine();
            this.CurrentValue = GetCurrentValue();
        }

        public PositionInfo GetPositionInfo()
        {
            return new PositionInfo(this.SourceName, this.Source, this.Position, this.LineNumber, this.LinePosition, this.Line);
        }
        
        public bool ReadPattern(Regex regex, out string result)
        {
            Match match = regex.Match(this.Source, this.Position);
            
            if (match.Success)
            {
                result = match.Value;
                this.AdvancePosition(this.Position + result.Length);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private void AdvancePosition(int newPosition)
        {
            int oldPosition = this.Position;

            if (newPosition <= oldPosition || this.Source.Length < newPosition)
                throw new ArgumentOutOfRangeException("newPosition");

            this.Position = newPosition;

            for (int i = oldPosition; i < newPosition; i++)
            {
                this.LinePosition++;

                if (this.Source[i] == '\n')
                {
                    this.LinePosition = 0;
                    this.LineNumber++;
                }
            }

            this.Line = GetCurrentLine();
            this.CurrentValue = GetCurrentValue();
        }

        private string GetCurrentLine()
        {
            int startIndex;
            int endIndex;
            int length;

            if (this.Source == string.Empty)
                return string.Empty;

            if (this.Position == 0)
            {
                startIndex = 0;
                endIndex = this.Source.IndexOf('\n');

                if (endIndex == -1)
                    endIndex = this.Source.Length - 1;
            }
            else if (this.Position < this.Source.Length)
            {
                startIndex = this.Source.LastIndexOf('\n', this.Position - 1) + 1;
                endIndex = this.Source.IndexOf('\n', this.Position);

                if (endIndex == -1)
                    endIndex = this.Source.Length - 1;
            }
            else
            {
                if (this.Source.Length == 1)
                    startIndex = 0;
                else
                    startIndex = this.Source.LastIndexOf('\n', this.Source.Length - 2) + 1;

                endIndex = this.Source.Length - 1;
            }

            length = endIndex - startIndex + 1;

            return this.Source.Substring(startIndex, length);
        }

        private int GetCurrentValue()
        {
            if (this.Position < this.Source.Length)
                return this.Source[this.Position];
            else
                return -1;
        }
    }
}
