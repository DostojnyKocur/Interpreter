using System;
using System.Linq;
using Interpreter.AST;

namespace Interpreter
{
    public class Parser
    {
        private static readonly TokenType[] TermOperators = { TokenType.Plus, TokenType.Minus };
        private static readonly TokenType[] FactorOperators = { TokenType.Mul, TokenType.Div, TokenType.Mod };

        private readonly Lexer _lexer;
        private Token _currentToken = null;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        public ASTNode Parse()
        {
            return Expression();
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
                    case TokenType.Mul:
                        Eat(TokenType.Mul);
                        break;
                    case TokenType.Div:
                        Eat(TokenType.Div);
                        break;
                    case TokenType.Mod:
                        Eat(TokenType.Mod);
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
                case TokenType.Number:
                    Eat(TokenType.Number);
                    return new ASTNumber(token);
                case TokenType.LParen:
                    Eat(TokenType.LParen);
                    var node = Expression();
                    Eat(TokenType.RParen);
                    return node;
            }

            throw new InvalidOperationException("Error parsing input");
        }

        private void Eat(TokenType tokenType)
        {
            if (_currentToken.Type == tokenType)
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                throw new InvalidOperationException("Error parsing input");
            }
        }
    }
}
