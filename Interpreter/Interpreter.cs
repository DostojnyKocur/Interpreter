using System;
using System.Collections.Generic;
using Interpreter.AST;

namespace Interpreter
{
    public class Interpreter
    {
        private readonly Dictionary<string, object> _globalScope = new Dictionary<string, object>();
        private readonly Parser _parser;

        public Interpreter(Parser parser)
        {
            _parser = parser;
        }

        public void DebugPrintGlobalScope()
        {
            Console.WriteLine("==== GLOBAL SCOPE ====");
            foreach(var entry in _globalScope)
            {
                Console.WriteLine($"{entry.Key}\t:{entry.Value}");
            }
            Console.WriteLine("==== ==== ====");
        }

        public void Run()
        {
            var root = _parser.Parse();

            Visit(root);
        }

        private dynamic Visit(ASTNode node)
        {
            switch (node)
            {
                case ASTEmpty empty:
                    VisitEmpty(empty);
                    return null;
                case ASTNumber number:
                    return VisitNumber(number);
                case ASTBinaryOperator binaryOperator:
                    return VisitBinaryOperator(binaryOperator);
                case ASTUnaryOperator unaryOperator:
                    return VisitUnaryOperator(unaryOperator);
                case ASTCompound compound:
                    VisitCompound(compound);
                    return null;
                case ASTAssign assign:
                    VisitAssign(assign);
                    return null;
                case ASTVariable variable:
                    return VisitVariable(variable);
            }

            throw new ArgumentException($"No visit method for node type {node.GetType()}");
        }

        private void VisitAssign(ASTAssign node)
        {
            var variableName = node.Left.Name;
            
            if(!_globalScope.ContainsKey(variableName))
            {
                var value = Visit(node.Right);
                _globalScope.Add(variableName, value);
            }
        }

        private object VisitVariable(ASTVariable node)
        {
            var variableName = node.Name;

            if (!_globalScope.ContainsKey(variableName))
            {
                throw new NullReferenceException($"Variable {variableName} not found");
            }

            return _globalScope[variableName];
        }

        private double VisitNumber(ASTNumber node)
        {
            return node.Value;
        }

        private void VisitEmpty(ASTEmpty node)
        {
            return;
        }

        private void VisitCompound(ASTCompound node)
        {
            foreach(var child in node.Children)
            {
                Visit(child);
            }
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
                case TokenType.Multiplication:
                    return Visit(node.Left) * Visit(node.Right);
                case TokenType.Divide:
                    return Visit(node.Left) / Visit(node.Right);
                case TokenType.Modulo:
                    return Visit(node.Left) % Visit(node.Right);
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }
    }
}
