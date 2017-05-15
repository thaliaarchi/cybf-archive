using System;
using System.Text.RegularExpressions;

namespace CyBF.Parsing
{
    public class Scanner
    {
        public string Source { get; private set; }
        public string Code { get; private set; }
        public int Position { get; private set; }
        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }
        public string Line { get; private set; }

        public int CurrentValue { get; private set; }

        public bool EndOfSource { get { return this.CurrentValue == -1; } }

        public Scanner(string code, string source)
        {
            this.Source = source;
            this.Code = code;
            this.Position = 0;
            this.LineNumber = 1;
            this.LinePosition = 0;
            this.Line = GetCurrentLine();
            this.CurrentValue = GetCurrentValue();
        }

        public PositionInfo GetPositionInfo()
        {
            return new PositionInfo(this.Source, this.Code, this.Position, this.LineNumber, this.LinePosition, this.Line);
        }
        
        public bool ReadPattern(Regex regex, out string result)
        {
            Match match = regex.Match(this.Code, this.Position);
            
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

            if (newPosition <= oldPosition || this.Code.Length < newPosition)
                throw new ArgumentOutOfRangeException("newPosition");

            this.Position = newPosition;

            for (int i = oldPosition; i < newPosition; i++)
            {
                this.LinePosition++;

                if (this.Code[i] == '\n')
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

            if (this.Code == string.Empty)
                return string.Empty;

            if (this.Position == 0)
            {
                startIndex = 0;
                endIndex = this.Code.IndexOf('\n');

                if (endIndex == -1)
                    endIndex = this.Code.Length - 1;
            }
            else if (this.Position < this.Code.Length)
            {
                startIndex = this.Code.LastIndexOf('\n', this.Position - 1) + 1;
                endIndex = this.Code.IndexOf('\n', this.Position);

                if (endIndex == -1)
                    endIndex = this.Code.Length - 1;
            }
            else
            {
                if (this.Code.Length == 1)
                    startIndex = 0;
                else
                    startIndex = this.Code.LastIndexOf('\n', this.Code.Length - 2) + 1;

                endIndex = this.Code.Length - 1;
            }

            length = endIndex - startIndex + 1;

            return this.Code.Substring(startIndex, length);
        }

        private int GetCurrentValue()
        {
            if (this.Position < this.Code.Length)
                return this.Code[this.Position];
            else
                return -1;
        }
    }
}
