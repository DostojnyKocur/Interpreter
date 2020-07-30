using System;
using System.Linq;
using Interpreter.Common.Errors;
using Interpreter.LexerService.Tokens;
using Interpreter.ParserService.AST;

namespace Interpreter.ParserService
{
    public partial class Parser
    {
        private ASTNode ArithmeticExpression()
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
                    var node = ArithmeticExpression();
                    Eat(TokenType.RightParen);
                    return node;
                case TokenType.Identifier:
                    return _secondToken.Type == TokenType.LeftParen ? (ASTNode)FunctionCall() : Variable();
                default:
                    ThrowParsingException(ErrorCode.UnexpectedToken, _currentToken);
                    return null;
            }
        }

        private ASTNode Expression()
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
                var condition = Expression();
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

            var node = ArithmeticExpression();

            while (CompareOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;
                Eat(token.Type);

                node = new ASTBinaryOperator(node, token, ArithmeticExpression());
            }

            return node;
        }
    }
}
