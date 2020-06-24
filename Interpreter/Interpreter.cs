using System;
using System.Linq;

namespace Interpreter
{
    public class Interpreter
    {
        private static readonly TokenType[] TermOperators = { TokenType.Plus, TokenType.Minus };
        private static readonly TokenType[] FactorOperators = { TokenType.Mul, TokenType.Div, TokenType.Mod };

        private readonly Lexer _lexer;
        private Token _currentToken = null;

        public Interpreter(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        public string Run()
        {
            var result = Expression();

            return $"{result}";
        }

        private double Expression()
        {
            var result = Term();

            while (TermOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Plus:
                        Eat(TokenType.Plus);
                        result += Term();
                        break;
                    case TokenType.Minus:
                        Eat(TokenType.Minus);
                        result -= Term();
                        break;
                }
            }

            return result;
        }

        private double Term()
        {
            var result = Factor();

            while (FactorOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Mul:
                        Eat(TokenType.Mul);
                        result *= Factor();
                        break;
                    case TokenType.Div:
                        Eat(TokenType.Div);
                        result /= Factor();
                        break;
                    case TokenType.Mod:
                        Eat(TokenType.Mod);
                        result %= Factor();
                        break;
                }
            }

            return result;
        }

        private double Factor()
        {
            var token = _currentToken;
            Eat(TokenType.Number);

            return token.Value.ToNumber();
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
