using CyBF.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CyBF.BFC.Model.Modules
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
                parser.Next();
                dependencies.Add(parser.Match(TokenType.Identifier));

                while (parser.Matches(TokenType.Comma))
                {
                    parser.Next();
                    dependencies.Add(parser.Match(TokenType.Identifier));
                }

                parser.Matches(TokenType.CloseParen);
            }

            Module module = new Module(reference, name.Value, tokens);

            this.Add(module, dependencies.Select(t => t.Value));
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
