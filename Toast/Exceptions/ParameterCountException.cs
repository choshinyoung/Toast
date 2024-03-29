﻿using System;

namespace Toast.Exceptions
{
    public class ParameterCountException : Exception
    {
        public ParameterCountException() { }

        public ParameterCountException(int given, int expect) : base($"{given} Arguments is not valid in this line. {expect} expected.") { }
    }
}
