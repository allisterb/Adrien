﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.Linq;
using System.Linq.Expressions;

using AgileObjects.ReadableExpressions;
using Sawmill;
using Sawmill.Expressions;

using Adrien.Notation;

namespace Adrien.Trees
{
    public static class ExpressionExtensions
    {
        [DebuggerStepThrough]
        public static TensorOp ToOp(this ExpressionType et)
        {
            switch(et)
            {
                case ExpressionType.Index:
                    return TensorOp.Summation;
                case ExpressionType.Multiply:
                    return TensorOp.Mul;
                case ExpressionType.Add: return TensorOp.Add;
                default:
                    throw new Exception($"Cannot translate expression type: {et} to TensorOp.");
            }
        }

        [DebuggerStepThrough]
        public static TExpr As<TExpr>(this Expression expr) where TExpr : Expression
        {
            return (expr as TExpr) ?? throw new Exception($"This expression {expr.ToReadableString()} is not type {typeof(TExpr).ToString()}.");
        }

        [DebuggerStepThrough]
        public static List<T> GetConstants<T>(this Expression expr) where T : ITerm
        {
            return expr.DescendantsAndSelf()
                   .OfType<ConstantExpression>()
                   .Select(e => e.Value)
                   .Cast<Array>()
                   .Select(a => a.Flatten<T>().First())
                   .ToList();
        }

        [DebuggerStepThrough]
        public static List<T> GetIndexObjects<T>(this Expression expr) where T : ITerm
        {
            return expr.DescendantsAndSelf()
                   .OfType<IndexExpression>()
                   .Select(e => e.Object)
                   .Cast<Array>()
                   .Select(a => a.Flatten<T>().First())
                   .ToList();
        }
    }
}
