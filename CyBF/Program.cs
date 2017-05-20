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
using CyBF.BFC.Model.Types.Instances;

namespace CyBF
{
    class Program
    {
        static string _workingFolder;
        static IEnumerable<string> _filePatterns;

        static bool _recursive;
        static bool _debug;
        static bool _output;
        static bool _run;
        static bool _fileio;

        static string _codeOutputFile;
        static string _dataInputFile;
        static string _dataOutputFile;

        static string _debugRawBFILOutputFile;
        static string _debugProcessedBFILOutputFile;
        static string _debugBFASMOutputFile;

        static void Main(string[] args)
        {
            //args = new string[]
            //{
            //    ".\\Program",
            //    "*.cbf",
            //    "-output",
            //    "code.txt",
            //    "-debug",
            //    "-run"
            //};

            //args = new string[]
            //{
            //    @"..\..\..\CyBF Library",
            //    "*.cbf",
            //    @"stdlib\*.cbf",
            //    "-output",
            //    "code.txt",
            //    "-debug",
            //    "-run"
            //};

            try
            {
                if (args.Length == 0 || args[0] == "-help")
                {
                    PrintHelp();
                    return;
                }

                LoadSettings(args);

                Console.WriteLine("Loading Program...");

                ModuleLibrary moduleLibrary = BuildModuleLibrary();
                List<Token> programTokens = moduleLibrary.GetSortedProgramTokens();

                if (programTokens.Count == 0)
                    throw new ArgumentException("No source code found.");

                Console.WriteLine("Building Model...");

                ModelBuilder modelBuilder = new ModelBuilder(programTokens);
                CyBFProgram cybfProgram = modelBuilder.BuildProgram();

                Console.WriteLine("Compiling to BFIL Model...");
                
                BFILProgram bfilProgram = cybfProgram.Compile();
                BFILAssembler bfilAssembler = new BFILAssembler();

                string processedBFIL;
                int allocationMaximum;

                Console.WriteLine("Assembling...");

                string bfcode = bfilAssembler.AssembleProgram(bfilProgram, out processedBFIL, out allocationMaximum);

                if (_debug)
                    WriteFile(_debugProcessedBFILOutputFile, processedBFIL);

                if (_output)
                    WriteFile(_codeOutputFile, bfcode);

                Console.WriteLine("Generating Interpreter Instructions...");

                BFAssembler bfAssembler = new BFAssembler(bfcode);
                Instruction[] instructions = bfAssembler.Compile();

                if (_debug)
                    WriteFile(_debugBFASMOutputFile, string.Join("\n", instructions));

                Console.WriteLine("Program Requires " + allocationMaximum.ToString() + " Bytes Memory.");

                Console.WriteLine("Executing Interpreter...");

                if (_run)
                    RunInterpreter(instructions);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.GetType().Name + ": " + e.Message);
                Console.WriteLine("Press any key to quit...");
                Console.ReadKey();
            }
        }

        static void RunInterpreter(Instruction[] instructions)
        {
            try
            {
                Interpreter interpreter = new Interpreter();
                Stream input;
                Stream output;

                if (_fileio)
                {
                    input = new FileStream(_dataInputFile, FileMode.Open);
                    output = new FileStream(_dataOutputFile, FileMode.Create);
                }
                else
                {
                    input = output = new AsciiConsoleStream();
                }

                using (input)
                using (output)
                {
                    interpreter.Run(instructions, input, output);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.GetType().Name + ": " + e.Message);
            }

            Console.WriteLine("\nProgram Terminated. Press any key to quit...");
            Console.ReadKey();
        }

        static void WriteFile(string path, string content)
        {
            using (var writer = new StreamWriter(path))
                writer.Write(content);
        }

        static ModuleLibrary BuildModuleLibrary()
        {
            ModuleLibrary library = new ModuleLibrary();

            foreach (string pattern in _filePatterns)
                library.AddFromFiles(_workingFolder, pattern, _recursive);

            library.LinkDependencies();

            return library;
        }

        static void LoadSettings(string[] args)
        {
            _workingFolder = args[0];
            _filePatterns = args.Skip(1).TakeWhile(arg => !arg.StartsWith("-"));
            _recursive = args.Contains("-recursive");
            _output = args.Contains("-output");
            _debug = _output && args.Contains("-debug");
            _run = args.Contains("-run");
            _fileio = args.Contains("-fileio");
            
            List<string> argslist = new List<string>(args);

            if (_output)
            {
                int codeOutputFileIndex = argslist.IndexOf("-output") + 1;
                string codeOutputFileName;

                if (codeOutputFileIndex < args.Length)
                    codeOutputFileName = args[codeOutputFileIndex];
                else
                    throw new ArgumentException("Invalid command line.");

                _codeOutputFile = Path.Combine(_workingFolder, codeOutputFileName);
                _debugRawBFILOutputFile = Path.Combine(_workingFolder, "RAW_" + codeOutputFileName);
                _debugProcessedBFILOutputFile = Path.Combine(_workingFolder, "BFIL_" + codeOutputFileName);
                _debugBFASMOutputFile = Path.Combine(_workingFolder, "ASM_" + codeOutputFileName);
            }

            if (_fileio)
            {
                int dataInputFileIndex = argslist.IndexOf("-fileio") + 1;
                int dataOutputFileIndex = argslist.IndexOf("-fileio") + 2;

                if (dataOutputFileIndex < args.Length)
                {
                    _dataInputFile = args[dataInputFileIndex];
                    _dataOutputFile = args[dataOutputFileIndex];
                }
                else
                {
                    throw new ArgumentException("Invalid command line.");
                }
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("cybf workingFolder filePattern* -recursive -output outputFile\n     -debug -run -fileio inputDataFile outputDataFile");
            Console.WriteLine();
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}
