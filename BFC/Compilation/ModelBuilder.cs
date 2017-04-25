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

    */

    public class ModelBuilder
    {
        Parser _parser;

        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
        }
    }
}
