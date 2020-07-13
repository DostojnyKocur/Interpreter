namespace Interpreter
{
    public class VisitResult
    {
        public ControlType ControlType { get; set; } = ControlType.None;
        public dynamic Value { get; set; } = null;
    }
}
