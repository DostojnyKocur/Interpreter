using System;
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
                case ASTBlock block:
                    return VisitBlock(block);
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
            return _currentScope.LookupSingle("number");
        }

        private Symbol VisitBool(ASTBool node)
        {
            return _currentScope.LookupSingle("bool");
        }

        private Symbol VisitString(ASTString node)
        {
            return _currentScope.LookupSingle("string");
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

            var mainFunction = _currentScope.LookupSingle("Main", true);
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

        private Symbol VisitBlock(ASTBlock node)
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
                    if (currentType != null && currentType.Name != "void" && returnType.Name != "void" && currentType != returnType)
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

            return BoolOperators.Contains(node.Type) ? _currentScope.LookupSingle("bool") : leftType;
        }

        private Symbol VisitUnaryOperator(ASTUnaryOperator node)
        {
            var type = Visit(node.Expression);

            return BoolOperators.Contains(node.Type) ? _currentScope.LookupSingle("bool") : type;
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

            if (_currentScope.LookupSingle(variableName, true) != null)
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

            return _currentScope.LookupSingle($"{itemType.Name}{ArrayTypeSuffix}");
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
            var variableSymbol = _currentScope.LookupSingle(variableName);
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

                if (node.ArrayIndexTo != null)
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

            var overloadedFunctions = _currentScope.LookupMany(functionName, true);

            _currentScope.Define(functionSymbol);

            Logger.DebugScope($"Enter scope : {functionName}");
            var functionScope = new ScopedSymbolTable(functionName, _currentScope.Level + 1, _currentScope);

            _currentScope = functionScope;

            foreach (var param in node.Parameters)
            {
                var paramName = param.Variable.Name;
                var paramType = _currentScope.LookupSingle(param.VariableType.Name);
                var paramSymbol = new SymbolVariable(paramName, paramType);

                if (_currentScope.LookupSingle(paramName, true) != null)
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

            foreach (var overloadedFunction in overloadedFunctions)
            {
                var function = overloadedFunction as SymbolFunction;

                if (function.Parameters.Count != functionSymbol.Parameters.Count)
                {
                    continue;
                }

                var allParametersHaveSameType = true;
                for (var i = 0; i < function.Parameters.Count; ++i)
                {
                    if (function.Parameters[i].Type != functionSymbol.Parameters[i].Type)
                    {
                        allParametersHaveSameType = false;
                        break;
                    }
                }

                if (allParametersHaveSameType)
                {
                    ThrowSemanticException(ErrorCode.DuplicateIdentifier, node.Token);
                }
            }

            functionSymbol.Body = node.Body;

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : {functionName}");

            return null;
        }

        private Symbol VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var functionSymbols = _currentScope.LookupMany(functionCall.FunctionName);

            if (!functionSymbols.Any())
            {
                ThrowSemanticException(ErrorCode.IdentifierNotFound, functionCall.Token);
            }

            foreach (var overloadedFunction in functionSymbols)
            {
                var formalParameters = (overloadedFunction as SymbolFunction).Parameters;
                var actualParameters = functionCall.ActualParameters;

                if (formalParameters.Count != actualParameters.Count)
                {
                    continue;
                }

                var parametersAreValid = true;
                for (var i = 0; i < formalParameters.Count; ++i)
                {
                    var param = functionCall.ActualParameters[i];
                    var actualParamType = Visit(param);
                    var formalParamType = formalParameters[i].Type;
                    if (actualParamType != formalParamType)
                    {
                        parametersAreValid = false;
                        break;
                    }
                }

                if (!parametersAreValid)
                {
                    continue;
                }

                functionCall.SymbolFunction = overloadedFunction as SymbolFunction;

                var returnType = (overloadedFunction as SymbolFunction).ReturnType;

                return returnType.Name == "void" ? null : returnType;
            }

            ThrowSemanticException(ErrorCode.WrongParamNumber, functionCall.Token);

            return null;
        }

        private Symbol PrepareTypeSymbol(ASTType node)
        {
            var typeName = node.Name;
            var typeSymbol = _currentScope.LookupSingle(typeName);

            if (node.TypeSpec is ASTArrayType)
            {
                var arrayTypeName = $"{typeName}{ArrayTypeSuffix}";

                var arrayTypeSymbol = _currentScope.LookupSingle(arrayTypeName);
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
                case ASTBlock block:
                    foreach (var children in block.Children)
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
