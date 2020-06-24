using System;
using Interpreter.AST;

namespace Interpreter
{
    public class Interpreter
    {
        private readonly Parser _parser;

        public Interpreter(Parser parser)
        {
            _parser = parser;
        }

        public string Run()
        {
            var root = _parser.Parse();

            var result = Visit(root);

            return $"{result}";
        }

        private double Visit(ASTNode node)
        {
            switch (node)
            {
                case ASTNumber number:
                    return VisitNumber(number);
                case ASTBinaryOperator binaryOperator:
                    return VisitBinaryOperator(binaryOperator);
                case ASTUnaryOperator unaryOperator:
                    return VisitUnaryOperator(unaryOperator);
            }

            throw new ArgumentException($"No visit method for node type {node.GetType()}");
        }

        private double VisitNumber(ASTNumber node)
        {
            return node.Value;
        }

        private double VisitUnaryOperator(ASTUnaryOperator node)
        {
            switch (node.Type)
            {
                case TokenType.Plus:
                    return +Visit(node.Node);
                case TokenType.Minus:
                    return -Visit(node.Node);
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }

        private double VisitBinaryOperator(ASTBinaryOperator node)
        {
            switch (node.Type)
            {
                case TokenType.Plus:
                    return Visit(node.Left) + Visit(node.Right);
                case TokenType.Minus:
                    return Visit(node.Left) - Visit(node.Right);
                case TokenType.Mul:
                    return Visit(node.Left) * Visit(node.Right);
                case TokenType.Div:
                    return Visit(node.Left) / Visit(node.Right);
                case TokenType.Mod:
                    return Visit(node.Left) % Visit(node.Right);
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }
    }
}
