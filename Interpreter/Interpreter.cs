using System;
using System.Collections.Generic;
using Interpreter.AST;
using Interpreter.Tokens;

namespace Interpreter
{
    public class Interpreter
    {
        private readonly Dictionary<string, object> _globalMemory = new Dictionary<string, object>();

        public void DebugPrintGlobalScope()
        {
            Console.WriteLine("\r\n==== GLOBAL Memory ====");
            foreach (var entry in _globalMemory)
            {
                Console.WriteLine("{0, 20}\t:{1, 25}", entry.Key.Trim(), entry.Value);
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
                case ASTFunctionDefinition functionDefinition:
                    VisitFunctionDefinition(functionDefinition);
                    return null;
            }

            throw new ArgumentException($"[{nameof(Interpreter)}] No visit method for node type {node.GetType()}");
        }

        private void VisitAssign(ASTAssign node)
        {
            var value = Visit(node.Right);

            switch (node.Left)
            {
                case ASTVariable variable:
                    _globalMemory[variable.Name] = value;
                    return;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    foreach (var variable in variablesDeclarations.Children)
                    {
                        _globalMemory[variable.Variable.Name] = value;
                    }
                    return;
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }

        private object VisitVariable(ASTVariable node)
        {
            var variableName = node.Name;

            if (!_globalMemory.ContainsKey(variableName))
            {
                throw new NullReferenceException($"Variable {variableName} not found");
            }

            return _globalMemory[variableName];
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

            if (!_globalMemory.ContainsKey(variableName))
            {
                _globalMemory.Add(variableName, 0);
            }
        }

        private void VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            return;
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
