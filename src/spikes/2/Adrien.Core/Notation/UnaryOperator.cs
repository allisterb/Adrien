﻿using System;
using System.Linq.Expressions;

namespace Adrien.Notation
{
    public class UnaryOperator : IElementwiseOp
    {
        protected Func<TensorExpression, TensorExpression> Operation { get; set; }


        public UnaryOperator(Func<TensorExpression, TensorExpression> op)
        {
            Operation = op;
        }

        public TensorExpression this[TensorExpression e] => Operation(e);

        public UnaryOperator this[UnaryOperator right] => new UnaryOperator((e) => this[right[e]]);

        public BinaryOperator this[BinaryOperator right] => new BinaryOperator((l, r) => this[right[l, r]]);

        public static UnaryOperator operator |(UnaryOperator left, UnaryOperator right)
            => new UnaryOperator((e) => right[left[e]]);

        public static UnaryOperator operator +(UnaryOperator left, UnaryOperator right)
        {
            return new UnaryOperator((e) => { return new TensorExpression(Expression.Add(left[e], right[e])); });
        }

        public static UnaryOperator operator -(UnaryOperator left, UnaryOperator right)
        {
            return new UnaryOperator((e) => { return new TensorExpression(Expression.Subtract(left[e], right[e])); });
        }

        public static UnaryOperator operator *(UnaryOperator left, UnaryOperator right)
        {
            return new UnaryOperator((e) => { return new TensorExpression(Expression.Multiply(left[e], right[e])); });
        }

        public static UnaryOperator operator /(UnaryOperator left, UnaryOperator right)
        {
            return new UnaryOperator((e) => { return new TensorExpression(Expression.Divide(left[e], right[e])); });
        }
    }
}