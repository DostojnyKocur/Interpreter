using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.AST;
using Interpreter.Errors;
using Interpreter.Tokens;

namespace Interpreter
{
    public class Parser
    {
        private static readonly TokenType[] TermOperators = { TokenType.Plus, TokenType.Minus };
        private static readonly TokenType[] FactorOperators = { TokenType.Multiplication, TokenType.Divide, TokenType.Modulo };

        private readonly Lexer _lexer;
        private Token _currentToken = null;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        public ASTNode Parse()
        {
            var node = Program();
            if(_currentToken.Type != TokenType.EOF)
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            }

            return node;
        }

        private ASTProgram Program()
        {
            var token = _currentToken;
            return new ASTProgram(Statement(), token);
        }

        private ASTCompound CompoundStatement()
        {
            Eat(TokenType.ScopeBegin);
            var nodes = StatementList();
            Eat(TokenType.ScopeEnd);
            return new ASTCompound(nodes);
        }

        private IEnumerable<ASTNode> StatementList()
        {
            var results = new List<ASTNode>();

            results.Add(Statement());

            while (_currentToken.Type != TokenType.ScopeEnd)
            {
                results.Add(Statement());
            }

            if (_currentToken.Type == TokenType.Id)
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            }

            return results;
        }

        private ASTNode Statement()
        {
            switch(_currentToken.Type)
            {
                case TokenType.ScopeBegin:
                    return CompoundStatement();
                case TokenType.Id:
                    return StatementAssignmentFunctionCall();
                case TokenType.TypeNumber:
                    return StatementDeclarationsDefinitionsAssignments();
                default:
                    return Empty();
            }
        }

        private ASTNode StatementAssignmentFunctionCall()
        {
            var idToken = _currentToken;
            Eat(TokenType.Id);
            if (_currentToken.Type == TokenType.LeftParen)
            {
                var functionCall = FunctionCall(idToken);
                Eat(TokenType.Semicolon);
                return functionCall;
            }
            else
            {
                var variable = new ASTVariable(idToken);
                var assignmentStatement = AssignmentStatement(variable);
                Eat(TokenType.Semicolon);
                return assignmentStatement;
            }
        }

        private ASTNode StatementDeclarationsDefinitionsAssignments()
        {
            var typeToken = _currentToken; //type
            Eat(TokenType.TypeNumber);
            var idToken = _currentToken; //id
            Eat(TokenType.Id);

            var type = new ASTType(typeToken);

            if(_currentToken.Type == TokenType.Comma || 
                _currentToken.Type == TokenType.Semicolon ||
                _currentToken.Type == TokenType.Assign)
            {
                var firstVariable = new ASTVariable(idToken);
                var variablesDeclarations = VariablesDeclarations(type, firstVariable);
                if (_currentToken.Type == TokenType.Semicolon)
                {
                    Eat(TokenType.Semicolon);
                    return variablesDeclarations;
                }
                if (_currentToken.Type == TokenType.Assign)
                {
                    var assignment = AssignmentStatement(variablesDeclarations);
                    Eat(TokenType.Semicolon);
                    return assignment;
                }
            }

            if(_currentToken.Type == TokenType.LeftParen)
            {
                return FunctionDefinition(type, idToken);
            }

            ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            return null;
        }

        private ASTAssign AssignmentStatement(ASTNode leftNode = null)
        {
            var left = leftNode ?? Variable();
            var token = _currentToken;
            Eat(TokenType.Assign);
            var right = Expression();
            return new ASTAssign(left, token, right);
        }

        private ASTNode Expression()
        {
            var node = Term();

            while (TermOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Plus:
                        Eat(TokenType.Plus);
                        break;
                    case TokenType.Minus:
                        Eat(TokenType.Minus);
                        break;
                }

                node = new ASTBinaryOperator(node, token, Term());
            }

            return node;
        }

        private ASTNode Term()
        {
            var node = Factor();

            while (FactorOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Multiplication:
                        Eat(TokenType.Multiplication);
                        break;
                    case TokenType.Divide:
                        Eat(TokenType.Divide);
                        break;
                    case TokenType.Modulo:
                        Eat(TokenType.Modulo);
                        break;
                }

                node = new ASTBinaryOperator(node, token, Factor());
            }

            return node;
        }

        private ASTNode Factor()
        {
            var token = _currentToken;

            switch (token.Type)
            {
                case TokenType.Plus:
                    Eat(TokenType.Plus);
                    return new ASTUnaryOperator(token, Factor());
                case TokenType.Minus:
                    Eat(TokenType.Minus);
                    return new ASTUnaryOperator(token, Factor());
                case TokenType.ConstNumber:
                    Eat(TokenType.ConstNumber);
                    return new ASTNumber(token);
                case TokenType.LeftParen:
                    Eat(TokenType.LeftParen);
                    var node = Expression();
                    Eat(TokenType.RightParen);
                    return node;
                default:
                    Eat(TokenType.Id);
                    return new ASTVariable(token);
            }
        }

        private ASTFunctionDefinition FunctionDefinition(ASTType returnType, Token name)
        {
            var argumentList = ParameterList();
            var body = CompoundStatement();
            return new ASTFunctionDefinition(returnType, name, argumentList, body);
        }

        private List<ASTParam> ParameterList()
        {
            Eat(TokenType.LeftParen);

            var parameters = new List<ASTParam>();

            if (_currentToken.Type == TokenType.TypeNumber)
            {
                var param = new ASTParam(Type(), Variable());

                parameters.Add(param);

                while (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);

                    var anotherParam = new ASTParam(Type(), Variable());
                    parameters.Add(anotherParam);
                }
            }

            Eat(TokenType.RightParen);

            return parameters;
        }

        private ASTFunctionCall FunctionCall(Token idToken)
        {
            var functionName = idToken.Value;

            var actualParameters = new List<ASTNode>();

            Eat(TokenType.LeftParen);

            if(_currentToken.Type != TokenType.RightParen)
            {
                var node = Expression();
                actualParameters.Add(node);

                while(_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    var anotherParam = Expression();
                    actualParameters.Add(anotherParam);
                }
            }

            Eat(TokenType.RightParen);

            return new ASTFunctionCall(functionName, actualParameters, idToken);
        }

        private ASTVariablesDeclarations VariablesDeclarations(ASTType type = null, ASTVariable firstVariable = null)
        {
            var variableType = type ?? Type();
            var variables = new List<ASTVariable>();

            variables.Add(firstVariable ?? Variable());

            while(_currentToken.Type == TokenType.Comma)
            {
                Eat(TokenType.Comma);
                variables.Add(Variable());
            }

            var result = new List<ASTVariableDeclaration>();

            foreach(var variable in variables)
            {
                result.Add(new ASTVariableDeclaration(variable, variableType));
            }

            return new ASTVariablesDeclarations(result);
        }

        private ASTVariable Variable()
        {
            var node = new ASTVariable(_currentToken);
            Eat(TokenType.Id);
            return node;
        }

        private ASTType Type()
        {
            var node = new ASTType(_currentToken);
            Eat(TokenType.TypeNumber);
            return node;
        }

        private ASTEmpty Empty()
        {
            return new ASTEmpty();
        }

        private void Eat(TokenType tokenType)
        {
            if (_currentToken.Type == tokenType)
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            }
        }

        private void ThrowParsingException(ErrorCode errorCode, Token token)
        {
            var message = $"{ErrorCodes.StringRepresentatnion[errorCode]} -> {token}";
            throw new ParserError(errorCode, token, message);
        }
    }
}
