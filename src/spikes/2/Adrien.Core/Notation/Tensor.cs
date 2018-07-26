﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Humanizer;

using Adrien.Compiler;
using Adrien.Trees;

namespace Adrien.Notation
{
    public partial class Tensor : Term, IAlgebra<Tensor, TensorExpression>, IVariableShape
    {
        public int[] Dimensions { get; protected set; }

        public int[] Stride { get; protected set; }

        public int Rank => Dimensions.Length;

        public int NumberofElements
        {
            get
            {
                var n = 1;
                for (int i = 0; i < Dimensions.Length; i++)
                {
                    n *= Dimensions[i];
                }
                return n;
            }
        }

        public (IndexSet IndexSet, TensorExpression Expression) IndexedAssignment { get; protected set; }

        public (IEnumerable<Tensor> Dependents, TensorExpression Expression) ElementwiseAssignment { get; protected set; }

        public bool IsAssigned => IndexedAssignment.Expression != null || ElementwiseAssignment.Expression != null;

        public bool IsElementwiseAssigned => this.ElementwiseAssignment.Expression != null;

        public bool IsIndexAssigned => this.IndexedAssignment.Expression != null;

        internal override Expression LinqExpression => this.IsAssigned ? this.IsIndexAssigned 
            ? this.IndexedAssignment.Expression.LinqExpression : this.ElementwiseAssignment.Expression.LinqExpression 
            : Expression.Constant(this, typeof(Tensor));
        
        internal override Name DefaultNameBase { get; } = "A";

        protected (Tensor tensor, int index)? GeneratorContext { get; set; }


        public Tensor(string name, params int [] dim) : base(name)
        {
            if (dim == null || dim.Length == 0)
            {
                throw new ArgumentException("The number of dimensions must be at least 1.");
            }
            Dimensions = dim;
            Stride = GenericMath<int>.StrideInElements(Dimensions);
        }

        public Tensor(params int[] dim) : this("A", dim)
        {

        }

        public Tensor(string name, string indexNameBase, out IndexSet I, params int[] dim) : this(name, dim)
        {
            I = new IndexSet(this, indexNameBase, dim);
        }

        public Tensor(IVariableShape shape) : this(shape.Label, shape.Dimensions) {}

        internal Tensor((TensorExpression te, string name) expr) : base(expr.name)
        {
            IndexedAssignment = (null, expr.te);
        }


        public TensorExpression this[IndexSet I]
        {
            get
            {
                ThrowIfIndicesExceedRank(1);

                int[] tdim = new int[I.Indices.Count];
                int[] tidx = new int[I.Indices.Count];
                for (int i= 0; i < tdim.Length; i++)
                {
                    tdim[i] = 1;
                }
                Array t = Array.CreateInstance(typeof(Tensor), tdim);
                t.SetValue(this, tidx);
                Expression[] e = I.Indices.Select(i => Expression.Parameter(typeof(int), i.Name)).ToArray();
                return new TensorExpression(Expression.ArrayAccess(Expression.Constant(t), e));
            }
            set
            {
                ThrowIfAlreadyAssiged();
                IndexedAssignment = (I, value);
            }
        }

        public static implicit operator TensorExpression(Tensor t)
        {
            return new TensorExpression(t.LinqExpression);
        }

        public static implicit operator ExpressionTree(Tensor t)
        {
            if (t.IsAssigned)
            {
                return t.IndexedAssignment.Expression.ToTree((t, t.IndexedAssignment.IndexSet));
            }
            else
            {
                return new TensorExpression(t.LinqExpression).ToTree();
            }
        }

        public static TensorExpression operator - (Tensor left) => left.Negate();
        

        public static TensorExpression operator + (Tensor left, Tensor right) => left.Add(right);
        

        public static TensorExpression operator - (Tensor left, Tensor right) => left.Subtract(right);
       

        public static TensorExpression operator * (Tensor left, Tensor right) => left.Multiply(right);
        

        public static TensorExpression operator / (Tensor left, Tensor right) => left.Divide(right);
        

        public TensorExpression Negate() => - (TensorExpression) this;

        public TensorExpression Add(Tensor right) => (TensorExpression) this + right;

        public TensorExpression Subtract(Tensor right) => (TensorExpression) this - right;

        public TensorExpression Multiply(Tensor right) => (TensorExpression) this * right;

        public TensorExpression Divide(Tensor right) => (TensorExpression) this / right;

        public Tensor With(out Tensor with) 
        {
            GeneratorContext = GeneratorContext.HasValue ? GeneratorContext.Value : (this, 1);
            with = new Tensor(this.GenerateName(GeneratorContext.Value.index, this.Name), this.Dimensions);
            this.GeneratorContext = (this.GeneratorContext.Value.tensor, this.GeneratorContext.Value.index + 1);
            return this.GeneratorContext.Value.tensor;
        }

        public Tensor With(out Tensor with, string name)
        {
            GeneratorContext = GeneratorContext.HasValue ? GeneratorContext.Value : (this, 1);
            with = new Tensor(name, this.Dimensions);
            this.GeneratorContext = (this.GeneratorContext.Value.tensor, this.GeneratorContext.Value.index + 1);
            return this.GeneratorContext.Value.tensor;
        }

        public Tensor With(out Tensor with, params int[] dim)
        {

            GeneratorContext = GeneratorContext.HasValue ? GeneratorContext.Value : (this, 1);
            if (dim.Length != GeneratorContext.Value.tensor.Dimensions.Length)
            {
                throw new ArgumentException($"The rank of the new tensor must be the same as the original: " +
                $"{dim.Length}.");                                                                            
            }
            with = new Tensor(this.GenerateName(GeneratorContext.Value.index, this.Name), dim);
            this.GeneratorContext = (this.GeneratorContext.Value.tensor, this.GeneratorContext.Value.index + 1);
            return this.GeneratorContext.Value.tensor;
        }

        public Tensor With(out Tensor with, string name, params int[] dim)
        {

            GeneratorContext = GeneratorContext.HasValue ? GeneratorContext.Value : (this, 1);
            if (dim.Length != GeneratorContext.Value.tensor.Dimensions.Length)
            {
                throw new ArgumentException($"The rank of the new tensor must be the same as the original: " +
                $"{dim.Length}.");
            }
            with = new Tensor(name, dim);
            this.GeneratorContext = (this.GeneratorContext.Value.tensor, this.GeneratorContext.Value.index + 1);
            return this.GeneratorContext.Value.tensor;
        }

        public ExpressionTree ToTree () => this.IsAssigned ? 
            this.IsIndexAssigned ? 
            this.IndexedAssignment.Expression.ToTree((this, this.IndexedAssignment.IndexSet)) :
            this.ElementwiseAssignment.Expression.ToTree((this, null)) : 
            new TensorExpression(this.LinqExpression).ToTree();
        
        public Var<T> Var<T>(Array array) where T : unmanaged, IEquatable<T>, IComparable<T>, IConvertible 
            => new Var<T>(this, array);

        public Var<T> Var<T>(params T[] data) where T : unmanaged, IEquatable<T>, IComparable<T>, IConvertible 
            => new Var<T>(this, data);

        public Var<T> Var<T>() where T : unmanaged, IEquatable<T>, IComparable<T>, IConvertible
            => new Var<T>(this, Array.CreateInstance(typeof(T), this.Dimensions));

        public static string RankToTensorName(int rank)
        {
            string[] names = rank.ToWords().Split('-');
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = char.ToUpper(names[i][0]) + names[i].Substring(1);
            }
            return string.Join("", names);
        }

        #region IEnumerable<int> implementation
        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < this.Dimensions.Length; i++)
            {
                yield return this.Dimensions[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion

        [DebuggerStepThrough]
        internal void ThrowIfAlreadyAssiged()
        {
            if (IndexedAssignment.IndexSet != null)
            {
                throw new InvalidOperationException("This tensor variable has an existing assigment. + " +
                    $"You can only assign to a tensor variable once.");
            }
        }

        [DebuggerStepThrough]
        internal void ThrowIfIndicesExceedRank(int c)
        {
            if (Rank < c) throw new ArgumentOutOfRangeException("The number of indices exceeds the dimensions of " +
                $"this tensor.");
        }
    }
}
