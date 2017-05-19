using CyBF.BFC.Model;
using CyBF.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CyBF.BFC.Compilation
{
    public class ModuleLibrary
    {
        private Dictionary<string, Module> _modules 
            = new Dictionary<string, Module>();

        private Dictionary<Module, IEnumerable<string>> _moduleDependencies
            = new Dictionary<Module, IEnumerable<string>>();

        private Lexer _lexer 
            = new Lexer();
        
        public IEnumerable<Module> Modules { get { return _modules.Values; } }

        public void LinkDependencies()
        {
            foreach (Module module in _modules.Values)
            {
                foreach (string dependency in _moduleDependencies[module])
                {
                    if (!_modules.ContainsKey(dependency))
                        throw new SemanticError("Module '" + dependency + "' not defined.", module.Reference);

                    module.AddDependency(_modules[dependency]);
                }
            }
        }
        
        public List<Module> GetSortedModules()
        {
            List<Module> modules = this.Modules.ToList();
            modules.Sort((m1, m2) => m2.Rank.CompareTo(m1.Rank));
            return modules;
        }

        public List<Token> GetSortedProgramTokens()
        {
            List<Token> programTokens = new List<Token>();

            foreach (Module module in this.GetSortedModules())
                programTokens.AddRange(module.Code);

            Token lastEndOfSource = programTokens[programTokens.Count - 1];
            programTokens.RemoveAll(t => t.TokenType == TokenType.EndOfSource);
            programTokens.Add(lastEndOfSource);

            return programTokens;
        }

        public void Add(Module module, IEnumerable<string> dependencies)
        {
            if (_modules.ContainsKey(module.Name))
            {
                throw new SemanticError("A module with the same name has already been defined.",
                    module.Reference, _modules[module.Name].Reference);
            }

            _modules[module.Name] = module;
            _moduleDependencies[module] = dependencies;
        }

        public void AddFromCode(string code, string source)
        {
            _lexer.SetInput(code, source);
            IEnumerable<Token> tokens = _lexer.GetAllTokens();
            Parser parser = new Parser(tokens);

            Token reference = parser.Match(TokenType.Keyword_Module);
            Token name = parser.Match(TokenType.Identifier);
            List<Token> dependencies = new List<Token>();

            if (parser.Matches(TokenType.OpenParen))
            {
                dependencies = parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    () => parser.Match(TokenType.Identifier));
            }

            parser.Match(TokenType.Semicolon);

            Module module = new Module(reference, name.TokenString, tokens);

            this.Add(module, dependencies.Select(t => t.TokenString));
        }

        public void AddFromFile(string path)
        {
            string code;

            using (var reader = new StreamReader(path))
                code = reader.ReadToEnd();

            AddFromCode(code, path);
        }

        public void AddFromFiles(string folder, string filePattern, bool recursive = false)
        {
            string[] files;

            if (recursive)
                files = Directory.GetFiles(folder, filePattern, SearchOption.AllDirectories);
            else
                files = Directory.GetFiles(folder, filePattern, SearchOption.TopDirectoryOnly);

            foreach (string file in files)
                AddFromFile(file);
        }
    }
}
