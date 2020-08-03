using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Common.AST;
using Interpreter.Common.Tokens;

namespace Interpreter.ParserService
{
    public partial class Parser
    {
        private ASTReturn ReturnStatement()
        {
            ASTReturn result;
            var returnToken = _currentToken;
            Eat(TokenType.Return);
            result = _currentToken.Type == TokenType.Semicolon ? new ASTReturn(returnToken, null) : new ASTReturn(returnToken, Expression());
            Eat(TokenType.Semicolon);
            return result;
        }

        private ASTBreak BreakStatement()
        {
            var token = _currentToken;
            Eat(TokenType.Break);
            Eat(TokenType.Semicolon);
            return new ASTBreak(token);
        }

        private ASTContinue ContinueStatement()
        {
            var token = _currentToken;
            Eat(TokenType.Continue);
            Eat(TokenType.Semicolon);
            return new ASTContinue(token);
        }

        private ASTWhile WhileStatement()
        {
            var token = _currentToken;
            Eat(TokenType.While);
            Eat(TokenType.LeftParen);
            var condition = Expression();
            Eat(TokenType.RightParen);
            var body = Statement();

            return new ASTWhile(token, condition, body);
        }

        private ASTIfElse IfElseStatement()
        {
            var token = _currentToken;
            Eat(TokenType.If);
            Eat(TokenType.LeftParen);
            var condition = Expression();
            Eat(TokenType.RightParen);
            var ifTrue = Statement();
            if (_currentToken.Type == TokenType.Else)
            {
                Eat(TokenType.Else);
                return new ASTIfElse(token, condition, ifTrue, Statement());
            }

            return new ASTIfElse(token, condition, ifTrue, null);
        }

        private ASTFor ForStatement()
        {
            var token = _currentToken;
            Eat(TokenType.For);
            Eat(TokenType.LeftParen);
            var assignments = new List<ASTAssign>();
            if (_currentToken.Type != TokenType.Semicolon)
            {
                assignments = ForInitializationAssignments();
            }
            Eat(TokenType.Semicolon);

            ASTNode condition = null;
            if (_currentToken.Type != TokenType.Semicolon)
            {
                condition = Expression();
            }
            Eat(TokenType.Semicolon);

            var continueStatements = new List<ASTNode>();
            if (_currentToken.Type != TokenType.RightParen)
            {
                continueStatements = ForContinueStatement();
            }
            Eat(TokenType.RightParen);

            var statement = Statement();

            return new ASTFor(token, assignments, condition, continueStatements, statement);
        }

        private List<ASTAssign> ForInitializationAssignments()
        {
            var assignments = new List<ASTAssign>();
            while (_currentToken.Type != TokenType.Semicolon)
            {
                ASTAssign assignment;
                if (BuilinVarTypes.Contains(_currentToken.Type))
                {
                    var variablesDeclarations = VariablesDeclarations();
                    assignment = AssignmentStatement(variablesDeclarations);

                }
                else
                {
                    var firstVariable = Variable();
                    assignment = AssignmentStatement(firstVariable);

                }
                assignments.Add(assignment);
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }

            return assignments;
        }

        private List<ASTNode> ForContinueStatement()
        {
            var continueStatements = new List<ASTNode>();

            while (_currentToken.Type != TokenType.RightParen)
            {
                var firstVariable = Variable();
                var assignment = AssignmentStatement(firstVariable);
                continueStatements.Add(assignment);
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }

            return continueStatements;
        }
    }
}
