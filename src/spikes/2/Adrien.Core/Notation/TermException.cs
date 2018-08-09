﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Adrien.Notation
{
    public class TermException : Exception
    {
        public Term Term { get; protected set; }

        public TermException(Term t, string message) : base(message)
        {
            Term = t;
        }
    }
}
