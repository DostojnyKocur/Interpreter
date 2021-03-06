﻿using System;
using System.Collections.Generic;
using Interpreter.Common;
using Interpreter.Common.AST;
using Interpreter.Common.Extensions;
using Interpreter.Common.Symbols;
using Interpreter.Common.Tokens;
using Interpreter.InterpreterService.Memory;

namespace Interpreter.InterpreterService
{
    public class Interpreter : IInterpreter
    {
        private readonly CallStack _callStack = new CallStack();

        public void Interpret(ASTNode tree)
        {
            Visit(tree);
        }

        private VisitResult Visit(ASTNode node)
        {
            switch (node)
            {
                case ASTEmpty empty:
                    return VisitEmpty(empty);
                case ASTProgram program:
                    return VisitProgram(program);
                case ASTType type:
                    return VisitType(type);
                case ASTNumber number:
                    return VisitNumber(number);
                case ASTBool @bool:
                    return VisitBool(@bool);
                case ASTString @string:
                    return VisitString(@string);
                case ASTBinaryOperator binaryOperator:
                    return VisitBinaryOperator(binaryOperator);
                case ASTUnaryOperator unaryOperator:
                    return VisitUnaryOperator(unaryOperator);
                case ASTBlock block:
                    return VisitBlock(block);
                case ASTAssign assign:
                    return VisitAssign(assign);
                case ASTVariable variable:
                    return VisitVariable(variable);
                case ASTVariablesDeclarations variablesDeclarations:
                    return VisitVariablesDeclarations(variablesDeclarations);
                case ASTVariableDeclaration variableDeclaration:
                    return VisitVariableDeclaration(variableDeclaration);
                case ASTArrayInitialization arrayInitialization:
                    return VisitArrayInitialization(arrayInitialization);
                case ASTFunctionDefinition functionDefinition:
                    return VisitFunctionDefinition(functionDefinition);
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
                case ASTFor forStatement:
                    return VisitForStatement(forStatement);
            }

            throw new ArgumentException($"[{nameof(Interpreter)}] No visit method for node type {node.GetType()}");
        }

        private dynamic VisitProgram(ASTProgram program)
        {
            Logger.Debug("Enter Program");
            var activationRecord = new ActivationRecord("Main", ActivationRecordType.Program, 1);
            _callStack.Push(activationRecord);

            var runParameters = new List<ASTNode>();
            var mainFunction = new ASTFunctionCall(program.Token, "Main", runParameters);
            mainFunction.SymbolFunction = program.MainFunction;

            var result = Visit(mainFunction);

            Logger.DebugMemory("Leave Program");
            Logger.DebugMemory(_callStack.ToString());

            _callStack.Pop();

            Logger.Debug($"Program exited with status code {result?.Value ?? "null"}");

            return result;
        }

        private VisitResult VisitAssign(ASTAssign node)
        {
            var value = Visit(node.Right);

            switch (node.Left)
            {
                case ASTVariable variable:
                    if (variable.ArrayIndexFrom != null)
                    {
                        var index = Visit(variable.ArrayIndexFrom).Value;
                        ((List<dynamic>)_callStack.Top[variable.Name])[(int)index] = value.Value;
                    }
                    else
                    {
                        _callStack.Top[variable.Name] = value.Value;
                    }
                    return null;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    foreach (var variable in variablesDeclarations.Children)
                    {
                        _callStack.Top[variable.Variable.Name] = value.Value;
                    }
                    return null;
            }

            throw new ArgumentException($"Invalid AST node type {node.GetType()}");
        }

        private VisitResult VisitVariable(ASTVariable node)
        {
            if (node.ArrayIndexFrom != null)
            {
                var array = (List<dynamic>)_callStack.Top[node.Name];
                var indexFrom = (int)Visit(node.ArrayIndexFrom).Value;
                var integreIndexFrom = indexFrom < 0 ? array.Count + indexFrom : indexFrom;
                var integreIndexTo = array.Count - 1;
                var integerIndexStep = 1;

                if (node.ArrayIndexTo != null)
                {
                    var indexTo = (int)Visit(node.ArrayIndexTo).Value;
                    integreIndexTo = indexTo < 0 ? array.Count + indexTo : indexTo == int.MaxValue ? integreIndexTo : indexTo;

                    if (node.ArrayIndexStep != null)
                    {
                        integerIndexStep = (int)Visit(node.ArrayIndexStep).Value;
                    }

                    var result = new List<dynamic>();

                    var from = integreIndexTo > integreIndexFrom ? integreIndexFrom : integreIndexTo;
                    var to = integreIndexFrom > integreIndexTo ? integreIndexFrom : integreIndexTo;
                    var step = integerIndexStep < 0 ? -integerIndexStep : integerIndexStep;

                    for (var i = from; i < to; i += step)
                    {
                        result.Add(array[i]);
                    }

                    if ((integreIndexFrom <= integreIndexTo && integerIndexStep < 0) ||
                        (integreIndexFrom > integreIndexTo && integerIndexStep > 0))
                    {
                        result.Reverse();
                    }

                    return new VisitResult
                    {
                        ControlType = ControlType.Return,
                        Value = result
                    };
                }

                return new VisitResult
                {
                    ControlType = ControlType.Return,
                    Value = array[integreIndexFrom]
                };

            }

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

        private VisitResult VisitString(ASTString node)
        {
            return new VisitResult
            {
                ControlType = ControlType.Return,
                Value = node.Value
            };
        }

        private VisitResult VisitEmpty(ASTEmpty node)
        {
            return null;
        }

        private VisitResult VisitType(ASTType node)
        {
            return null;
        }

        private VisitResult VisitVariablesDeclarations(ASTVariablesDeclarations node)
        {
            foreach (var child in node.Children)
            {
                Visit(child);
            }

            return null;
        }

        private VisitResult VisitVariableDeclaration(ASTVariableDeclaration node)
        {
            var variableName = node.Variable.Name;

            switch (node.VariableType.TypeSpec)
            {
                case ASTArrayType arrayType:
                    _callStack.Top[variableName] = new List<dynamic>();
                    break;
                case ASTNonArrayType nonArrayType:
                    switch (nonArrayType.Type)
                    {
                        case TokenType.TypeNumber:
                            _callStack.Top[variableName] = 0;
                            break;
                        case TokenType.TypeBool:
                            _callStack.Top[variableName] = false;
                            break;
                        case TokenType.TypeString:
                            _callStack.Top[variableName] = string.Empty;
                            break;
                    }
                    break;
                default:
                    throw new ArgumentException($"Invalid variable type {variableName} : {node.VariableType.Name}");
            }

            return null;
        }

        private VisitResult VisitArrayInitialization(ASTArrayInitialization node)
        {
            var result = new List<dynamic>();

            foreach (var item in node.Children)
            {
                result.Add(Visit(item).Value);
            }

            return new VisitResult
            {
                ControlType = ControlType.Return,
                Value = result
            };
        }

        private VisitResult VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            return null;
        }

        private VisitResult VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var symbolFunction = functionCall.SymbolFunction;
            var functionName = functionCall.FunctionName;
            var formalParameters = symbolFunction.Parameters;
            var actualParameters = functionCall.ActualParameters;

            var activationRecord = new ActivationRecord(functionName, ActivationRecordType.Function, symbolFunction.ScopeLevel + 1);

            for (var i = 0; i < formalParameters.Count; ++i)
            {
                activationRecord[formalParameters[i].Name] = Visit(actualParameters[i]).Value;
            }

            Logger.Debug($"Enter {functionName}");
            _callStack.Push(activationRecord);

            var result = symbolFunction is SymbolBuiltinFunction ? BuiltinFunctionCall(functionCall) : Visit(symbolFunction.Body);

            var returnedValue = result != null ? result.Value : "null";

            if(returnedValue is List<dynamic> collection)
            {
                returnedValue = collection.ToPrint();
            }

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
            else
            {
                foreach (var elif in node.Elifs)
                {
                    var elifCondition = Visit(elif.Condition).Value;
                    if (elifCondition)
                    {
                        var result = Visit(elif.IfTrue);
                        if (result != null && result.ControlType != ControlType.None)
                        {
                            return result;
                        }
                    }
                }

                if (node.Else != null)
                {
                    var result = Visit(node.Else);
                    if (result != null && result.ControlType != ControlType.None)
                    {
                        return result;
                    }
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
                    switch (result.ControlType)
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

        private VisitResult VisitForStatement(ASTFor node)
        {
            foreach (var assign in node.Assignments)
            {
                Visit(assign);
            }

            var condition = node.Condition != null ? Visit(node.Condition).Value : true;
            while (condition)
            {
                var result = Visit(node.Body);
                if (result != null)
                {
                    switch (result.ControlType)
                    {
                        case ControlType.Return:
                            return result;
                        case ControlType.Break:
                            return null;
                    }
                }

                foreach (var continueStatement in node.ContinueStatements)
                {
                    Visit(continueStatement);
                }

                condition = node.Condition != null ? Visit(node.Condition).Value : true;
            }

            return null;
        }

        private VisitResult VisitBlock(ASTBlock node)
        {
            foreach (var child in node.Children)
            {
                switch (child)
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

        private VisitResult BuiltinFunctionCall(ASTFunctionCall functionCall)
        {
            var functionName = functionCall.FunctionName;

            switch (functionName)
            {
                case "print":
                    return PrintFunctionCall();
            }

            throw new ArgumentNullException($"Builtin function {functionName} not found");
        }

        private VisitResult PrintFunctionCall()
        {
            var value = _callStack.Top["str"];
            string valueToPrint;

            if (value is List<dynamic> collection)
            {
                valueToPrint = collection.ToPrint();
            }
            else
            {
                valueToPrint = $"{value}";
            }

            Console.WriteLine(valueToPrint);

            return null;
        }
    }
}
