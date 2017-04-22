﻿using CyBF.BFI;
using CyBF.Utility;
using CyBF.BFIL;
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

            //try
            //{
                string code;

                using (var reader = new StreamReader("code.txt"))
                    code = reader.ReadToEnd();

                BFILParser parser = new BFILParser(code, "code.txt");
                BFILProgram program = parser.ParseProgram();
                BFILAssembler assembler = new BFILAssembler();
                string brainfuck = assembler.AssembleProgram(program);

                BFAssembler bytecodeAssembler = new BFAssembler(brainfuck);
                Instruction[] instructions = bytecodeAssembler.Compile();
                Interpreter interpreter = new Interpreter();

                AsciiConsoleStream io = new AsciiConsoleStream();
                interpreter.Run(instructions, io, io);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    Console.ReadKey();
            //}
        }
    }
}
