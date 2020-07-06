using System;
using System.Collections.Generic;
using Interpreter.AST;
using Interpreter.Memory;
using Interpreter.Tokens;

namespace Interpreter
{
    public class Interpreter
    {
        private readonly CallStack _callStack = new CallStack();

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
                    return VisitProgram(program);
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
                    return VisitCompound(compound);
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
                case ASTFunctionCall functionCall:
                    return VisitFunctionCall(functionCall);
                case ASTReturn returnStatement:
                    return VisitReturnStatement(returnStatement);
                    
            }

            throw new ArgumentException($"[{nameof(Interpreter)}] No visit method for node type {node.GetType()}");
        }

        private dynamic VisitProgram(ASTProgram program)
        {
            Logger.Debug("Enter Main");
            var activationRecord = new ActivationRecord("Main", ActivationRecordType.Program, 1);
            _callStack.Push(activationRecord);

            var runParameters = new List<ASTNode>();
            var mainFunction = new ASTFunctionCall("Main", runParameters, program.Token);
            mainFunction.SymbolFunction = program.MainFunction;

            var result = Visit(mainFunction);

            Logger.DebugMemory("Leave Main");
            Logger.DebugMemory(_callStack.ToString());

            _callStack.Pop();

            Logger.Debug($"Program exited with status code {result}");

            return result;
        }

        private void VisitAssign(ASTAssign node)
        {
            var value = Visit(node.Right);

            switch (node.Left)
            {
                case ASTVariable variable:
                    _callStack.Top[variable.Name] = value;
                    return;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    foreach (var variable in variablesDeclarations.Children)
                    {
                        _callStack.Top[variable.Variable.Name] = value;
                    }
                    return;
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }

        private object VisitVariable(ASTVariable node)
        {
            var variableName = node.Name;
            return _callStack.Top[variableName];
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
            _callStack.Top[variableName] = 0;
        }

        private void VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            return;
        }

        private dynamic VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var functionName = functionCall.FunctionName;
            var symbolFunction = functionCall.SymbolFunction;
            var formalParameters = symbolFunction.Parameters;
            var actualParameters = functionCall.ActualParameters;

            var activationRecord = new ActivationRecord(functionName, ActivationRecordType.Function, symbolFunction.ScopeLevel + 1);

            for (int i = 0; i < formalParameters.Count; ++i)
            {
                activationRecord[formalParameters[i].Name] = Visit(actualParameters[i]);
            }

            Logger.Debug($"Enter {functionName}");
            _callStack.Push(activationRecord);

            var result = Visit(symbolFunction.Body);

            Logger.DebugMemory($"Leave {functionName}");
            Logger.DebugMemory(_callStack.ToString());

            _callStack.Pop();

            return result;
        }

        private dynamic VisitReturnStatement(ASTReturn node)
        {
            return Visit(node.Expression);
        }

        private dynamic VisitCompound(ASTCompound node)
        {
            foreach (var child in node.Children)
            {
                if(child is ASTReturn)
                {
                    return Visit(child);
                }
                Visit(child);
            }

            return null;
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
