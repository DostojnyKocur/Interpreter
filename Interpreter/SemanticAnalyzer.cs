using System;
using System.Linq;
using Interpreter.AST;
using Interpreter.Errors;
using Interpreter.Symbols;
using Interpreter.Tokens;

namespace Interpreter
{
    public class SemanticAnalyzer
    {
        private readonly ScopedSymbolTable _globalScope = new ScopedSymbolTable("global", 1);
        private ScopedSymbolTable _currentScope;

        public void DebugPrintSymbolTable()
        {
            _currentScope.DebugPrintSymbols();
        }

        public void Prepare(ASTNode node)
        {
            Visit(node);
        }

        private void Visit(ASTNode node)
        {
            switch (node)
            {
                case ASTEmpty empty:
                    VisitEmpty(empty);
                    break;
                case ASTProgram program:
                    VisitProgram(program);
                    break;
                case ASTCompound compound:
                    VisitCompound(compound);
                    break;
                case ASTBinaryOperator binaryOperator:
                    VisitBinaryOperator(binaryOperator);
                    break;
                case ASTUnaryOperator unaryOperator:
                    VisitUnaryOperator(unaryOperator);
                    break;
                case ASTVariablesDeclarations variablesDeclarations:
                    VisitVariablesDeclarations(variablesDeclarations);
                    break;
                case ASTVariableDeclaration variableDeclaration:
                    VisitVariableDeclaration(variableDeclaration);
                    break;
                case ASTNumber number:
                    VisitNumber(number);
                    break;
                case ASTBool @bool:
                    VisitBool(@bool);
                    break;
                case ASTString @string:
                    VisitString(@string);
                    break;
                case ASTAssign assign:
                    VisitAssign(assign);
                    break;
                case ASTVariable variable:
                    VisitVariable(variable);
                    break;
                case ASTFunctionDefinition functionDefinition:
                    VisitFunctionDefinition(functionDefinition);
                    break;
                case ASTFunctionCall functionCall:
                    VisitFunctionCall(functionCall);
                    break;
                case ASTReturn returnStatement:
                    VisitReturnStatement(returnStatement);
                    break;
                case ASTBreak breakStatement:
                    VisitBreakStatement(breakStatement);
                    break;
                case ASTContinue continueStatement:
                    VisitContinueStatement(continueStatement);
                    break;
                case ASTIfElse ifElseStatement:
                    VisitIfElseStatement(ifElseStatement);
                    break;
                case ASTWhile whileStatement:
                    VisitWhileStatement(whileStatement);
                    break;
                default:
                    throw new ArgumentException($"[{nameof(SemanticAnalyzer)}] No visit method for node type {node.GetType()}");
            }
        }

        private void VisitNumber(ASTNumber node)
        {
            return;
        }

        private void VisitBool(ASTBool node)
        {
            return;
        }

        private void VisitString(ASTString node)
        {
            return;
        }

        private void VisitEmpty(ASTEmpty node)
        {
            return;
        }

        private void VisitProgram(ASTProgram node)
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
        }

        private void VisitCompound(ASTCompound node)
        {
            foreach (var child in node.Children)
            {
                Visit(child);
            }
        }

        private void VisitBinaryOperator(ASTBinaryOperator node)
        {
            Visit(node.Left);
            Visit(node.Right);
        }

        private void VisitUnaryOperator(ASTUnaryOperator node)
        {
            Visit(node.Expression);
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
            var typeName = node.Type.Name;
            var typeSymbol = _currentScope.Lookup(typeName);

            var variableName = node.Variable.Name;
            var variableSymbol = new SymbolVariable(variableName, typeSymbol);

            if (_currentScope.Lookup(variableName, true) != null)
            {
                ThrowSemanticException(ErrorCode.DuplicateIdentifier, node.Variable.Token);
            }

            _currentScope.Define(variableSymbol);
        }

        private void VisitAssign(ASTAssign node)
        {
            switch (node.Left)
            {
                case ASTVariable variable:
                    Visit(variable);
                    break;
                case ASTVariablesDeclarations variablesDeclarations:
                    Visit(variablesDeclarations);
                    break;
                default:
                    throw new ArgumentException($"Invalid AST node type {node.GetType()}");
            }

            Visit(node.Right);
        }

        private void VisitVariable(ASTVariable node)
        {
            var variableName = node.Name;
            var variableSymbol = _currentScope.Lookup(variableName);
            if (variableSymbol is null)
            {
                ThrowSemanticException(ErrorCode.IdentifierNotFound, node.Token);
            }
        }

        private void VisitFunctionDefinition(ASTFunctionDefinition node)
        {
            var typeName = node.ReturnType.Name;
            var typeSymbol = _currentScope.Lookup(typeName);

            var functionName = node.Name.Value;
            var functionSymbol = new SymbolFunction(functionName, typeSymbol);

            if (_currentScope.Lookup(functionName, true) != null)
            {
                ThrowSemanticException(ErrorCode.DuplicateIdentifier, node.Name);
            }

            _currentScope.Define(functionSymbol);

            Logger.DebugScope($"Enter scope : {functionName}");
            var functionScope = new ScopedSymbolTable(functionName, _currentScope.Level + 1, _currentScope);

            _currentScope = functionScope;

            foreach (var param in node.Parameters)
            {
                var paramName = param.Variable.Name;
                var paramType = _currentScope.Lookup(param.Type.Name);
                var paramSymbol = new SymbolVariable(paramName, paramType);

                if (_currentScope.Lookup(paramName, true) != null)
                {
                    ThrowSemanticException(ErrorCode.DuplicateIdentifier, param.Variable.Token);
                }
                _currentScope.Define(paramSymbol);

                functionSymbol.Parameters.Add(paramSymbol);
            }

            Visit(node.Body);

            if (typeName != "void" && !HasReturnStatement(node.Body))
            {
                ThrowSemanticException(ErrorCode.MissingReturnStatement, node.Name);
            }

            functionSymbol.Body = node.Body;

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : {functionName}");
        }

        private void VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var functionSymbol = _currentScope.Lookup(functionCall.FunctionName);

            if (functionSymbol is null)
            {
                ThrowSemanticException(ErrorCode.IdentifierNotFound, functionCall.Token);
            }

            var formalParameters = (functionSymbol as SymbolFunction).Parameters;
            var actualParameters = functionCall.ActualParameters;

            if (formalParameters.Count != actualParameters.Count)
            {
                ThrowSemanticException(ErrorCode.WrongParamNumber, functionCall.Token);
            }

            foreach (var param in functionCall.ActualParameters)
            {
                Visit(param);
            }

            functionCall.SymbolFunction = (functionSymbol as SymbolFunction);
        }

        private void VisitReturnStatement(ASTReturn node)
        {
            if (node.Expression != null)
            {
                Visit(node.Expression);
            }
        }

        private void VisitBreakStatement(ASTBreak node)
        {
            return;
        }

        private void VisitContinueStatement(ASTContinue node)
        {
            return;
        }

        private void VisitIfElseStatement(ASTIfElse node)
        {
            Visit(node.Condition);

            Logger.DebugScope($"Enter scope : if");
            var ifScope = new ScopedSymbolTable("if", _currentScope.Level + 1, _currentScope);
            _currentScope = ifScope;

            Visit(node.IfTrue);

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : if");

            if (node.Else != null)
            {
                Logger.DebugScope($"Enter scope : else");
                var elseScope = new ScopedSymbolTable("else", _currentScope.Level + 1, _currentScope);
                _currentScope = elseScope;

                Visit(node.Else);

                DebugPrintSymbolTable();

                _currentScope = _currentScope.EnclosingScope;
                Logger.DebugScope($"Leave scope : else");
            }
        }

        private void VisitWhileStatement(ASTWhile node)
        {
            Visit(node.Condition);

            Logger.DebugScope($"Enter scope : while");
            var whileScope = new ScopedSymbolTable("while", _currentScope.Level + 1, _currentScope);
            _currentScope = whileScope;

            Visit(node.Body);

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : while");
        }

        private void ThrowSemanticException(ErrorCode errorCode, Token token)
        {
            var message = $"{ErrorCodes.StringRepresentatnion[errorCode]} -> {token}";
            throw new SemanticError(errorCode, token, message);
        }

        private bool HasReturnStatement(ASTNode node)
        {
            if(node is null)
            {
                return false;
            }

            var result = false;
            switch (node)
            {
                case ASTReturn @return:
                    return true;
                case ASTCompound compound:
                    foreach(var children in compound.Children)
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
