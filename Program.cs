using CyBF.BFI;
using CyBF.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BufferWidth = 200;

            string bfcode;

            using (StreamReader reader = new StreamReader("bftest.txt"))
                bfcode = reader.ReadToEnd();

            BFAssembler assembler = new BFAssembler(bfcode);
            Interpreter interpreter = new Interpreter();
            Instruction[] program = assembler.Compile();

            StringBuilder assemblerOutput = new StringBuilder();
            foreach (Instruction instruction in program)
                assemblerOutput.AppendLine(instruction.ToString());

            using (StreamWriter writer = new StreamWriter("assembler output.txt"))
                writer.Write(assemblerOutput.ToString());

            interpreter.Run(program);

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
