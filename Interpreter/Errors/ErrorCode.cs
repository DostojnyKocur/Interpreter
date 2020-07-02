﻿using System.Collections.Generic;

namespace Interpreter.Errors
{
    public enum ErrorCode
    {
        UnexpectedToken,
        IdentifierNotFound,
        DuplicateIdentifier,
        WrongParamNumber
    }

    public static class ErrorCodes
    {
        public static readonly Dictionary<ErrorCode, string> StringRepresentatnion = new Dictionary<ErrorCode, string>
        {
            { ErrorCode.UnexpectedToken, "Unexpected token" },
            { ErrorCode.IdentifierNotFound, "Identifier not found" },
            { ErrorCode.DuplicateIdentifier, "Duplicate identifier found" },
            { ErrorCode.WrongParamNumber, "Wrong number of arguments" }
        };
    }
}