using System.Collections.Generic;
using Interpreter.Common.AST;
using Interpreter.Common.Errors;
using Interpreter.Common.Tokens;
using Interpreter.LexerService;

namespace Interpreter.ParserService
{
    public partial class Parser : IParser
    {
        private static readonly TokenType[] TermOperators = { TokenType.Plus, TokenType.Minus };
        private static readonly TokenType[] FactorOperators = { TokenType.Multiplication, TokenType.Divide, TokenType.Modulo };
        private static readonly TokenType[] CompareOperators = { TokenType.Equal, TokenType.NotEqual, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual };
        private static readonly TokenType[] BuilinVarTypes = { TokenType.TypeNumber, TokenType.TypeBool, TokenType.TypeString };

        private readonly ILexer _lexer;
        private Token _currentToken = null;
        private Token _secondToken = null;
        private Token _thirdToken = null;
        private Token _fourthToken = null;
        private Token _fifthToken = null;

        public Parser(ILexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
            _secondToken = _lexer.GetNextToken();
            _thirdToken = _lexer.GetNextToken();
            _fourthToken = _lexer.GetNextToken();
            _fifthToken = _lexer.GetNextToken();
        }

        public ASTNode Parse()
        {
            var node = Program();
            if (_currentToken.Type != TokenType.EOF)
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken, TokenType.EOF);
            }

            return node;
        }

        private ASTProgram Program()
        {
            return new ASTProgram(_currentToken, Statement());
        }

        private ASTBlock Block()
        {
            var token = _currentToken;
            Eat(TokenType.ScopeBegin);
            var nodes = StatementList();
            Eat(TokenType.ScopeEnd);
            return new ASTBlock(token, nodes);
        }

        private IEnumerable<ASTNode> StatementList()
        {
            var results = new List<ASTNode>();

            results.Add(Statement());

            while (_currentToken.Type != TokenType.ScopeEnd)
            {
                results.Add(Statement());
            }

            return results;
        }

        private ASTNode Statement()
        {
            switch (_currentToken.Type)
            {
                case TokenType.ScopeBegin:
                    return Block();
                case TokenType.Identifier:
                    var resultIdentifier = _secondToken.Type == TokenType.LeftParen ? FunctionCall() : (ASTNode)AssignmentStatement(Variable());
                    Eat(TokenType.Semicolon);
                    return resultIdentifier;
                case TokenType.TypeVoid:
                case TokenType.TypeNumber:
                case TokenType.TypeBool:
                case TokenType.TypeString:
                    if (_thirdToken.Type == TokenType.LeftParen ||
                        (_secondToken.Type == TokenType.LeftBracket && _fifthToken.Type == TokenType.LeftParen))
                    {
                        return FunctionDefinition();
                    }

                    ASTNode resultType = VariablesDeclarations();
                    if (_currentToken.Type == TokenType.Assign)
                    {
                        resultType = AssignmentStatement(resultType);
                    }
                    Eat(TokenType.Semicolon);
                    return resultType;
                case TokenType.Return:
                    return ReturnStatement();
                case TokenType.Break:
                    return BreakStatement();
                case TokenType.Continue:
                    return ContinueStatement();
                case TokenType.If:
                    return IfElseStatement();
                case TokenType.While:
                    return WhileStatement();
                case TokenType.For:
                    return ForStatement();
                default:
                    return Empty();
            }
        }

        private ASTAssign AssignmentStatement(ASTNode leftNode)
        {
            var token = _currentToken;
            Eat(TokenType.Assign);
            var right = _currentToken.Type == TokenType.LeftBracket ? ArrayInitialization() : ArithmeticExpression();
            return new ASTAssign(leftNode, token, right);
        }

        private ASTArrayInitialization ArrayInitialization()
        {
            var token = _currentToken;
            Eat(TokenType.LeftBracket);

            var items = new List<ASTNode>();

            while (_currentToken.Type != TokenType.RigthBracket)
            {
                items.Add(ArithmeticExpression());
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }

            Eat(TokenType.RigthBracket);

            return new ASTArrayInitialization(token, items);
        }
 
        private ASTFunctionDefinition FunctionDefinition()
        {
            var returnType = Type();

            var name = _currentToken;
            Eat(TokenType.Identifier);

            Eat(TokenType.LeftParen);
            var argumentList = ParameterList();
            Eat(TokenType.RightParen);

            var body = Block();
            return new ASTFunctionDefinition(name, returnType, argumentList, body);
        }

        private List<ASTParam> ParameterList()
        {
            

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

            return parameters;
        }

        private ASTFunctionCall FunctionCall()
        {
            var idToken = _currentToken;
            Eat(TokenType.Identifier);

            var functionName = idToken.Value;

            var actualParameters = new List<ASTNode>();

            Eat(TokenType.LeftParen);

            if (_currentToken.Type != TokenType.RightParen)
            {
                var node = ArithmeticExpression();
                actualParameters.Add(node);

                while (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    var anotherParam = ArithmeticExpression();
                    actualParameters.Add(anotherParam);
                }
            }

            Eat(TokenType.RightParen);

            return new ASTFunctionCall(idToken, functionName, actualParameters);
        }

        private ASTVariablesDeclarations VariablesDeclarations()
        {
            var variableType = Type();

            if (variableType.Token.Type == TokenType.TypeVoid)
            {
                ThrowParsingException(ErrorCode.IncorrectType, variableType.Token);
            }

            var variables = new List<ASTVariable>
            {
                Variable()
            };

            while (_currentToken.Type == TokenType.Comma)
            {
                Eat(TokenType.Comma);
                variables.Add(Variable());
            }

            var result = new List<ASTVariableDeclaration>();

            foreach (var variable in variables)
            {
                result.Add(new ASTVariableDeclaration(variableType, variable));
            }

            return new ASTVariablesDeclarations(result);
        }

        private ASTVariable Variable()
        {
            var token = _currentToken;
            Eat(TokenType.Identifier);

            ASTNode indexFrom = null;
            ASTNode indexTo = null;
            ASTNode indexStep = null;
            if (_currentToken.Type == TokenType.LeftBracket)
            {
                Eat(TokenType.LeftBracket);
                if (_currentToken.Type == TokenType.Colon)
                {
                    indexFrom = new ASTNumber(new Token(TokenType.ConstNumber, "0", _currentToken.LineNumber, _currentToken.Column));
                }
                else
                {
                    indexFrom = ArithmeticExpression();
                }

                if(_currentToken.Type == TokenType.Colon)
                {
                    Eat(TokenType.Colon);
                    if (_currentToken.Type == TokenType.Colon || _currentToken.Type == TokenType.RigthBracket)
                    {
                        indexTo = new ASTNumber(new Token(TokenType.ConstNumber, $"{int.MaxValue}", _currentToken.LineNumber, _currentToken.Column));
                    }
                    else
                    {
                        indexTo = ArithmeticExpression();
                    }
                }

                if (_currentToken.Type == TokenType.Colon)
                {
                    Eat(TokenType.Colon);
                    indexStep = ArithmeticExpression();
                }

                Eat(TokenType.RigthBracket);
            }

            return new ASTVariable(token, indexFrom, indexTo, indexStep);
        }

        private ASTEmpty Empty()
        {
            return new ASTEmpty();
        }

        private void Eat(TokenType tokenType)
        {
            if (_currentToken.Type == tokenType)
            {
                _currentToken = _secondToken;
                _secondToken = _thirdToken;
                _thirdToken = _fourthToken;
                _fourthToken = _fifthToken;
                _fifthToken = _lexer.GetNextToken();
            }
            else
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken, tokenType);
            }
        }

        private void ThrowParsingException(ErrorCode errorCode, Token givenToken, TokenType? expectedTokenType = null)
        {
            var expected = expectedTokenType is null ? string.Empty : $" Expected token type {expectedTokenType}";
            var message = $"{ErrorCodes.StringRepresentatnion[errorCode]} -> {givenToken}{expected}";
            throw new ParserError(errorCode, givenToken, message);
        }
    }
}
