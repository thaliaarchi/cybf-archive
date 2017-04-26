using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Statements.Commands;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFC.Compilation
{
    /*

    Statement:

        VariableAllocation
        VariableAssignment
        IfStatement
        WhileLoop
        Expression
        CommandBlock

    VariableAllocation:

        'var' Identifier ':' TypeExpression

    VariableAssignment:
        
        'let' Identifier ':' Expression

    IfStatement:
        
        'if' Expression ':' 
            Statement*
        (
        'elif' Expression ':'
            Statement*
        )*
        (
        'else' ':'
            Statement*
        )
        'end'

    WhileLoop:
        'while' Expression ':'
            Statement*
        'end'

    Expression:
        
        FunctionCall
        NumericLiteral
        StringLiteral
        Symbol

    CommandBlock:

        '{' (LoopCommand | OperatorString | Repeater | ReferenceCommand | WriteCommand)* '}'

    Where do I start?
    
    Command blocks seems to be the obvious place.
    The subset of BFIL that CyBF is to support.
    
    
    */

    public class ModelBuilder
    {
        private Parser _parser;
        private List<Statement> _globalStatements;
        private List<FunctionDefinition> _functions;
        private List<TypeDefinition> _types;
        private SymbolTable _symbols;

        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
            _globalStatements = new List<Statement>();
            _functions = new List<FunctionDefinition>();
            _types = new List<TypeDefinition>();
            _symbols = new SymbolTable();
        }

        public CommandBlockStatement ParseCommandBlockStatement()
        {
            Token reference = _parser.Match(TokenType.OpenBrace);
            List<Command> commands = _parser.ParseTerminatedList(TokenType.CloseBrace, ParseCommand);
            _parser.Match(TokenType.CloseBrace);

            return new CommandBlockStatement(reference, commands);
        }

        public Command ParseCommand()
        {
            /*
                I haven't really planned out the Command object model yet.
                Also, I think it's wrong to have temp variable creation as part of 
                the BFCompiler object, considering that it's during model
                building that we need them. 
            */

            throw new NotImplementedException();
        }
    }
}
