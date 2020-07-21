using System.Collections.Generic;
using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTFor : ASTNode
    {
        public ASTFor(Token token, IEnumerable<ASTAssign> assignments, ASTNode condition, List<ASTNode> continueStatements, ASTNode body)
        {
            Token = token;
            Assignments.AddRange(assignments);
            Condition = condition;
            ContinueStatements.AddRange(continueStatements);
            Body = body;
        }

        public List<ASTAssign> Assignments { get; } = new List<ASTAssign>();
        public ASTNode Condition { get; }
        public List<ASTNode> ContinueStatements { get; } = new List<ASTNode>();
        public ASTNode Body { get; }
    }
}
