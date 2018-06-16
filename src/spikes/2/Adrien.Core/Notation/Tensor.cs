﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
namespace Adrien.Notation
{
    public class Tensor<T> : Term where T : unmanaged
    {
        #region Constructors
        public Tensor(string name, params int [] dim) : base(name)
        {
            Dimensions = dim;
        }

        public Tensor(string name, out IndexSet I, params int[] dim) : this(name, dim)
        {
            I = new IndexSet(dim.Length);
        }

        public Tensor(string name, string indexNameBase, out IndexSet I, params int[] dim) : this(name, dim)
        {
            I = new IndexSet(dim.Length, indexNameBase);
        }

        #endregion

        #region Overriden members
        internal override Expression LinqExpression => Expression.Constant(this);
        internal override Name DefaultNameBase { get; } = "T";
        #endregion

        #region Properties
        public int[] Dimensions { get; protected set; }

        public int Rank => Dimensions.Length;

        public (IndexSet AssignmentIndexSet, TensorExpression<T> AssignmentExpression) Assignment { get; protected set; }
        #endregion

        #region Operators

        #region Indexers
        public static implicit operator TensorExpression<T>(Tensor<T> t)
        {
            return new TensorExpression<T>(t.LinqExpression);
        }

        public TensorExpression<T> this[IndexSet indexSet]
        {
            get
            {
                if (Rank <= indexSet.DimensionCount)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[] { }), null,
                        new Expression[] { Expression.Parameter(typeof(int), indexSet.Name) }));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (indexSet, value);
            }
        }

        public TensorExpression<T> this[Index index1]
        {
            get
            {
                if (Rank > 0)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name) }));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1), value);
            }
        }

        public TensorExpression<T> this[Index index1, Index index2]
        {
            get
            {
                if (Rank > 1)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[,] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name), Expression.Parameter(typeof(int), index2.Name) }));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1, index2), value);
            }
        }

        public TensorExpression<T> this[Index index1, Index index2, Index index3]
        {
            get
            {
                if (Rank > 2)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[,,] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name),
                        Expression.Parameter(typeof(int), index2.Name), Expression.Parameter(typeof(int), index3.Name) }));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1, index2, index3), value);
            }
        }

        public TensorExpression<T> this[Index index1, Index index2, Index index3, Index index4]
        {
            get
            {
                if (Rank > 3)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[,,,] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name),
                        Expression.Parameter(typeof(int), index2.Name), Expression.Parameter(typeof(int), index3.Name),
                        Expression.Parameter(typeof(int), index4.Name) }));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1, index2, index3, index4), value);
            }
        }

        public TensorExpression<T> this[Index index1, Index index2, Index index3, Index index4, Index index5]
        {
            get
            {
                if (Rank > 4)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[,,,,] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name),
                        Expression.Parameter(typeof(int), index2.Name), Expression.Parameter(typeof(int), index3.Name),
                        Expression.Parameter(typeof(int), index4.Name), Expression.Parameter(typeof(int), index5.Name) }));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1, index2, index3, index4, index5), value);
            }
        }

        public TensorExpression<T> this[Index index1, Index index2, Index index3, Index index4, Index index5, Index index6]
        {
            get
            {
                if (Rank > 5)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[,,,,,] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name),
                        Expression.Parameter(typeof(int), index2.Name), Expression.Parameter(typeof(int), index3.Name),
                        Expression.Parameter(typeof(int), index4.Name), Expression.Parameter(typeof(int), index5.Name),
                        Expression.Parameter(typeof(int), index6.Name)}));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1, index2, index3, index4, index5), value);
            }
        }

        public TensorExpression<T> this[Index index1, Index index2, Index index3, Index index4, Index index5, Index index6, Index index7]
        {
            get
            {
                if (Rank > 6)
                {
                    return new TensorExpression<T>(Expression.MakeIndex(Expression.Constant(new Tensor<T>[,,,,,,] { }), null, new Expression[] {
                        Expression.Parameter(typeof(int), index1.Name),
                        Expression.Parameter(typeof(int), index2.Name), Expression.Parameter(typeof(int), index3.Name),
                        Expression.Parameter(typeof(int), index4.Name), Expression.Parameter(typeof(int), index5.Name),
                        Expression.Parameter(typeof(int), index6.Name), Expression.Parameter(typeof(int), index7.Name)}));
                }
                else throw new ArgumentOutOfRangeException($"This tensor has rank {Rank}.");
            }
            set
            {
                ThrowIfAlreadyAssiged();
                Assignment = (new IndexSet(index1, index2, index3, index4, index5, index6, index7), value);
            }
        }
        #endregion

        #endregion

        #region Methods
        internal void ThrowIfAlreadyAssiged()
        {
            if (Assignment.AssignmentIndexSet != null)
            {
                throw new InvalidOperationException("This tensor variable has an existing assigment. You can only assign to a tensor variable once.");
            }
        }

        internal void ThrowIfIndicesExceedRank(int c)
        {
            if (Rank < c) throw new ArgumentOutOfRangeException("The number of indices exceeds the dimensions of this tensor.");
        }

        public static Tensor<T> OneD(string name) => new Tensor<T>(name, new int[1]);
        public static Tensor<T> OneD(string name, string indexName, out Index index)
        {
            index = new IndexSet(1, indexName);
            return new Tensor<T>(name, new int[1]);
        }

        public static Tensor<T> TwoD(string name) => new Tensor<T>(name, new int[2]);
        public static Tensor<T> TwoD(string name, out IndexSet I) => new Tensor<T>(name, out I, new int[2]);
        public static Tensor<T> TwoD(string name, string indexNameBase, out Index index1, out Index index2)
        {
            (index1, index2)  = new IndexSet(2, indexNameBase);
            return new Tensor<T>(name, new int[2]);
        }

        public static Tensor<T> ThreeD(string name) => new Tensor<T>(name, new int[3]);
        public static Tensor<T> ThreeD(string name, out IndexSet I) => new Tensor<T>(name, out I, new int[3]);
        public static Tensor<T> ThreeD(string name, string indexNameBase, out Index index1, out Index index2, out Index index3)
        {
            (index1, index2, index3) = new IndexSet(3, indexNameBase);
            return new Tensor<T>(name, new int[3]);
        }


        public static Tensor<T> FourD(string name) => new Tensor<T>(name, new int[4]);
        public static Tensor<T> FourD(string name, out IndexSet I) => new Tensor<T>(name, out I, new int[4]);
        public static Tensor<T> FourD(string name, string indexNameBase, out Index index1, out Index index2, out Index index3, out Index index4)
        {
            (index1, index2, index3, index4) = new IndexSet(4, indexNameBase);
            return new Tensor<T>(name, new int[4]);
        }

        public static Tensor<T> FiveD(string name) => new Tensor<T>(name, new int[5]);
        public static Tensor<T> FiveD(string name, out IndexSet I) => new Tensor<T>(name, out I, new int[5]);
        public static Tensor<T> FiveD(string name, string indexNameBase, out Index index1, out Index index2, out Index index3, out Index index4, out Index index5)
        {
            (index1, index2, index3, index4, index5) = new IndexSet(5, indexNameBase);
            return new Tensor<T>(name, new int[5]);
        }

        public static Tensor<T> SixD(string name) => new Tensor<T>(name, new int[6]);
        public static Tensor<T> SixD(string name, out IndexSet I) => new Tensor<T>(name, out I, new int[6]);
        public static Tensor<T> SixD(string name, string indexNameBase, out Index index1, out Index index2, out Index index3, out Index index4, out Index index5, out Index index6)
        {
            (index1, index2, index3, index4, index5, index6) = new IndexSet(6, indexNameBase);
            return new Tensor<T>(name, new int[6]);
        }

        public static Tensor<T> SevenD(string name) => new Tensor<T>(name, new int[7]);
        public static Tensor<T> SevenD(string name, out IndexSet I) => new Tensor<T>(name, out I, new int[7]);
        public static Tensor<T> SevenD(string name, string indexNameBase, out Index index1, out Index index2, out Index index3, out Index index4, out Index index5, out Index index6, out Index index7)
        {
            (index1, index2, index3, index4, index5, index6, index7) = new IndexSet(7, indexNameBase);
            return new Tensor<T>(name, new int[7]);
        }
        #endregion
    }
}
