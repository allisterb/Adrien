﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Adrien.Compiler
{
    public interface IRunnable<T> where T : unmanaged, IEquatable<T>, IComparable<T>, IConvertible
    {
        bool Run(IEnumerable<IVariable<T>> inputData, IVariable<T> output);
    }
}
