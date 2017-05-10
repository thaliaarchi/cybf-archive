using CyBF.BFC.Model;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Functions.Builtins;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Statements.Commands;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using CyBF.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyBF.BFC.Compilation
{
    public class ModelBuilder
    {
        private Parser _parser;
        private Dictionary<string, SystemVariable> _systemVariables;
        private StackedDictionary<string, UserVariable> _userVariables;
        private StackedDictionary<string, TypeVariable> _typeVariables;

        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
            _systemVariables = new Dictionary<string, SystemVariable>();
            _userVariables = new StackedDictionary<string, UserVariable>();
            _typeVariables = new StackedDictionary<string, TypeVariable>();
        }

        public Variable LookupVariable(Token variableNameToken)
        {
            string variableName = variableNameToken.ProcessedValue;

            SystemVariable sysvar;

            if (_systemVariables.TryGetValue(variableName, out sysvar))
                return sysvar;

            UserVariable uvar;

            if (_userVariables.TryGetValue(variableName, out uvar))
                return uvar;

            throw new SemanticError("Variable not defined.", variableNameToken);
        }

        public TypeVariable LookupTypeVariable(Token typeVariableNameToken)
        {
            string typeVariableName = typeVariableNameToken.ProcessedValue;
            TypeVariable typeVariable;

            if (!_typeVariables.TryGetValue(typeVariableName, out typeVariable))
                throw new SemanticError("Type variable not defined.", typeVariableNameToken);

            return typeVariable;
        }

        public Variable CreateEnvironmentVariable(Token variableNameToken)
        {
            string variableName = variableNameToken.ProcessedValue;
            
            if (_systemVariables.ContainsKey(variableName) ||
                _userVariables.CurrentFrame.ContainsKey(variableName))
                throw new SemanticError("Duplicate variable definition.", variableNameToken);

            UserVariable variable = new UserVariable(variableNameToken, variableName);
            _userVariables.Add(variableName, variable);

            return variable;
        }

        public CyBFProgram BuildProgram()
        {
            List<TypeDefinition> dataTypes = new List<TypeDefinition>();
            List<FunctionDefinition> functions = new List<FunctionDefinition>();
            List<Statement> statements = new List<Statement>();

            dataTypes.Add(new ByteDefinition());
            dataTypes.Add(new ConstDefinition());
            dataTypes.Add(new ArrayDefinition());
            dataTypes.Add(new VoidDefinition());
            dataTypes.Add(new StringDefinition());

            functions.Add(new BinaryMathOperatorDefinition("+", (left, right) => left + right));
            functions.Add(new BinaryMathOperatorDefinition("-", (left, right) => left - right));
            functions.Add(new BinaryMathOperatorDefinition("*", (left, right) => left * right));
            functions.Add(new BinaryMathOperatorDefinition("/", (left, right) => left / right));
            functions.Add(new BinaryMathOperatorDefinition("%", (left, right) => left % right));

            functions.Add(new UnaryMathOperatorDefinition("+", value => value));
            functions.Add(new UnaryMathOperatorDefinition("-", value => -value));

            functions.Add(new BinaryMathOperatorDefinition("<", 
                (left, right) => left < right ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition("<=",
                (left, right) => left <= right ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition("==",
                (left, right) => left == right ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition("!=",
                (left, right) => left != right ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition(">=",
                (left, right) => left >= right ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition(">",
                (left, right) => left > right ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition("&",
                (left, right) => left != 0 && right != 0 ? 1 : 0));

            functions.Add(new BinaryMathOperatorDefinition("|",
                (left, right) => left == 1 || right == 1 ? 1 : 0));

            functions.Add(new UnaryMathOperatorDefinition("!",
                value => value == 0 ? 1 : 0));

            functions.Add(new AssertFunctionDefinition());

            SystemVariable nullVariable = new SystemVariable();
            nullVariable.Value = BFObject.Null;

            SystemVariable unallocatedVariable = new SystemVariable();
            unallocatedVariable.Value = BFObject.Unallocated;

            _systemVariables.Add("_NULL_", nullVariable);
            _systemVariables.Add("_UNALLOCATED_", unallocatedVariable);

            while (!_parser.Matches(TokenType.EndOfSource))
            {
                switch (_parser.Current.TokenType)
                {
                    case TokenType.Keyword_Module:
                        ParseModuleDeclaration();
                        break;

                    case TokenType.Keyword_Struct:
                        dataTypes.Add(ParseStructDefinition());
                        break;

                    case TokenType.Keyword_Function:
                        functions.Add(ParseProcedureDefinition());
                        break;

                    case TokenType.Keyword_Selector:
                        functions.Add(ParseSelectorDefinition());
                        break;

                    default:
                        statements.Add(ParseStatement());
                        break;
                }
            }

            DefinitionLibrary<TypeDefinition> typeLibrary = new DefinitionLibrary<TypeDefinition>(dataTypes);
            DefinitionLibrary<FunctionDefinition> functionLibrary = new DefinitionLibrary<FunctionDefinition>(functions);

            return new CyBFProgram(typeLibrary, functionLibrary, statements);
        }

        public void ParseModuleDeclaration()
        {
            _parser.Match(TokenType.Keyword_Module);
            _parser.Match(TokenType.Identifier);

            if (_parser.Matches(TokenType.OpenParen))
            {
                _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    () => _parser.Match(TokenType.Identifier));
            }
        }

        public StructDefinition ParseStructDefinition()
        {
            _userVariables.Push();
            _typeVariables.Push();

            Token reference = _parser.Match(TokenType.Keyword_Struct);
            TypeConstraint constraint = ParseTypeConstraint();
            List<Variable> parameters = new List<Variable>(0);

            if (_parser.Matches(TokenType.OpenParen))
            {
                List<Token> parameterNameTokens = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    () => _parser.Match(TokenType.Identifier));

                foreach (Token parameterNameToken in parameterNameTokens)
                {
                    Variable parameter = CreateEnvironmentVariable(parameterNameToken);
                    parameters.Add(parameter);
                }
            }

            List<FieldDefinition> fields = new List<FieldDefinition>();

            while (_parser.Matches(TokenType.Identifier))
            {
                Token fieldNameToken = _parser.Next();
                string fieldName = fieldNameToken.ProcessedValue;

                _parser.Match(TokenType.Colon);

                TypeExpressionStatement fieldType = ParseTypeExpression();

                fields.Add(new FieldDefinition(fieldNameToken, fieldName, fieldType));
            }

            _typeVariables.Pop();
            _userVariables.Pop();

            List<FunctionDefinition> methods = new List<FunctionDefinition>();

            while (_parser.Matches(TokenType.Keyword_Selector, TokenType.Keyword_Function))
            {
                if (_parser.Matches(TokenType.Keyword_Selector))
                    methods.Add(ParseSelectorDefinition());
                else
                    methods.Add(ParseProcedureDefinition());
            }

            _parser.Match(TokenType.Keyword_End);

            StructDefinition definition = new StructDefinition(
                reference, constraint, parameters, fields, methods);

            return definition;
        }

        public SelectorDefinition ParseSelectorDefinition()
        {
            _userVariables.Push();
            _typeVariables.Push();

            Token referenceToken = _parser.Match(TokenType.Keyword_Selector);

            string selectorName = ParseFunctionName();

            List<FunctionParameter> parameters = _parser.ParseDelimitedList(
                TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseFunctionParameter);
            
            _parser.Match(TokenType.Colon);

            TypeExpressionStatement returnTypeExpression = ParseTypeExpression();

            List<Statement> referenceBody = ParseSelectorBodySection(TokenType.Keyword_Reference);
            List<Statement> dereferenceBody = ParseSelectorBodySection(TokenType.Keyword_Dereference);

            _parser.Match(TokenType.Keyword_End);

            SelectorDefinition definition = new SelectorDefinition(
                referenceToken, selectorName, parameters, returnTypeExpression, referenceBody, dereferenceBody);

            _typeVariables.Pop();
            _userVariables.Pop();

            return definition;
        }
        
        public List<Statement> ParseSelectorBodySection(TokenType bodyType)
        {
            _userVariables.Push();

            List<Statement> body = new List<Statement>();

            _parser.Match(bodyType);

            while (!_parser.Matches(TokenType.Keyword_End))
                body.Add(ParseStatement());
            
            _parser.Match(TokenType.Keyword_End);

            _userVariables.Pop();

            return body;
        }

        public ProcedureDefinition ParseProcedureDefinition()
        {
            _userVariables.Push();
            _typeVariables.Push();

            Token reference = _parser.Match(TokenType.Keyword_Function);

            string functionName = ParseFunctionName();
            
            List<FunctionParameter> parameters = _parser.ParseDelimitedList(
                TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseFunctionParameter);

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_Return, TokenType.Keyword_End))
                body.Add(ParseStatement());

            ExpressionStatement returnExpression;
            
            if (_parser.Matches(TokenType.Keyword_Return))
            {
                _parser.Next();
                returnExpression = ParseExpression();

                if (_parser.Matches(TokenType.Semicolon))
                    _parser.Next();
            }
            else
            {
                returnExpression = new VoidExpressionStatement(reference);
            }

            _parser.Match(TokenType.Keyword_End);

            ProcedureDefinition definition = new ProcedureDefinition(
                reference, functionName, parameters, body, returnExpression);

            _typeVariables.Pop();
            _userVariables.Pop();

            return definition;
        }
        
        public string ParseFunctionName()
        {
            string functionName;

            if (_parser.Matches(TokenType.OpenBracket))
            {
                _parser.Next();
                _parser.Match(TokenType.CloseBracket);

                functionName = "[]";
            }
            else
            {
                functionName = _parser.Match(TokenType.Identifier, TokenType.Operator).ProcessedValue;
            }

            return functionName;
        }

        public FunctionParameter ParseFunctionParameter()
        {
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = CreateEnvironmentVariable(variableNameToken);

            TypeParameter typeParameter;

            if (_parser.Matches(TokenType.Colon))
            {
                _parser.Next();
                typeParameter = ParseTypeParameter();
            }
            else
            {
                typeParameter = new TypeParameter();
            }
            
            return new FunctionParameter(variable, typeParameter);
        }

        public TypeParameter ParseTypeParameter()
        {
            if (_parser.MatchesLookahead(TokenType.TypeVariable, TokenType.Colon))
            {
                TypeVariable typeVariable = ParseTypeParameterVariable();
                _parser.Match(TokenType.Colon);
                TypeConstraint constraint = ParseTypeConstraint();

                return new ConstrainedTypeParameter(typeVariable, constraint);
            }
            else if (_parser.Matches(TokenType.TypeVariable))
            {
                TypeVariable typeVariable = ParseTypeParameterVariable();
                return new TypeParameter(typeVariable);
            }
            else
            {
                TypeConstraint constraint = ParseTypeConstraint();
                return new ConstrainedTypeParameter(constraint);
            }
        }
        
        public TypeVariable ParseTypeParameterVariable()
        {
            string typeVariableName = _parser.Match(TokenType.TypeVariable).ProcessedValue;

            TypeVariable typeVariable;

            if (!_typeVariables.TryGetValue(typeVariableName, out typeVariable))
            {
                typeVariable = new TypeVariable(typeVariableName);
                _typeVariables.Add(typeVariableName, typeVariable);
            }

            return typeVariable;
        }

        public TypeConstraint ParseTypeConstraint()
        {
            if (_parser.Matches(TokenType.Identifier))
            {
                string typeName = _parser.Next().ProcessedValue;
                return new TypeConstraint(typeName);
            }
            else
            {
                _parser.Match(TokenType.OpenBracket);

                string typeName = _parser.Next().ProcessedValue;

                List<TypeParameter> parameters = new List<TypeParameter>();

                while (!_parser.Matches(TokenType.CloseBracket))
                    parameters.Add(ParseTypeParameter());

                _parser.Match(TokenType.CloseBracket);

                return new TypeConstraint(typeName, parameters);
            }
        }

        public Statement ParseStatement(bool terminator = true)
        {
            switch (_parser.Current.TokenType)
            {
                case TokenType.Keyword_Let:

                    Statement letStatement = ParseVariableAssignmentStatement();

                    if (terminator)
                        _parser.Match(TokenType.Semicolon);

                    return letStatement;

                case TokenType.Keyword_Var:

                    Statement varStatement = ParseVariableDeclarationStatement();

                    if (terminator && _parser.Matches(TokenType.Semicolon))
                        _parser.Next();

                    return varStatement;

                case TokenType.Keyword_While:
                    return ParseWhileLoopStatement();

                case TokenType.Keyword_Do:
                    return ParseDoWhileLoopStatement();

                case TokenType.Keyword_For:
                    return ParseForLoopStatement();

                case TokenType.Keyword_Iterate:
                    return ParseIterateStatement();

                case TokenType.Keyword_If:
                    return ParseIfStatement();

                case TokenType.OpenBrace:
                    return ParseCommandBlockStatement();

                default:

                    Statement expression = ParseExpression();

                    if (terminator)
                        _parser.Match(TokenType.Semicolon);

                    return expression;
            }
        }

        public Statement ParseVariableDeclarationStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Var);
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            _parser.Match(TokenType.Colon);
            TypeExpressionStatement dataType = ParseTypeExpression();

            Variable variable = CreateEnvironmentVariable(variableNameToken);

            return new VariableDeclarationStatement(reference, variable, dataType);
        }

        public TypeExpressionStatement ParseTypeExpression()
        {
            if (_parser.Matches(TokenType.TypeVariable))
            {
                Token typeVariableNameToken = _parser.Next();
                TypeVariable variable = LookupTypeVariable(typeVariableNameToken);
                return new TypeVariableStatement(typeVariableNameToken, variable);                
            }
            
            return ParseTypeConstructor();
        }

        public TypeConstructorStatement ParseTypeConstructor()
        {
            Token reference;
            string typeName;
            List<TypeExpressionStatement> typeArguments = new List<TypeExpressionStatement>();

            if (_parser.Matches(TokenType.Identifier))
            {
                reference = _parser.Next();
                typeName = reference.ProcessedValue;
            }
            else
            {
                reference = _parser.Match(TokenType.OpenBracket);
                typeName = _parser.Match(TokenType.Identifier).ProcessedValue;

                while (!_parser.Matches(TokenType.CloseBracket))
                    typeArguments.Add(ParseTypeExpression());

                _parser.Match(TokenType.CloseBracket);
            }

            List<ExpressionStatement> valueArguments = new List<ExpressionStatement>(0);

            if (_parser.Matches(TokenType.OpenParen))
            {
                valueArguments = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseExpression);
            }

            return new TypeConstructorStatement(reference, typeName, typeArguments, valueArguments);
        }

        public Statement ParseVariableAssignmentStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Let);

            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = CreateEnvironmentVariable(variableNameToken);

            _parser.Match(TokenType.Colon);

            ExpressionStatement expression = ParseExpression();

            return new VariableAssignmentStatement(reference, variable, expression);
        }

        public Statement ParseWhileLoopStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_While);
            ExpressionStatement condition = ParseExpression();
            _parser.Match(TokenType.Colon);

            _userVariables.Push();

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_End))
                body.Add(ParseStatement());

            _userVariables.Pop();

            _parser.Match(TokenType.Keyword_End);
            
            return new WhileLoopStatement(reference, condition, body);
        }

        public Statement ParseDoWhileLoopStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Do);

            _userVariables.Push();

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_While))
                body.Add(ParseStatement());

            _userVariables.Pop();

            _parser.Match(TokenType.Keyword_While);
            ExpressionStatement condition = ParseExpression();

            _parser.Match(TokenType.Semicolon);
            
            return new DoWhileLoopStatement(reference, condition, body);
        }

        public Statement ParseForLoopStatement()
        {
            _userVariables.Push();

            Token reference = _parser.Match(TokenType.Keyword_For);
            
            Statement initializer = ParseStatement(false);
            _parser.Match(TokenType.Comma);

            ExpressionStatement condition = ParseExpression();
            _parser.Match(TokenType.Comma);

            Statement step = ParseStatement(false);

            _parser.Match(TokenType.Colon);

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_End))
                body.Add(ParseStatement());

            _parser.Match(TokenType.Keyword_End);

            _userVariables.Pop();

            return new ForLoopStatement(reference, initializer, condition, step, body);
        }

        public Statement ParseIterateStatement()
        {
            _userVariables.Push();

            Token reference = _parser.Match(TokenType.Keyword_Iterate);

            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable controlVariable = CreateEnvironmentVariable(variableNameToken);

            ExpressionStatement limitExpression = ParseExpression();

            _parser.Match(TokenType.Colon);

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_End))
                body.Add(ParseStatement());

            _parser.Match(TokenType.Keyword_End);

            _userVariables.Pop();

            return new IterateStatement(reference, controlVariable, limitExpression, body);
        }

        public Statement ParseIfStatement()
        {
            Statement statement = ParseIfStatementConditional(TokenType.Keyword_If);
            _parser.Match(TokenType.Keyword_End);
            return statement;
        }

        public Statement ParseIfStatementConditional(TokenType leadingToken)
        {
            Token reference = _parser.Match(leadingToken);
            ExpressionStatement condition = ParseExpression();
            _parser.Match(TokenType.Colon);

            _userVariables.Push();

            List<Statement> conditionalBody = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_Elif, TokenType.Keyword_Else, TokenType.Keyword_End))
                conditionalBody.Add(ParseStatement());

            _userVariables.Pop();

            List<Statement> elseBody = new List<Statement>();

            if (_parser.Matches(TokenType.Keyword_Elif))
            {
                elseBody.Add(ParseIfStatementConditional(TokenType.Keyword_Elif));
            }
            else if (_parser.Matches(TokenType.Keyword_Else))
            {
                _parser.Next();

                if (_parser.Matches(TokenType.Colon))
                    _parser.Next();

                _userVariables.Push();

                while (!_parser.Matches(TokenType.Keyword_End))
                    elseBody.Add(ParseStatement());

                _userVariables.Pop();
            }

            return new IfStatement(reference, condition, conditionalBody, elseBody);
        }

        public Statement ParseCommandBlockStatement()
        {
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenBrace);

            while (!_parser.Matches(TokenType.CloseBrace))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseBrace);

            return new CommandBlockStatement(reference, commands);
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
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenBracket);

            while (!_parser.Matches(TokenType.CloseBracket))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseBracket);

            return new LoopCommand(reference, commands);
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
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenParen);

            while (!_parser.Matches(TokenType.CloseParen))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseParen);
            _parser.Match(TokenType.Asterisk);

            ExpressionStatement counter = ParseCommandDataItem();
            
            return new RepeatCommand(reference, commands, counter);
        }

        public VariableReferenceCommand ParseVariableReferenceCommand()
        {
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = LookupVariable(variableNameToken);

            return new VariableReferenceCommand(variableNameToken, variable);
        }

        public WriteCommand ParseWriteCommand()
        {
            Token reference = _parser.Match(TokenType.Hash);
            List<ExpressionStatement> dataItems;

            if (_parser.Matches(TokenType.OpenParen))
            {
                dataItems = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    ParseCommandDataItem);
            }
            else
            {
                dataItems = new List<ExpressionStatement>() { ParseCommandDataItem() };
            }

            return new WriteCommand(reference, dataItems);
        }
        
        public ExpressionStatement ParseCommandDataItem()
        {
            if (_parser.Matches(TokenType.Identifier))
            {
                Token variableNameToken = _parser.Next();
                Variable variable = LookupVariable(variableNameToken);
                return new VariableExpressionStatement(variableNameToken, variable);
            }
            else
            {
                return ParseLiteralExpression();
            }
        }

        public ExpressionStatement ParseExpression()
        {
            ExpressionStatement left = ParseUnaryExpression();

            if (_parser.Matches(TokenType.Operator))
            {
                Token operatorToken = _parser.Next();

                ExpressionStatement right = ParseExpression();

                left = new FunctionCallExpressionStatement(
                    operatorToken, operatorToken.ProcessedValue, new ExpressionStatement[] { left, right });
            }

            return left;
        }

        public ExpressionStatement ParseUnaryExpression()
        {
            if (_parser.Matches(TokenType.Operator))
            {
                Token operatorToken = _parser.Next();
                ExpressionStatement argument = ParseUnaryExpression();

                return new FunctionCallExpressionStatement(
                    operatorToken, operatorToken.ProcessedValue, new ExpressionStatement[] { argument });
            }

            return ParseAtomicExpression();
        }
        
        public ExpressionStatement ParseAtomicExpression()
        {
            ExpressionStatement returnValue;

            if (_parser.Matches(TokenType.Numeric, TokenType.String))
                returnValue = ParseLiteralExpression();

            else if (_parser.Matches(TokenType.Keyword_Cast))
                returnValue = ParseCastExpression();

            else if (_parser.Matches(TokenType.Keyword_Sizeof))
                returnValue = ParseSizeOfExpression();

            else if (_parser.Matches(TokenType.Keyword_New))
                returnValue = ParseNewObjectExpression();

            else if (_parser.Matches(TokenType.OpenParen))
                returnValue = ParseParenthesizedExpression();

            else if (_parser.MatchesLookahead(TokenType.Identifier, TokenType.OpenParen))
                returnValue = ParseFunctionCallExpression();

            else
                returnValue = ParseVariableExpression();

            while (_parser.Matches(TokenType.Period, TokenType.OpenBracket))
                returnValue = ParseExpressionSuffix(returnValue);

            return returnValue;
        }

        public ExpressionStatement ParseExpressionSuffix(ExpressionStatement source)
        {
            if (_parser.Matches(TokenType.OpenBracket))
            {
                Token reference = _parser.Current;
                
                List<ExpressionStatement> indexArguments = _parser.ParseDelimitedList(
                    TokenType.OpenBracket, TokenType.Comma, TokenType.CloseBracket, ParseExpression);

                return new FunctionCallExpressionStatement(reference, "[]",
                    new ExpressionStatement[] { source }.Concat(indexArguments));
            }
            else
            {
                _parser.Match(TokenType.Period);
                
                if (_parser.MatchesLookahead(TokenType.Identifier, TokenType.OpenParen))
                {
                    Token methodNameToken = _parser.Match(TokenType.Identifier);

                    List<ExpressionStatement> arguments = _parser.ParseDelimitedList(
                        TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseExpression);

                    return new MethodCallExpressionStatement(
                        methodNameToken, methodNameToken.ProcessedValue, source, arguments);
                }
                else
                {
                    Token fieldNameToken = _parser.Match(TokenType.Identifier);
                    string fieldName = fieldNameToken.ProcessedValue;

                    return new FieldExpressionStatement(fieldNameToken, source, fieldName);
                }
            }
        }

        public ExpressionStatement ParseCastExpression()
        {
            Token reference = _parser.Match(TokenType.Keyword_Cast);
            ExpressionStatement sourceExpression = ParseExpression();
            _parser.Match(TokenType.Colon);
            TypeExpressionStatement targetTypeExpression = ParseTypeExpression();

            return new CastExpressionStatement(reference, sourceExpression, targetTypeExpression);
        }

        public ExpressionStatement ParseSizeOfExpression()
        {
            Token reference = _parser.Match(TokenType.Keyword_Sizeof);

            if (_parser.Matches(TokenType.OpenParen))
            {
                _parser.Next();
                ExpressionStatement expression = ParseExpression();
                _parser.Match(TokenType.CloseParen);

                return new SizeOfExpressionStatement(reference, expression);
            }
            else
            {
                TypeExpressionStatement typeExpression = ParseTypeExpression();
                return new SizeOfExpressionStatement(reference, typeExpression);
            }
        }

        public ExpressionStatement ParseNewObjectExpression()
        {
            Token reference = _parser.Match(TokenType.Keyword_New);
            TypeExpressionStatement typeExpression = ParseTypeExpression();

            return new NewObjectExpressionStatement(reference, typeExpression);
        }

        public ExpressionStatement ParseFunctionCallExpression()
        {
            Token nameToken = _parser.Match(TokenType.Identifier);

            List<ExpressionStatement> arguments = _parser.ParseDelimitedList(
                TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseExpression);
            
            return new FunctionCallExpressionStatement(nameToken, nameToken.ProcessedValue, arguments);
        }

        public ExpressionStatement ParseVariableExpression()
        {
            Token nameToken = _parser.Match(TokenType.Identifier);
            Variable variable = LookupVariable(nameToken);
            return new VariableExpressionStatement(nameToken, variable);
        }

        public ExpressionStatement ParseParenthesizedExpression()
        {
            _parser.Match(TokenType.OpenParen);
            ExpressionStatement result = ParseExpression();
            _parser.Match(TokenType.CloseParen);

            return result;
        }

        public ExpressionStatement ParseLiteralExpression()
        {
            Token token = _parser.Match(TokenType.String, TokenType.Numeric);

            if (token.TokenType == TokenType.String)
                return new StringExpressionStatement(token, token.RawValue, token.ProcessedValue);
            else
                return new ConstExpressionStatement(token, token.NumericValue);
        }
    }
}
