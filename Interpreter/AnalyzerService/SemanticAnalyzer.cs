using System;
using System.Linq;
using Interpreter.AnalyzerService.Symbols;
using Interpreter.Common;
using Interpreter.Common.Errors;
using Interpreter.LexerService.Tokens;
using Interpreter.ParserService.AST;
using Interpreter.Symbols;

namespace Interpreter.AnalyzerService
{
    public class SemanticAnalyzer : ISemanticAnalyzer
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
                case ASTIndexExpression indexExpression:
                    return VisitIndexExpression(indexExpression);
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
                    if (currentType.Name != returnType.Name)
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

            if(BoolOperators.Contains(node.Type))
            {
                return new Symbol("bool");
            }

            return leftType;
        }

        private Symbol VisitUnaryOperator(ASTUnaryOperator node)
        {
            return Visit(node.Expression);
        }

        private Symbol VisitVariablesDeclarations(ASTVariablesDeclarations node)
        {
            Symbol variableType = null;

            foreach (var child in node.Children)
            {
                if (variableType is null)
                {
                    variableType = Visit(child);
                }
                else
                {
                    Visit(child);
                }
            }

            return variableType;
        }

        private Symbol VisitVariableDeclaration(ASTVariableDeclaration node)
        {
            var typeName = node.VariableType.Name;
            var typeSymbol = _currentScope.Lookup(typeName);

            if (node.VariableType.TypeSpec is ASTArrayType)
            {
                typeSymbol = new Symbol($"{typeName}{ArrayTypeSuffix}");
            }

            var variableName = node.Variable.Name;
            var variableSymbol = new SymbolVariable(variableName, typeSymbol);

            if (_currentScope.Lookup(variableName, true) != null)
            {
                ThrowSemanticException(ErrorCode.DuplicateIdentifier, node.Variable.Token);
            }

            _currentScope.Define(variableSymbol);

            return typeSymbol;
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
                    if (currentItemType != itemType)
                    {
                        ThrowIncompatibleTypesException(node.Token, itemType.Name, currentItemType.Name);
                    }
                }
            }

            return new Symbol($"{itemType.Name}{ArrayTypeSuffix}");
        }

        private Symbol VisitIndexExpression(ASTIndexExpression node)
        {
            var variableType = Visit(node.Variable);
            var indexType = Visit(node.Expression);
            switch (indexType.Name)
            {
                case "number":
                    var singleType = variableType.Name.Replace(ArrayTypeSuffix, string.Empty);
                    return _currentScope.Lookup(singleType);
                default:
                    ThrowIncompatibleTypesException(node.Token, "index type", indexType.Name);
                    break;

            }

            return variableType;
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
                    leftType = Visit(variablesDeclarations);
                    break;
                default:
                    throw new ArgumentException($"Invalid AST node type {node.GetType()}");
            }

            var rightType = Visit(node.Right);

            if (leftType.Name != rightType.Name)
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

            return variableSymbol.Type;
        }

        private Symbol VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            var typeName = node.ReturnType.Name;
            var typeSymbol = _currentScope.Lookup(typeName);

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
            if (typeName != "void" && (returnedType.Name != typeSymbol.Name))
            {
                ThrowIncompatibleTypesException(node.Token, typeSymbol.Name, returnedType.Name);
            }

            if (typeName != "void" && !HasReturnStatement(node.Body))
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
                if (actualParamType.Name != formalParamType.Name)
                {
                    ThrowIncompatibleTypesException(param.Token, formalParamType.Name, actualParamType.Name);
                }
            }

            functionCall.SymbolFunction = functionSymbol as SymbolFunction;

            return (functionSymbol as SymbolFunction).ReturnType;
        }

        private Symbol VisitReturnStatement(ASTReturn node)
        {
            if (node.Condition != null)
            {
                return Visit(node.Condition);
            }

            return null;
        }

        private Symbol VisitBreakStatement(ASTBreak node)
        {
            return null;
        }

        private Symbol VisitContinueStatement(ASTContinue node)
        {
            return null;
        }

        private Symbol VisitIfElseStatement(ASTIfElse node)
        {
            var conditionType = Visit(node.Condition);
            if (conditionType.Name != "bool")
            {
                ThrowIncompatibleTypesException(node.Token, conditionType.Name, "bool");
            }

            Logger.DebugScope($"Enter scope : if");
            var ifScope = new ScopedSymbolTable("if", _currentScope.Level + 1, _currentScope);
            _currentScope = ifScope;

            var returnType = Visit(node.IfTrue);

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : if");

            if (node.Else != null)
            {
                Logger.DebugScope($"Enter scope : else");
                var elseScope = new ScopedSymbolTable("else", _currentScope.Level + 1, _currentScope);
                _currentScope = elseScope;

                var elseType = Visit(node.Else);

                DebugPrintSymbolTable();

                _currentScope = _currentScope.EnclosingScope;
                Logger.DebugScope($"Leave scope : else");
            }

            return returnType;
        }

        private Symbol VisitWhileStatement(ASTWhile node)
        {
            var conditionType = Visit(node.Condition);
            if (conditionType.Name != "bool")
            {
                ThrowIncompatibleTypesException(node.Token, conditionType.Name, "bool");
            }

            Logger.DebugScope($"Enter scope : while");
            var whileScope = new ScopedSymbolTable("while", _currentScope.Level + 1, _currentScope);
            _currentScope = whileScope;

            var returnType = Visit(node.Body);

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : while");

            return returnType;
        }

        private Symbol VisitForStatement(ASTFor node)
        {
            Logger.DebugScope($"Enter scope : for");
            var whileScope = new ScopedSymbolTable("for", _currentScope.Level + 1, _currentScope);
            _currentScope = whileScope;

            foreach (var assign in node.Assignments)
            {
                Visit(assign);
            }

            if (node.Condition != null)
            {
                Visit(node.Condition);
            }

            foreach (var statement in node.ContinueStatements)
            {
                Visit(statement);
            }

            var returnType = Visit(node.Body);

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : for");

            return returnType;
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
