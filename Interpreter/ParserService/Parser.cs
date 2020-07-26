using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Common.Errors;
using Interpreter.LexerService;
using Interpreter.LexerService.Tokens;
using Interpreter.ParserService.AST;

namespace Interpreter.ParserService
{
    public class Parser : IParser
    {
        private static readonly TokenType[] TermOperators = { TokenType.Plus, TokenType.Minus };
        private static readonly TokenType[] FactorOperators = { TokenType.Multiplication, TokenType.Divide, TokenType.Modulo };
        private static readonly TokenType[] CompareOperators = { TokenType.Equal, TokenType.NotEqual, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual };
        private static readonly TokenType[] BuilinVarTypes = { TokenType.TypeNumber, TokenType.TypeBool, TokenType.TypeString };

        private readonly ILexer _lexer;
        private Token _currentToken = null;

        public Parser(ILexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        public ASTNode Parse()
        {
            var node = Program();
            if (_currentToken.Type != TokenType.EOF)
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            }

            return node;
        }

        private ASTProgram Program()
        {
            return new ASTProgram(_currentToken, Statement());
        }

        private ASTCompound CompoundStatement()
        {
            var token = _currentToken;
            Eat(TokenType.ScopeBegin);
            var nodes = StatementList();
            Eat(TokenType.ScopeEnd);
            return new ASTCompound(token, nodes);
        }

        private IEnumerable<ASTNode> StatementList()
        {
            var results = new List<ASTNode>();

            results.Add(Statement());

            while (_currentToken.Type != TokenType.ScopeEnd)
            {
                results.Add(Statement());
            }

            if (_currentToken.Type == TokenType.Identifier)
            {
                ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            }

            return results;
        }

        private ASTNode Statement()
        {
            switch (_currentToken.Type)
            {
                case TokenType.ScopeBegin:
                    return CompoundStatement();
                case TokenType.Identifier:
                    return StatementAssignmentFunctionCall();
                case TokenType.TypeVoid:
                case TokenType.TypeNumber:
                case TokenType.TypeBool:
                case TokenType.TypeString:
                    return StatementDeclarationsDefinitionsAssignments();
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

        private ASTFor ForStatement()
        {
            var token = _currentToken;
            Eat(TokenType.For);
            Eat(TokenType.LeftParen);
            var assignments = new List<ASTAssign>();
            if (_currentToken.Type != TokenType.Semicolon)
            {
                assignments = ForInitializationAssignments();
            }
            Eat(TokenType.Semicolon);

            ASTNode condition = null;
            if (_currentToken.Type != TokenType.Semicolon)
            {
                condition = Condition();
            }
            Eat(TokenType.Semicolon);

            var continueStatements = new List<ASTNode>();
            if (_currentToken.Type != TokenType.RightParen)
            {
                continueStatements = ForContinueStatement();
            }
            Eat(TokenType.RightParen);

            var statement = Statement();

            return new ASTFor(token, assignments, condition, continueStatements, statement);
        }

        private ASTIfElse IfElseStatement()
        {
            var token = _currentToken;
            Eat(TokenType.If);
            Eat(TokenType.LeftParen);
            var condition = Condition();
            Eat(TokenType.RightParen);
            var ifTrue = Statement();
            if (_currentToken.Type == TokenType.Else)
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

        private ASTReturn ReturnStatement()
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
                var result = new ASTReturn(returnToken, Condition());
                Eat(TokenType.Semicolon);
                return result;
            }
        }

        private ASTBreak BreakStatement()
        {
            var token = _currentToken;
            Eat(TokenType.Break);
            Eat(TokenType.Semicolon);
            return new ASTBreak(token);
        }

        private ASTContinue ContinueStatement()
        {
            var token = _currentToken;
            Eat(TokenType.Continue);
            Eat(TokenType.Semicolon);
            return new ASTContinue(token);
        }

        private ASTNode StatementAssignmentFunctionCall()
        {
            var idToken = _currentToken;
            Eat(TokenType.Identifier);

            if (_currentToken.Type == TokenType.LeftParen)
            {
                var functionCall = FunctionCall(idToken);
                Eat(TokenType.Semicolon);
                return functionCall;
            }
            else
            {
                var variable = Variable(idToken);
                var assignmentStatement = AssignmentStatement(variable);
                Eat(TokenType.Semicolon);
                return assignmentStatement;
            }
        }

        private ASTNode StatementDeclarationsDefinitionsAssignments()
        {
            var type = Type();
            var idToken = _currentToken;
            Eat(TokenType.Identifier);

            if (_currentToken.Type == TokenType.Comma ||
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

            if (_currentToken.Type == TokenType.LeftParen)
            {
                return FunctionDefinition(type, idToken);
            }

            ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            return null;
        }

        private List<ASTAssign> ForInitializationAssignments()
        {
            var assignments = new List<ASTAssign>();
            while (_currentToken.Type != TokenType.Semicolon)
            {
                ASTAssign assignment;
                if (BuilinVarTypes.Contains(_currentToken.Type))
                {
                    var type = Type();
                    var firstVariable = Variable();
                    var variablesDeclarations = VariablesDeclarations(type, firstVariable);
                    assignment = AssignmentStatement(variablesDeclarations);

                }
                else
                {
                    var firstVariable = Variable();
                    assignment = AssignmentStatement(firstVariable);

                }
                assignments.Add(assignment);
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }

            return assignments;
        }

        private List<ASTNode> ForContinueStatement()
        {
            var continueStatements = new List<ASTNode>();

            while (_currentToken.Type != TokenType.RightParen)
            {
                var firstVariable = Variable();
                var assignment = AssignmentStatement(firstVariable);
                continueStatements.Add(assignment);
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }

            return continueStatements;
        }

        private ASTAssign AssignmentStatement(ASTNode leftNode)
        {
            var token = _currentToken;
            Eat(TokenType.Assign);
            var right = _currentToken.Type == TokenType.LeftBracket ? ArrayInitialization() : Expression();
            return new ASTAssign(leftNode, token, right);
        }

        private ASTArrayInitialization ArrayInitialization()
        {
            var token = _currentToken;
            Eat(TokenType.LeftBracket);

            var items = new List<ASTNode>();

            while (_currentToken.Type != TokenType.RigthBracket)
            {
                items.Add(Expression());
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }

            Eat(TokenType.RigthBracket);

            return new ASTArrayInitialization(token, items);
        }

        private ASTIndexExpression IndexExpression(ASTVariable variable)
        {
            Eat(TokenType.LeftBracket);
            var node = new ASTIndexExpression(variable.Token, variable, Expression());
            Eat(TokenType.RigthBracket);
            return node;
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

            while (_currentToken.Type == TokenType.And)
            {
                var token = _currentToken;

                Eat(TokenType.And);

                node = new ASTBinaryOperator(node, token, NotCondition());
            }

            return node;
        }

        private ASTNode NotCondition()
        {

            if (_currentToken.Type == TokenType.Not)
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
            if (_currentToken.Type == TokenType.ConstBool)
            {
                var token = _currentToken;
                Eat(TokenType.ConstBool);

                return new ASTBool(token);
            }

            var node = Expression();

            while (CompareOperators.Contains(_currentToken.Type))
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
                case TokenType.ConstBool:
                    Eat(TokenType.ConstBool);
                    return new ASTBool(token);
                case TokenType.ConstString:
                    Eat(TokenType.ConstString);
                    return new ASTString(token);
                case TokenType.LeftParen:
                    Eat(TokenType.LeftParen);
                    var node = Expression();
                    Eat(TokenType.RightParen);
                    return node;
                case TokenType.Identifier:
                    switch (_lexer.CurrentChar)
                    {
                        case '(':
                            Eat(TokenType.Identifier);
                            return FunctionCall(token);
                        case '[':
                            Eat(TokenType.Identifier);
                            return IndexExpression(new ASTVariable(token));
                    }
                    Eat(TokenType.Identifier);
                    return new ASTVariable(token);
            }

            ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
            return null;
        }

        private ASTFunctionDefinition FunctionDefinition(ASTType returnType, Token name)
        {
            var argumentList = ParameterList();
            var body = CompoundStatement();
            return new ASTFunctionDefinition(name, returnType, argumentList, body);
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

            if (_currentToken.Type != TokenType.RightParen)
            {
                var node = Expression();
                actualParameters.Add(node);

                while (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    var anotherParam = Expression();
                    actualParameters.Add(anotherParam);
                }
            }

            Eat(TokenType.RightParen);

            return new ASTFunctionCall(idToken, functionName, actualParameters);
        }

        private ASTVariablesDeclarations VariablesDeclarations(ASTType type = null, ASTVariable firstVariable = null)
        {
            var variableType = type ?? Type();

            if (variableType.Token.Type == TokenType.TypeVoid)
            {
                ThrowParsingException(ErrorCode.IncorrectType, variableType.Token);
            }

            var variables = new List<ASTVariable>();

            variables.Add(firstVariable ?? Variable());

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

        private ASTVariable Variable(Token idToken = null)
        {
            var token = idToken;
            if (token is null)
            {
                token = _currentToken;
                Eat(TokenType.Identifier);
            }

            ASTNode index = null;
            if(_currentToken.Type == TokenType.LeftBracket)
            {
                Eat(TokenType.LeftBracket);
                index = Expression();
                Eat(TokenType.RigthBracket);
            }

            var node = new ASTVariable(token, index);
            
            return node;
        }

        private ASTType Type()
        {
            ASTNode type;
            var nonArrayType = NonArrayType();
            type = nonArrayType;

            if (_currentToken.Type == TokenType.LeftBracket)
            {
                type = ArrayType(nonArrayType);
            }

            return new ASTType(type.Token, type);
        }

        private ASTNonArrayType NonArrayType()
        {
            var token = _currentToken;
            Eat(token.Type);
            return new ASTNonArrayType(token);
        }

        private ASTArrayType ArrayType(ASTNonArrayType type)
        {
            var rank = new ASTRankSpec(_currentToken);
            Eat(TokenType.LeftBracket);
            Eat(TokenType.RigthBracket);
            return new ASTArrayType(type.Token, type, rank);
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
