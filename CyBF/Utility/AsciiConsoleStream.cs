using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.Utility
{
    public class AsciiConsoleStream : Stream
    {
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public AsciiConsoleStream()
        {
            Console.InputEncoding = Encoding.ASCII;
            Console.OutputEncoding = Encoding.ASCII;
        }

        public override void Flush()
        {
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset < 0 || count < 0 || buffer.Length <= offset + count)
                throw new ArgumentOutOfRangeException();

            char[] chars = new char[count];

            for (int i = 0; i < count; i++)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                    chars[i] = '\n';
                else
                    chars[i] = key.KeyChar;
            }
            
            byte[] bytes = Encoding.ASCII.GetBytes(chars);
            Array.Copy(bytes, 0, buffer, offset, count);

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset < 0 || count < 0 || buffer.Length <= offset + count)
                throw new ArgumentOutOfRangeException();

            char[] chars = Encoding.ASCII.GetChars(buffer, offset, count);
            Console.Write(chars);
        }
    }
}
