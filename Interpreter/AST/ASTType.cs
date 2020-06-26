namespace Interpreter.AST
{
    public class ASTType
    {
        private readonly Token _token;

        public ASTType(Token token) => (_token) = (token);

        public string Name => _token.Value;
    }
}
