using System;
using Interpreter.AST;

namespace Interpreter.Symbols
{
    public class SymbolTableBuilder
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();

        public void DebugPrintSymbolTable()
        {
            _symbolTable.DebugPrintSymbols();
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
                default:
                    throw new ArgumentException($"[SymbolTableBuilder] No visit method for node type {node.GetType()}");
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
            Visit(node.Root);
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
            var typeSymbol = _symbolTable.Lookup(typeName);

            var variableName = node.Variable.Name;
            var variableSymbol = new SymbolVariable(variableName, typeSymbol);

            _symbolTable.Define(variableSymbol);
        }

        private void VisitAssign(ASTAssign node)
        {
            switch (node.Left)
            {
                case ASTVariable variable:
                    VisitVariable(variable);
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
            var variableSymbol = _symbolTable.Lookup(variableName);
        }
    }
}
