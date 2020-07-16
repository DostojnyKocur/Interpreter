﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Interpreter.InterpreterService.Memory
{
    public class CallStack
    {
        private readonly Stack<ActivationRecord> _stack = new Stack<ActivationRecord>();

        public ActivationRecord Top => _stack.Peek();

        public void Push(ActivationRecord activationRecord)
        {
            _stack.Push(activationRecord);
        }

        public ActivationRecord Pop()
        {
            return _stack.Pop();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder($"{Environment.NewLine}==== CALL STACK ====");
            stringBuilder.Append(Environment.NewLine);
            foreach (var record in _stack.ToArray())
            {
                stringBuilder.Append(record);
                stringBuilder.Append(Environment.NewLine);
            }
            stringBuilder.Append("==== ==== ====");

            return stringBuilder.ToString();
        }
    }
}
