using CyBF.BFC.Model;
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
    
        ModelBuilder is really more than just a parser.
        ParseRepeatCommand seems to be an example where intermediate
        Statement objects need to be created and saved somewhere.

        This isn't a statement I'm "Parsing".

        I could compile directly from a token stream...
        Well... except I need to build up some kind of model anyway
        in order to create the Definition objects.

        Sleep on it. I'm so damned close.
        
        Once I figure out the relationship between the parser and semantic model,
        I think the rest of CyBF will be easy. Oh, except the UI. But that'll
        be easy enough.

        Question - do I add "Statement" objects in between Command objects?
        Trivial example is for repeaters.
        Variable references potentially a more complex example.

            -> No. The "Statement" will occur before the command,
            since Commands themselves aren't statements. 

        

    */

    public class ModelBuilder
    {
        private Parser _parser;
        private List<Statement> _globalStatements;
        private List<FunctionDefinition> _functions;
        private List<TypeDefinition> _types;
        private SymbolTable _symbols;
        private int _variableAutonum;

        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
            _globalStatements = new List<Statement>();
            _functions = new List<FunctionDefinition>();
            _types = new List<TypeDefinition>();
            _symbols = new SymbolTable();
            
            _variableAutonum = 1;
        }

        public Variable NewVariable()
        {
            return new Variable("v" + (_variableAutonum++).ToString());
        }

        public CommandBlockStatement ParseCommandBlockStatement()
        {
            //Token reference = _parser.Match(TokenType.OpenBrace);
            //List<Command> commands = _parser.ParseTerminatedList(ParseCommand, TokenType.CloseBrace);
            //_parser.Match(TokenType.CloseBrace);

            //return new CommandBlockStatement(reference, commands);
            throw new NotImplementedException();
        }

        public Command ParseCommand()
        {
            if (_parser.Matches(TokenType.OpenBracket))
                return ParseLoopCommand();

            if (_parser.Matches(TokenType.OpenParen))
                return ParseRepeatCommand();

            if (_parser.Matches(TokenType.Identifier))
                return ParseVariableReferenceCommand();

            if (_parser.Matches(TokenType.Hash))
                return ParseWriteCommand();

            return ParseOperatorStringCommand();
        }

        public LoopCommand ParseLoopCommand()
        {
            //Token reference = _parser.Match(TokenType.OpenBracket);
            //List<Command> commands = _parser.ParseTerminatedList(ParseCommand, TokenType.CloseBracket);
            //_parser.Match(TokenType.CloseBracket);

            //return new LoopCommand(reference, commands);
            throw new NotImplementedException();
        }

        public OperatorStringCommand ParseOperatorStringCommand()
        {
            StringBuilder opstring = new StringBuilder();

            TokenType[] operatorStringTokens = new TokenType[]
            {
                TokenType.Plus,
                TokenType.Minus,
                TokenType.OpenAngle,
                TokenType.CloseAngle,
                TokenType.Comma,
                TokenType.Period
            };

            Token reference = _parser.Match(operatorStringTokens);
            opstring.Append(reference.ProcessedValue);

            while (_parser.Matches(operatorStringTokens))
                opstring.Append(_parser.Next().ProcessedValue);

            return new OperatorStringCommand(reference, opstring.ToString());
        }

        public RepeatCommand ParseRepeatCommand()
        {
            //Token reference = _parser.Match(TokenType.OpenParen);
            //List<Command> commands = _parser.ParseTerminatedList(ParseCommand, TokenType.CloseParen);
            //_parser.Match(TokenType.CloseParen);
            //_parser.Match(TokenType.Asterisk);

            //Variable counter = null;

            //if (_parser.Matches(TokenType.Identifier))
            //{
            //    // Lookup counter from the symbol table.
            //}
            //else
            //{
            //    // Match a numeric token. Then what?
            //}

            //return new RepeatCommand(reference, commands, counter);
            throw new NotImplementedException();
        }

        public VariableReferenceCommand ParseVariableReferenceCommand()
        {
            throw new NotImplementedException();
        }

        public WriteCommand ParseWriteCommand()
        {
            throw new NotImplementedException();
        }
    }
}
