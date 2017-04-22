using System.Text;

namespace CyBF.BFIL
{
    public class BFStringBuilder
    {
        StringBuilder _buffer = new StringBuilder();

        public void Append(string commands)
        {
            foreach (char c in commands)
            {
                switch (c)
                {
                    case '>':
                        PushMatch('<', c);
                        break;

                    case '<':
                        PushMatch('>', c);
                        break;

                    case '+':
                        PushMatch('-', c);
                        break;

                    case '-':
                        PushMatch('+', c);
                        break;

                    default:
                        _buffer.Append(c);
                        break;
                }
            }
        }

        private void PushMatch(char opposite, char value)
        {
            if (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == opposite)
            {
                _buffer.Length--;
            }
            else
            {
                _buffer.Append(value);
            }
        }

        public override string ToString()
        {
            return _buffer.ToString();
        }
    }
}
