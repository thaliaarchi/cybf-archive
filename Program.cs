using CyBF.BFI;
using CyBF.Utility;
using CyBF.BFIL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyBF.Parsing;
using CyBF.BFC.Compilation;

namespace CyBF
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string cybfCode;
                string bfilCode;
                string bfilDebug;
                string bfCode;

                using (var reader = new StreamReader("code.txt"))
                    cybfCode = reader.ReadToEnd();

                Lexer lexer = new Lexer();
                lexer.SetInput(cybfCode, "code.txt");

                List<Token> cybfTokens = lexer.GetAllTokens();
                ModelBuilder cybfModel = new ModelBuilder(cybfTokens);
                cybfModel.ParseProgram();
                bfilCode = cybfModel.Compile();

                BFILParser bfilParser = new BFILParser(bfilCode, "code.txt");
                BFILProgram bfilProgram = bfilParser.ParseProgram();

                BFILAssembler bfilAssembler = new BFILAssembler();
                bfCode = bfilAssembler.AssembleProgram(bfilProgram, out bfilDebug);

                using (var writer = new StreamWriter("bfil_code.txt"))
                    writer.Write(bfilCode);

                using (var writer = new StreamWriter("bfil_debug.txt"))
                    writer.Write(bfilDebug);

                using (var writer = new StreamWriter("bf_code.txt"))
                    writer.Write(bfCode);

                BFAssembler bfAssembler = new BFAssembler(bfCode);
                Instruction[] instructions = bfAssembler.Compile();
                Interpreter interpreter = new Interpreter();
                AsciiConsoleStream iostream = new AsciiConsoleStream();
                interpreter.Run(instructions, iostream, iostream);
            }
            catch(CyBFException ex)
            {
                Console.Write(ex.ToString());
            }

            Console.WriteLine("\nProgram Terminated.");
            Console.ReadKey();
        }
    }
}
