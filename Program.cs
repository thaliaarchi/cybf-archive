using CyBF.BFI;
using CyBF.Utility;
using CyBF.BFIL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CyBF.Parsing;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model;

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

                using (var reader = new StreamReader("code.cbf"))
                    cybfCode = reader.ReadToEnd();

                Lexer lexer = new Lexer();
                lexer.SetInput(cybfCode, "code.cbf");

                List<Token> cybfTokens = lexer.GetAllTokens();
                ModelBuilder cybfModel = new ModelBuilder(cybfTokens);
                CyBFProgram cybfProgram = cybfModel.BuildProgram();
                bfilCode = cybfProgram.Compile();

                BFILParser bfilParser = new BFILParser(bfilCode, "code.cbf");
                BFILProgram bfilProgram = bfilParser.ParseProgram();

                BFILAssembler bfilAssembler = new BFILAssembler();
                int allocationMaximum;
                bfCode = bfilAssembler.AssembleProgram(bfilProgram, out bfilDebug, out allocationMaximum);

                using (var writer = new StreamWriter("bfil_code.txt"))
                    writer.Write(bfilCode);

                using (var writer = new StreamWriter("bfil_debug.txt"))
                    writer.Write(bfilDebug);

                using (var writer = new StreamWriter("bf_code.txt"))
                    writer.Write(bfCode);

                BFAssembler bfAssembler = new BFAssembler(bfCode);
                Instruction[] instructions = bfAssembler.Compile();

                using (var writer = new StreamWriter("bfasm_code.txt"))
                {
                    foreach (Instruction instruction in instructions)
                        writer.WriteLine(instruction.ToString());
                }
                
                Interpreter interpreter = new Interpreter();
                AsciiConsoleStream iostream = new AsciiConsoleStream();

                Console.WriteLine("Running Program. Allocates " + allocationMaximum.ToString() + " bytes.");
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
