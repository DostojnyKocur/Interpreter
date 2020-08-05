﻿using System;
using System.Linq;
using Interpreter.Common;
using Interpreter.Common.AST;
using Interpreter.Common.Errors;
using Interpreter.Common.Symbols;
using Interpreter.Common.Tokens;

namespace Interpreter.AnalyzerService
{
    public partial class SemanticAnalyzer : ISemanticAnalyzer
    {
        private static readonly TokenType[] BoolOperators = { TokenType.Equal, TokenType.NotEqual, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual, TokenType.Not, TokenType.And, TokenType.Or };
        private const string ArrayTypeSuffix = "_array";

        private readonly ScopedSymbolTable _globalScope = new ScopedSymbolTable("global", 1);
        private ScopedSymbolTable _currentScope;

        public void DebugPrintSymbolTable()
        {
            _currentScope.DebugPrintSymbols();
        }

        public void Analyze(ASTNode node)
        {
            Visit(node);
        }

        private Symbol Visit(ASTNode node)
        {
            switch (node)
            {
                case ASTEmpty empty:
                    return VisitEmpty(empty);
                case ASTProgram program:
                    return VisitProgram(program);
                case ASTCompound compound:
                    return VisitCompound(compound);
                case ASTBinaryOperator binaryOperator:
                    return VisitBinaryOperator(binaryOperator);
                case ASTUnaryOperator unaryOperator:
                    return VisitUnaryOperator(unaryOperator);
                case ASTVariablesDeclarations variablesDeclarations:
                    return VisitVariablesDeclarations(variablesDeclarations);
                case ASTVariableDeclaration variableDeclaration:
                    return VisitVariableDeclaration(variableDeclaration);
                case ASTArrayInitialization arrayInitialization:
                    return VisitArrayInitialization(arrayInitialization);
                case ASTNumber number:
                    return VisitNumber(number);
                case ASTBool @bool:
                    return VisitBool(@bool);
                case ASTString @string:
                    return VisitString(@string);
                case ASTAssign assign:
                    return VisitAssign(assign);
                case ASTVariable variable:
                    return VisitVariable(variable);
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
                case ASTElif elifStatement:
                    return VisitElifStatement(elifStatement);
                case ASTWhile whileStatement:
                    return VisitWhileStatement(whileStatement);
                case ASTFor forStatement:
                    return VisitForStatement(forStatement);
                default:
                    throw new ArgumentException($"[{nameof(SemanticAnalyzer)}] No visit method for node type {node.GetType()}");
            }
        }

        private Symbol VisitNumber(ASTNumber node)
        {
            return _currentScope.Lookup("number");
        }

        private Symbol VisitBool(ASTBool node)
        {
            return _currentScope.Lookup("bool");
        }

        private Symbol VisitString(ASTString node)
        {
            return _currentScope.Lookup("string");
        }

        private Symbol VisitEmpty(ASTEmpty node)
        {
            return null;
        }

        private Symbol VisitProgram(ASTProgram node)
        {
            Logger.DebugScope("Enter scope : global");
            _currentScope = _globalScope;

            Visit(node.Root);

            var mainFunction = _currentScope.Lookup("Main", true);
            if (mainFunction == null)
            {
                ThrowSemanticException(ErrorCode.MissingMain, node.Token);
            }

            node.MainFunction = mainFunction as SymbolFunction;

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;

            Logger.DebugScope("Leave scope : global");

            return null;
        }

        private Symbol VisitCompound(ASTCompound node)
        {
            Symbol returnType = null;

            foreach (var child in node.Children)
            {
                if (returnType is null)
                {
                    returnType = Visit(child);
                }
                else
                {
                    var currentType = Visit(child);
                    if (currentType != returnType)
                    {
                        ThrowIncompatibleTypesException(child.Token, returnType.Name, currentType.Name);
                    }
                }
            }

            return returnType;
        }

        private Symbol VisitBinaryOperator(ASTBinaryOperator node)
        {
            var leftType = Visit(node.Left);
            var rightType = Visit(node.Right);

            if (leftType != rightType)
            {
                ThrowIncompatibleTypesException(node.Token, leftType.Name, rightType.Name);
            }

            return BoolOperators.Contains(node.Type) ? _currentScope.Lookup("bool") : leftType;
        }

        private Symbol VisitUnaryOperator(ASTUnaryOperator node)
        {
            var type = Visit(node.Expression);

            return BoolOperators.Contains(node.Type) ? _currentScope.Lookup("bool") : type;
        }

        private Symbol VisitVariablesDeclarations(ASTVariablesDeclarations node)
        {
            foreach (var child in node.Children)
            {
                Visit(child);
            }

            return null;
        }

        private Symbol VisitVariableDeclaration(ASTVariableDeclaration node)
        {
            var typeSymbol = PrepareTypeSymbol(node.VariableType);

            var variableName = node.Variable.Name;
            var variableSymbol = new SymbolVariable(variableName, typeSymbol);

            if (_currentScope.Lookup(variableName, true) != null)
            {
                ThrowSemanticException(ErrorCode.DuplicateIdentifier, node.Variable.Token);
            }

            _currentScope.Define(variableSymbol);

            return null;
        }

        private Symbol VisitArrayInitialization(ASTArrayInitialization node)
        {
            Symbol itemType = null;

            foreach (var item in node.Children)
            {
                if (itemType is null)
                {
                    itemType = Visit(item);
                }
                else
                {
                    var currentItemType = Visit(item);
                    if (currentItemType.Name != itemType.Name)
                    {
                        ThrowIncompatibleTypesException(node.Token, itemType.Name, currentItemType.Name);
                    }
                }
            }

            return _currentScope.Lookup($"{itemType.Name}{ArrayTypeSuffix}");
        }

        private Symbol VisitAssign(ASTAssign node)
        {
            Symbol leftType;

            switch (node.Left)
            {
                case ASTVariable variable:
                    leftType = Visit(variable);
                    break;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    var firstVariable = variablesDeclarations.Children.First();
                    leftType = Visit(firstVariable.Variable);
                    break;
                default:
                    throw new ArgumentException($"Invalid AST node type {node.GetType()}");
            }

            var rightType = Visit(node.Right);

            if (leftType != rightType)
            {
                ThrowIncompatibleTypesException(node.Token, leftType.Name, rightType.Name);
            }

            return null;
        }

        private Symbol VisitVariable(ASTVariable node)
        {
            var variableName = node.Name;
            var variableSymbol = _currentScope.Lookup(variableName);
            if (variableSymbol is null)
            {
                ThrowSemanticException(ErrorCode.IdentifierNotFound, node.Token);
            }

            if (node.ArrayIndexFrom != null)
            {
                var indexFromType = Visit(node.ArrayIndexFrom);
                if (indexFromType.Name != "number")
                {
                    ThrowIncompatibleTypesException(node.ArrayIndexFrom.Token, indexFromType.Name, "number");
                }

                if(node.ArrayIndexTo != null)
                {
                    var indexToType = Visit(node.ArrayIndexTo);
                    if (indexToType.Name != "number")
                    {
                        ThrowIncompatibleTypesException(node.ArrayIndexTo.Token, indexToType.Name, "number");
                    }

                    if (node.ArrayIndexStep != null)
                    {
                        var indexStepType = Visit(node.ArrayIndexStep);
                        if (indexStepType.Name != "number")
                        {
                            ThrowIncompatibleTypesException(node.ArrayIndexStep.Token, indexStepType.Name, "number");
                        }
                    }

                    return variableSymbol.Type;
                }

                return variableSymbol.Type.Type;
            }

            return variableSymbol.Type;
        }

        private Symbol VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            var typeSymbol = PrepareTypeSymbol(node.ReturnType);

            var functionName = node.Token.Value;
            var functionSymbol = new SymbolFunction(functionName, typeSymbol);

            if (_currentScope.Lookup(functionName, true) != null)
            {
                ThrowSemanticException(ErrorCode.DuplicateIdentifier, node.Token);
            }

            _currentScope.Define(functionSymbol);

            Logger.DebugScope($"Enter scope : {functionName}");
            var functionScope = new ScopedSymbolTable(functionName, _currentScope.Level + 1, _currentScope);

            _currentScope = functionScope;

            foreach (var param in node.Parameters)
            {
                var paramName = param.Variable.Name;
                var paramType = _currentScope.Lookup(param.VariableType.Name);
                var paramSymbol = new SymbolVariable(paramName, paramType);

                if (_currentScope.Lookup(paramName, true) != null)
                {
                    ThrowSemanticException(ErrorCode.DuplicateIdentifier, param.Variable.Token);
                }
                _currentScope.Define(paramSymbol);

                functionSymbol.Parameters.Add(paramSymbol);
            }

            var returnedType = Visit(node.Body);
            if (returnedType != null && returnedType != typeSymbol)
            {
                ThrowIncompatibleTypesException(node.Token, typeSymbol.Name, returnedType.Name);
            }

            if (returnedType != null && returnedType.Name != "void" && !HasReturnStatement(node.Body))
            {
                ThrowSemanticException(ErrorCode.MissingReturnStatement, node.Token);
            }

            functionSymbol.Body = node.Body;

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : {functionName}");

            return null;
        }

        private Symbol VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var functionSymbol = _currentScope.Lookup(functionCall.FunctionName);

            if (functionSymbol is null)
            {
                ThrowSemanticException(ErrorCode.IdentifierNotFound, functionCall.Token);
            }

            var formalParameters = (functionSymbol as SymbolFunction).Parameters;
            var actualParameters = functionCall.ActualParameters;

            if (formalParameters.Count != actualParameters.Count && functionSymbol.Name != "print")
            {
                ThrowSemanticException(ErrorCode.WrongParamNumber, functionCall.Token);
            }

            for (var i = 0; i < formalParameters.Count; ++i)
            {
                var param = functionCall.ActualParameters[i];
                var actualParamType = Visit(param);
                var formalParamType = formalParameters[i].Type;
                if (actualParamType != formalParamType)
                {
                    ThrowIncompatibleTypesException(param.Token, formalParamType.Name, actualParamType.Name);
                }
            }

            functionCall.SymbolFunction = functionSymbol as SymbolFunction;

            return (functionSymbol as SymbolFunction).ReturnType;
        }

        private Symbol PrepareTypeSymbol(ASTType node)
        {
            var typeName = node.Name;
            var typeSymbol = _currentScope.Lookup(typeName);

            if (node.TypeSpec is ASTArrayType)
            {
                var arrayTypeName = $"{typeName}{ArrayTypeSuffix}";

                var arrayTypeSymbol = _currentScope.Lookup(arrayTypeName);
                if (arrayTypeSymbol is null)
                {
                    typeSymbol = new SymbolArrayType(arrayTypeName, typeSymbol);
                    _currentScope.Define(typeSymbol);
                }
                else
                {
                    typeSymbol = arrayTypeSymbol;
                }
            }

            return typeSymbol;
        }

        private void ThrowSemanticException(ErrorCode errorCode, Token token)
        {
            var message = $"{ErrorCodes.StringRepresentatnion[errorCode]} -> {token}";
            throw new SemanticError(errorCode, token, message);
        }

        private void ThrowIncompatibleTypesException(Token token, string leftType, string rightType)
        {
            const ErrorCode errorCode = ErrorCode.IncompatibleTypes;
            var message = $"{ErrorCodes.StringRepresentatnion[errorCode]} {leftType} and {rightType} -> {token}";
            throw new SemanticError(errorCode, token, message);
        }

        private bool HasReturnStatement(ASTNode node)
        {
            if (node is null)
            {
                return false;
            }

            var result = false;
            switch (node)
            {
                case ASTReturn @return:
                    return true;
                case ASTCompound compound:
                    foreach (var children in compound.Children)
                    {
                        result |= HasReturnStatement(children);
                    }
                    return result;
                case ASTIfElse ifElse:
                    return HasReturnStatement(ifElse.IfTrue) || HasReturnStatement(ifElse.Else);
            }

            return false;
        }
    }
}
