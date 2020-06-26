﻿using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.AST;

namespace Interpreter
{
    public class Parser
    {
        private static readonly TokenType[] TermOperators = { TokenType.Plus, TokenType.Minus };
        private static readonly TokenType[] FactorOperators = { TokenType.Multiplication, TokenType.Divide, TokenType.Modulo };

        private readonly Lexer _lexer;
        private Token _currentToken = null;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        public ASTNode Parse()
        {
            var node = Program();
            if(_currentToken.Type != TokenType.EOF)
            {
                ThrowParsingException();
            }

            return node;
        }

        private ASTProgram Program()
        {
            return new ASTProgram(Statement());
        }

        private ASTCompound CompoundStatement()
        {
            Eat(TokenType.ScopeBegin);
            var nodes = StatementList();
            Eat(TokenType.ScopeEnd);
            return new ASTCompound(nodes);
        }

        private IEnumerable<ASTNode> StatementList()
        {
            var results = new List<ASTNode>();

            results.Add(Statement());

            while (_currentToken.Type != TokenType.ScopeEnd)
            {
                results.Add(Statement());
            }

            if (_currentToken.Type == TokenType.Id)
            {
                ThrowParsingException();
            }

            return results;
        }

        private ASTNode Statement()
        {
            switch(_currentToken.Type)
            {
                case TokenType.ScopeBegin:
                    return CompoundStatement();
                case TokenType.Id:
                    var assignmentStatement = AssignmentStatement();
                    Eat(TokenType.Semicolon);
                    return assignmentStatement;
                case TokenType.TypeNumber:
                    var variablesDeclarations = VariablesDeclarations();
                    if (_currentToken.Type == TokenType.Semicolon)
                    {
                        Eat(TokenType.Semicolon);
                        return variablesDeclarations;
                    }
                    if(_currentToken.Type == TokenType.Assign)
                    {
                        var assignment = AssignmentStatement(variablesDeclarations);
                        Eat(TokenType.Semicolon);
                        return assignment;
                    }
                    ThrowParsingException();
                    return null;
                default:
                    return Empty();
            }
        }

        private ASTAssign AssignmentStatement(ASTNode leftNode = null)
        {
            var left = leftNode ?? Variable();
            var token = _currentToken;
            Eat(TokenType.Assign);
            var right = Expression();
            return new ASTAssign(left, token, right);
        }

        private ASTNode Expression()
        {
            var node = Term();

            while (TermOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Plus:
                        Eat(TokenType.Plus);
                        break;
                    case TokenType.Minus:
                        Eat(TokenType.Minus);
                        break;
                }

                node = new ASTBinaryOperator(node, token, Term());
            }

            return node;
        }

        private ASTNode Term()
        {
            var node = Factor();

            while (FactorOperators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Multiplication:
                        Eat(TokenType.Multiplication);
                        break;
                    case TokenType.Divide:
                        Eat(TokenType.Divide);
                        break;
                    case TokenType.Modulo:
                        Eat(TokenType.Modulo);
                        break;
                }

                node = new ASTBinaryOperator(node, token, Factor());
            }

            return node;
        }

        private ASTNode Factor()
        {
            var token = _currentToken;

            switch (token.Type)
            {
                case TokenType.Plus:
                    Eat(TokenType.Plus);
                    return new ASTUnaryOperator(token, Factor());
                case TokenType.Minus:
                    Eat(TokenType.Minus);
                    return new ASTUnaryOperator(token, Factor());
                case TokenType.ConstNumber:
                    Eat(TokenType.ConstNumber);
                    return new ASTNumber(token);
                case TokenType.LeftParen:
                    Eat(TokenType.LeftParen);
                    var node = Expression();
                    Eat(TokenType.RightParen);
                    return node;
                default:
                    Eat(TokenType.Id);
                    return new ASTVariable(token);
            }
        }

        private ASTVariablesDeclarations VariablesDeclarations()
        {
            var variableType = Type();
            var variables = new List<ASTVariable>();

            variables.Add(Variable());

            while(_currentToken.Type == TokenType.Comma)
            {
                Eat(TokenType.Comma);
                variables.Add(Variable());
            }

            var result = new List<ASTVariableDeclaration>();

            foreach(var variable in variables)
            {
                result.Add(new ASTVariableDeclaration(variable, variableType));
            }

            return new ASTVariablesDeclarations(result);
        }

        private ASTVariable Variable()
        {
            var node = new ASTVariable(_currentToken);
            Eat(TokenType.Id);
            return node;
        }

        private ASTType Type()
        {
            var node = new ASTType(_currentToken);
            Eat(TokenType.TypeNumber);
            return node;
        }

        private ASTEmpty Empty()
        {
            return new ASTEmpty();
        }

        private void Eat(TokenType tokenType)
        {
            if (_currentToken.Type == tokenType)
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                ThrowParsingException();
            }
        }

        private void ThrowParsingException()
        {
            throw new InvalidOperationException("Error parsing input");
        }
    }
}
