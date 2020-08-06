using Interpreter.Common;
using Interpreter.Common.AST;
using Interpreter.Common.Symbols;

namespace Interpreter.AnalyzerService
{
    public partial class SemanticAnalyzer
    {
        private Symbol VisitReturnStatement(ASTReturn node)
        {
            if (node.Expression != null)
            {
                return Visit(node.Expression);
            }

            return _currentScope.LookupSingle("void");
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

            foreach (var elif in node.Elifs)
            {
                var elifType = Visit(elif);

                if (returnType is null)
                {
                    returnType = elifType;
                }

                if (returnType != elifType)
                {
                    ThrowIncompatibleTypesException(node.Token, returnType.Name, elifType.Name);
                }
            }

            if (node.Else != null)
            {
                Logger.DebugScope($"Enter scope : else");
                var elseScope = new ScopedSymbolTable("else", _currentScope.Level + 1, _currentScope);
                _currentScope = elseScope;

                var elseType = Visit(node.Else);

                if (returnType is null)
                {
                    returnType = elseType;
                }

                if (returnType != elseType)
                {
                    ThrowIncompatibleTypesException(node.Token, returnType.Name, elseType.Name);
                }

                DebugPrintSymbolTable();

                _currentScope = _currentScope.EnclosingScope;
                Logger.DebugScope($"Leave scope : else");
            }

            return returnType;
        }

        private Symbol VisitElifStatement(ASTElif node)
        {
            var conditionType = Visit(node.Condition);
            if (conditionType.Name != "bool")
            {
                ThrowIncompatibleTypesException(node.Token, conditionType.Name, "bool");
            }

            Logger.DebugScope($"Enter scope : elif");
            var ifScope = new ScopedSymbolTable("if", _currentScope.Level + 1, _currentScope);
            _currentScope = ifScope;

            var returnType = Visit(node.IfTrue);

            DebugPrintSymbolTable();

            _currentScope = _currentScope.EnclosingScope;
            Logger.DebugScope($"Leave scope : elif");

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
    }
}
