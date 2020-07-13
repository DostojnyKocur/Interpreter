﻿using System;
using System.Collections.Generic;
using Interpreter.AST;
using Interpreter.Memory;
using Interpreter.Tokens;

namespace Interpreter
{
    public class Interpreter
    {
        private enum ControlType
        {
            None,
            Break,
            Continue,
            Return
        }
        private class VisitResult
        {
            public ControlType ControlType { get; set; } = ControlType.None;
            public dynamic Value { get; set; } = null;
        }

        private readonly CallStack _callStack = new CallStack();

        public void Run(ASTNode tree)
        {
            Visit(tree);
        }

        private VisitResult Visit(ASTNode node)
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
                case ASTBool @bool:
                    return VisitBool(@bool);
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
                case ASTBreak breakStatement:
                    return VisitBreakStatement(breakStatement);
                case ASTContinue continueStatement:
                    return VisitContinueStatement(continueStatement);
                case ASTIfElse ifElseStatement:
                    return VisitIfElseStatement(ifElseStatement);
                case ASTWhile whileStatement:
                    return VisitWhileStatement(whileStatement);
            }

            throw new ArgumentException($"[{nameof(Interpreter)}] No visit method for node type {node.GetType()}");
        }

        private dynamic VisitProgram(ASTProgram program)
        {
            Logger.Debug("Enter Program");
            var activationRecord = new ActivationRecord("Main", ActivationRecordType.Program, 1);
            _callStack.Push(activationRecord);

            var runParameters = new List<ASTNode>();
            var mainFunction = new ASTFunctionCall("Main", runParameters, program.Token);
            mainFunction.SymbolFunction = program.MainFunction;

            var result = Visit(mainFunction);

            Logger.DebugMemory("Leave Program");
            Logger.DebugMemory(_callStack.ToString());

            _callStack.Pop();

            Logger.Debug($"Program exited with status code {result.Value}");

            return result;
        }

        private void VisitAssign(ASTAssign node)
        {
            var value = Visit(node.Right);

            switch (node.Left)
            {
                case ASTVariable variable:
                    _callStack.Top[variable.Name] = value.Value;
                    return;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    foreach (var variable in variablesDeclarations.Children)
                    {
                        _callStack.Top[variable.Variable.Name] = value.Value;
                    }
                    return;
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }

        private VisitResult VisitVariable(ASTVariable node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Return,
                Value = _callStack.Top[node.Name]
            };
        }

        private VisitResult VisitNumber(ASTNumber node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Return,
                Value = node.Value
            };
        }

        private VisitResult VisitBool(ASTBool node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Return,
                Value = node.Value
            };
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

            switch(node.Type.Token.Type)
            {
                case TokenType.TypeNumber:
                    _callStack.Top[variableName] = 0;
                    break;
                case TokenType.TypeBool:
                    _callStack.Top[variableName] = false;
                    break;
                default:
                    throw new ArgumentException($"Invalid variable type {variableName} : {node.Type.Name}");
            }

            
        }

        private void VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            return;
        }

        private VisitResult VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var functionName = functionCall.FunctionName;
            var symbolFunction = functionCall.SymbolFunction;
            var formalParameters = symbolFunction.Parameters;
            var actualParameters = functionCall.ActualParameters;

            var activationRecord = new ActivationRecord(functionName, ActivationRecordType.Function, symbolFunction.ScopeLevel + 1);

            for (int i = 0; i < formalParameters.Count; ++i)
            {
                activationRecord[formalParameters[i].Name] = Visit(actualParameters[i]).Value;
            }

            Logger.Debug($"Enter {functionName}");
            _callStack.Push(activationRecord);

            var result = Visit(symbolFunction.Body);

            var returnedValue = result != null ? result.Value : "null";
            Logger.DebugMemory($"Leave {functionName}, returned value ({returnedValue})");
            Logger.DebugMemory(_callStack.ToString());

            _callStack.Pop();

            return result;
        }

        private VisitResult VisitReturnStatement(ASTReturn node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Return,
                Value = node.Expression != null ? Visit(node.Expression).Value : null
            };
        }

        private VisitResult VisitBreakStatement(ASTBreak node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Break
            };
        }

        private VisitResult VisitContinueStatement(ASTContinue node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Continue
            };
        }

        private VisitResult VisitIfElseStatement(ASTIfElse node)
        {
            var condition = Visit(node.Condition).Value;
            if (condition)
            {
                var result = Visit(node.IfTrue);
                if (result != null && result.ControlType != ControlType.None)
                {
                    return result;
                }
            }
            else if (node.Else != null)
            {
                var result = Visit(node.Else);
                if (result != null && result.ControlType != ControlType.None)
                {
                    return result;
                }
            }

            return null;
        }

        private VisitResult VisitWhileStatement(ASTWhile node)
        {
            var condition = Visit(node.Condition).Value;
            while (condition)
            {
                var result = Visit(node.Body);
                if (result != null)
                {
                    switch(result.ControlType)
                    {
                        case ControlType.Return:
                            return result;
                        case ControlType.Break:
                            return null;
                    }
                }

                condition = Visit(node.Condition).Value;
            }

            return null;
        }

        private VisitResult VisitCompound(ASTCompound node)
        {
            foreach (var child in node.Children)
            {
                switch(child)
                {
                    case ASTReturn _:
                        return new VisitResult
                        {
                            ControlType = ControlType.Return,
                            Value = Visit(child).Value
                        };
                    case ASTBreak _:
                        return new VisitResult
                        {
                            ControlType = ControlType.Break
                        };
                    case ASTContinue _:
                        return new VisitResult
                        {
                            ControlType = ControlType.Continue
                        };
                }

                var result = Visit(child);
                if (result != null && result.ControlType != ControlType.None)
                {
                    return result;
                }
            }

            return null;
        }

        private VisitResult VisitUnaryOperator(ASTUnaryOperator node)
        {
            switch (node.Type)
            {
                case TokenType.Plus:
                    return new VisitResult
                    {
                        Value = +Visit(node.Expression).Value
                    };
                case TokenType.Minus:
                    return new VisitResult
                    {
                        Value = -Visit(node.Expression).Value
                    };
                case TokenType.Not:
                    return new VisitResult
                    {
                        Value = !Visit(node.Expression).Value
                    };
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }

        private VisitResult VisitBinaryOperator(ASTBinaryOperator node)
        {
            switch (node.Type)
            {
                case TokenType.Plus:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value + Visit(node.Right).Value
                    };
                case TokenType.Minus:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value - Visit(node.Right).Value
                    };
                case TokenType.Multiplication:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value * Visit(node.Right).Value
                    };
                case TokenType.Divide:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value / Visit(node.Right).Value
                    };
                case TokenType.Modulo:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value % Visit(node.Right).Value
                    };
                case TokenType.Equal:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value == Visit(node.Right).Value
                    };
                case TokenType.NotEqual:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value != Visit(node.Right).Value
                    };
                case TokenType.Greater:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value > Visit(node.Right).Value
                    };
                case TokenType.GreaterEqual:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value >= Visit(node.Right).Value
                    };
                case TokenType.Less:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value < Visit(node.Right).Value
                    };
                case TokenType.LessEqual:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value <= Visit(node.Right).Value
                    };
                case TokenType.And:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value && Visit(node.Right).Value
                    };
                case TokenType.Or:
                    return new VisitResult
                    {
                        Value = Visit(node.Left).Value || Visit(node.Right).Value
                    };
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }
    }
}
