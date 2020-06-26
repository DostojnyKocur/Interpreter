using System;
using System.Collections.Generic;
using Interpreter.AST;

namespace Interpreter
{
    public class Interpreter
    {
        private readonly Dictionary<string, object> _globalScope = new Dictionary<string, object>();

        public void DebugPrintGlobalScope()
        {
            Console.WriteLine("==== GLOBAL SCOPE ====");
            foreach (var entry in _globalScope)
            {
                Console.WriteLine($"{entry.Key}\t:{entry.Value}");
            }
            Console.WriteLine("==== ==== ====");
        }

        public void Run(ASTNode tree)
        {
            Visit(tree);
        }

        private dynamic Visit(ASTNode node)
        {
            switch (node)
            {
                case ASTEmpty empty:
                    VisitEmpty(empty);
                    return null;
                case ASTProgram program:
                    Visit(program.Root);
                    return null;
                case ASTType type:
                    VisitType(type);
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
                case ASTVariablesDeclarations variablesDeclarations:
                    VisitVariablesDeclarations(variablesDeclarations);
                    return null;
                case ASTVariableDeclaration variableDeclaration:
                    VisitVariableDeclaration(variableDeclaration);
                    return null;
            }

            throw new ArgumentException($"[Interpreter] No visit method for node type {node.GetType()}");
        }

        private void VisitAssign(ASTAssign node)
        {
            var value = Visit(node.Right);

            switch (node.Left)
            {
                case ASTVariable variable:
                    _globalScope[variable.Name] = value;
                    return;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    foreach (var variable in variablesDeclarations.Children)
                    {
                        _globalScope[variable.Variable.Name] = value;
                    }
                    return;
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
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

        private void VisitType(ASTType node)
        {
            return;
        }

        private void VisitVariablesDeclarations(ASTVariablesDeclarations node)
        {
            foreach (var child in node.Children)
            {
                Visit(child);
            }
        }

        private void VisitVariableDeclaration(ASTVariableDeclaration node)
        {
            var variableName = node.Variable.Name;

            if (!_globalScope.ContainsKey(variableName))
            {
                _globalScope.Add(variableName, 0);
            }
        }

        private void VisitCompound(ASTCompound node)
        {
            foreach (var child in node.Children)
            {
                Visit(child);
            }
        }

        private double VisitUnaryOperator(ASTUnaryOperator node)
        {
            switch (node.Type)
            {
                case TokenType.Plus:
                    return +Visit(node.Expression);
                case TokenType.Minus:
                    return -Visit(node.Expression);
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
