using System;
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
                default:
                    throw new ArgumentException($"[{nameof(SemanticAnalyzer)}] No visit method for node type {node.GetType()}");
            }
        }

        private void VisitNumber(ASTNumber node)
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

            if(_currentScope.Lookup(variableName, true) != null)
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
            if(variableSymbol is null)
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
            var functionScope = new ScopedSymbolTable(functionName, _currentScope.Level + 1, _currentScope) ;

            _currentScope = functionScope;

            foreach(var param in node.Parameters)
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

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : {functionName}");
        }

        private void VisitFunctionCall(ASTFunctionCall functionCall)
        {
            var functionSymbol = _currentScope.Lookup(functionCall.FunctionName);
            var formalParameters = (functionSymbol as SymbolFunction).Parameters;
            var actualParameters = functionCall.ActualParameters;

            if(formalParameters.Count != actualParameters.Count)
            {
                ThrowSemanticException(ErrorCode.WrongParamNumber, functionCall.Token);
            }

            foreach(var param in functionCall.ActualParameters)
            {
                Visit(param);
            }
        }

        private void ThrowSemanticException(ErrorCode errorCode, Token token)
        {
            var message = $"{ErrorCodes.StringRepresentatnion[errorCode]} -> {token}";
            throw new SemanticError(errorCode, token, message);
        }
    }
}
