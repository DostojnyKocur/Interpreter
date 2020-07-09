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
        private static readonly TokenType[] CompareOperators = { TokenType.Equal, TokenType.NotEqual, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual };

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
                case TokenType.Return:
                    return ReturnStatement();
                case TokenType.If:
                    return IfElseStatement();
                case TokenType.While:
                    return WhileStatement();
                default:
                    return Empty();
            }
        }

        private ASTIfElse IfElseStatement()
        {
            var token = _currentToken;
            Eat(TokenType.If);
            Eat(TokenType.LeftParen);
            var condition = Condition();
            Eat(TokenType.RightParen);
            var ifTrue = Statement();
            if(_currentToken.Type == TokenType.Else)
            {
                Eat(TokenType.Else);
                var @else = Statement();
                return new ASTIfElse(token, condition, ifTrue, @else);
            }

            return new ASTIfElse(token, condition, ifTrue, null);
        }

        private ASTWhile WhileStatement()
        {
            var token = _currentToken;
            Eat(TokenType.While);
            Eat(TokenType.LeftParen);
            var condition = Condition();
            Eat(TokenType.RightParen);
            var body = Statement();

            return new ASTWhile(token, condition, body);
        }

        private ASTNode ReturnStatement()
        {
            var returnToken = _currentToken;
            Eat(TokenType.Return);
            if (_currentToken.Type == TokenType.Semicolon)
            {
                var result = new ASTReturn(returnToken, null);
                Eat(TokenType.Semicolon);
                return result;
            }
            else
            {
                var result = new ASTReturn(returnToken, Expression());
                Eat(TokenType.Semicolon);
                return result;
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

        private ASTNode Condition()
        {
            return OrCondition();
        }

        private ASTNode OrCondition()
        {
            var node = AndCondition();

            while (_currentToken.Type == TokenType.Or)
            {
                var token = _currentToken;

                Eat(TokenType.Or);

                node = new ASTBinaryOperator(node, token, AndCondition());
            }

            return node;
        }

        private ASTNode AndCondition()
        {
            var node = NotCondition();

            while(_currentToken.Type == TokenType.And)
            {
                var token = _currentToken;

                Eat(TokenType.And);

                node = new ASTBinaryOperator(node, token, NotCondition());
            }

            return node;
        }

        private ASTNode NotCondition()
        {

            if(_currentToken.Type == TokenType.Not)
            {
                var token = _currentToken;
                Eat(TokenType.Not);
                return new ASTUnaryOperator(token, NotCondition());
            }
            else if (_currentToken.Type == TokenType.LeftParen)
            {
                Eat(TokenType.LeftParen);
                var condition = Condition();
                Eat(TokenType.RightParen);
                return condition;
            }
            return Comparision();
        }

        private ASTNode Comparision()
        {
            var node = Expression();

            while(CompareOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                Eat(token.Type);

                node = new ASTBinaryOperator(node, token, Expression());
            }

            return node;
        }

        private ASTNode Term()
        {
            var node = Factor();

            while (FactorOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                Eat(token.Type);

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
                case TokenType.Id:
                    if (_lexer.CurrentChar == '(')
                    {
                        Eat(TokenType.Id);
                        return FunctionCall(token);
                    }
                    else
                    {
                        Eat(TokenType.Id);
                        return new ASTVariable(token);
                    }
            }

            ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            return null;
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

            if(variableType.Name == "void")
            {
                ThrowParsingException(ErrorCode.IncorrectType, variableType.Token);
            }

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
