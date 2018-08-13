﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Sawmill;
using Adrien.Notation;
using Adrien.Trees;

namespace Adrien.Expressions
{
    public static class ExpressionExtensions
    {
        [DebuggerStepThrough]
        public static TensorOp ToOp(this ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Index: return TensorOp.Index;
                case ExpressionType.Multiply: return TensorOp.Mul;
                case ExpressionType.Divide: return TensorOp.Div;
                case ExpressionType.Add: return TensorOp.Add;
                case ExpressionType.Subtract: return TensorOp.Sub;
                case ExpressionType.Power: return TensorOp.Pow;
                default:
                    throw new NotSupportedException($"Cannot translate expression type: {et} to TensorOp.");
            }
        }

        [DebuggerStepThrough]
        public static TExpr As<TExpr>(this Expression expr) where TExpr : Expression
        {
            return (TExpr)expr;
        }

        public static List<T> GetConstants<T>(this Expression expr) where T : ITerm
        {
            IEnumerable<T> GetConstantsFromExpression(Expression expr0)
            {
                return expr.SelfAndDescendants()
                .OfType<ConstantExpression>()
                .Where(e => e.Type == typeof(T) || e.Type.BaseType == typeof(T))
                .Select(e => e.Value)
                .Cast<T>()
                .Distinct()
                .ToList();
            }

            IEnumerable<T> GetConstantsFromArrayExpressions(Expression expr1)
            {
                return expr1.SelfAndDescendants()
                .OfType<ConstantExpression>()
                .Where(e => e.Type.BaseType == typeof(Array)
                            && e.Type.HasElementType
                            && e.Type.GetElementType() == typeof(T))
                .Select(e => e.Value)
                .Cast<Array>()
                .Select(a => a.Flatten<T>().First())
                .Distinct()
                .ToList();
            }

            var c0 = GetConstantsFromExpression(expr);
            var c1 = GetConstantsFromArrayExpressions(expr);
            return c0.Concat(c1).ToList();
        }

        public static TReturn FlattenConstantExpressionArrayValue<TReturn>(this ConstantExpression node)
        {
            Array a = (Array)node.Value;
            return a.Flatten<TReturn>().First();
        }

        public static List<T> GetParameters<T>(this Expression expr) where T : ITerm
        {
            return expr.SelfAndDescendants()
            .OfType<ParameterExpression>()
            .Select(e => e.Name)
            .Where(s => Term.Terms.ContainsKey(s) && Term.Terms[s] is T)
            .Select(s => Term.Terms[s])
            .Cast<T>()
            .ToList();
        }

        #region Sawmill
        //!pastedoc M:Sawmill.IRewriter`1.GetChildren(`0)
        /// <summary>
        ///     Get the immediate children of the value.
        ///     <seealso cref="M:Sawmill.IRewritable`1.GetChildren" /></summary>
        /// <example>
        ///     Given a representation of the expression <c>(1+2)+3</c>,
        ///     <code>
        ///     Expr expr = new Add(
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     );
        ///     </code><see cref="M:Sawmill.IRewriter`1.GetChildren(`0)" /> returns the immediate children of the topmost node.
        ///     <code>
        ///     Expr[] expected = new[]
        ///         {
        ///             new Add(
        ///                 new Lit(1),
        ///                 new Lit(2)
        ///             ),
        ///             new Lit(3)
        ///         };
        ///     Assert.Equal(expected, rewriter.GetChildren(expr));
        ///     </code></example>
        /// <param name="value">The value</param>
        /// <returns>The immediate children of <paramref name="value" /></returns>
        /// <seealso cref="M:Sawmill.IRewriter`1.GetChildren(`0)"/>
        public static Children<System.Linq.Expressions.Expression> GetChildren(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.GetChildren(value);

        //!pastedoc M:Sawmill.IRewriter`1.SetChildren(Sawmill.Children{`0},`0)
        /// <summary>
        ///     Set the immediate children of the value.
        ///     <para>
        ///     Callers should ensure that <paramref name="newChildren" /> contains the same number of children as was returned by
        ///     <see cref="M:Sawmill.IRewriter`1.GetChildren(`0)" />.
        ///     </para><seealso cref="M:Sawmill.IRewritable`1.SetChildren(Sawmill.Children{`0})" /></summary>
        /// <example>
        ///     Given a representation of the expression <c>(1+2)+3</c>,
        ///     <code>
        ///     Expr expr = new Add(
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     );
        ///     </code><see cref="M:Sawmill.IRewriter`1.SetChildren(Sawmill.Children{`0},`0)" /> replaces the immediate children of the topmost node.
        ///     <code>
        ///     Expr expected = new Add(
        ///         new Lit(4),
        ///         new Lit(5)
        ///     );
        ///     Assert.Equal(expected, rewriter.SetChildren(Children.Two(new Lit(4), new Lit(5)), expr));
        ///     </code></example>
        /// <param name="newChildren">The new children</param>
        /// <param name="value">The old value, whose immediate children should be replaced</param>
        /// <returns>A copy of <paramref name="value" /> with updated children.</returns>
        /// <seealso cref="M:Sawmill.IRewriter`1.SetChildren(Sawmill.Children{`0},`0)"/>
        public static System.Linq.Expressions.Expression SetChildren(this System.Linq.Expressions.Expression value, Children<System.Linq.Expressions.Expression> newChildren)
            => ExpressionRewriter.Instance.SetChildren(newChildren, value);

        //!pastedoc M:Sawmill.Rewriter.DescendantsAndSelf``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Yields all of the nodes in the tree represented by <paramref name="value" />, starting at the bottom.
        ///     <seealso cref="M:Sawmill.Rewriter.SelfAndDescendants``1(Sawmill.IRewriter{``0},``0)" /></summary>
        /// <example>
        ///   <code>
        ///     Expr expr = new Add(
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     );
        ///     Expr[] expected = new[]
        ///         {
        ///             new Lit(1),
        ///             new Lit(2),
        ///             new Add(new Lit(1), new Lit(2)),
        ///             new Lit(3),
        ///             expr    
        ///         };
        ///     Assert.Equal(expected, rewriter.DescendantsAndSelf(expr));
        ///     </code>
        /// </example>
        /// <param name="value">The value to traverse</param>
        /// <returns>An enumerable containing all of the nodes in the tree represented by <paramref name="value" />, starting at the bottom.</returns>
        /// <seealso cref="M:Sawmill.Rewriter.DescendantsAndSelf``1(Sawmill.IRewriter{``0},``0)"/>
        public static IEnumerable<System.Linq.Expressions.Expression> DescendantsAndSelf(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.DescendantsAndSelf(value);

        //!pastedoc M:Sawmill.Rewriter.SelfAndDescendants``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Yields all of the nodes in the tree represented by <paramref name="value" />, starting at the top.
        ///     <seealso cref="M:Sawmill.Rewriter.DescendantsAndSelf``1(Sawmill.IRewriter{``0},``0)" /></summary>
        /// <example>
        ///   <code>
        ///     Expr expr = new Add(
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     );
        ///     Expr[] expected = new[]
        ///         {
        ///             expr,
        ///             new Add(new Lit(1), new Lit(2)),
        ///             new Lit(1),
        ///             new Lit(2),
        ///             new Lit(3),
        ///         };
        ///     Assert.Equal(expected, rewriter.SelfAndDescendants(expr));
        ///     </code>
        /// </example>
        /// <param name="value">The value to traverse</param>
        /// <returns>An enumerable containing all of the nodes in the tree represented by <paramref name="value" />, starting at the top.</returns>
        /// <seealso cref="M:Sawmill.Rewriter.SelfAndDescendants``1(Sawmill.IRewriter{``0},``0)"/>
        public static IEnumerable<System.Linq.Expressions.Expression> SelfAndDescendants(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.SelfAndDescendants(value);

        //!pastedoc M:Sawmill.Rewriter.SelfAndDescendantsBreadthFirst``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Yields all of the nodes in the tree represented by <paramref name="value" /> in a breadth-first traversal order.
        ///     </summary>
        /// <param name="value">The value to traverse</param>
        /// <returns>An enumerable containing all of the nodes in the tree represented by <paramref name="value" /> in a breadth-first traversal order.</returns>
        /// <seealso cref="M:Sawmill.Rewriter.SelfAndDescendantsBreadthFirst``1(Sawmill.IRewriter{``0},``0)"/>
        public static IEnumerable<System.Linq.Expressions.Expression> SelfAndDescendantsBreadthFirst(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.SelfAndDescendantsBreadthFirst(value);

        //!pastedoc M:Sawmill.Rewriter.ChildrenInContext``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Returns an instance of <see cref="T:Sawmill.Children`1" /> containing each immediate child of
        ///     <paramref name="value" /> paired with a function to replace the child.
        ///     This is typically useful when you need to replace a node's children one at a time,
        ///     such as during mutation testing.
        ///     
        ///     <para>
        ///     The replacement function can be seen as the "context" of the child; calling the
        ///     function with a new child "plugs the hole" in the context.
        ///     </para><seealso cref="M:Sawmill.Rewriter.SelfAndDescendantsInContext``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.DescendantsAndSelfInContext``1(Sawmill.IRewriter{``0},``0)" /></summary>
        /// <param name="value">The value to get the contexts for the immediate children</param>
        /// <seealso cref="M:Sawmill.Rewriter.ChildrenInContext``1(Sawmill.IRewriter{``0},``0)"/>
        public static Children<(System.Linq.Expressions.Expression item, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> replace)> ChildrenInContext(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.ChildrenInContext(value);

        //!pastedoc M:Sawmill.Rewriter.SelfAndDescendantsInContext``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Yields each node in the tree represented by <paramref name="value" />
        ///     paired with a function to replace the node, starting at the top.
        ///     This is typically useful when you need to replace nodes one at a time,
        ///     such as during mutation testing.
        ///     
        ///     <para>
        ///     The replacement function can be seen as the "context" of the node; calling the
        ///     function with a new node "plugs the hole" in the context.
        ///     </para><seealso cref="M:Sawmill.Rewriter.SelfAndDescendants``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.ChildrenInContext``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.DescendantsAndSelfInContext``1(Sawmill.IRewriter{``0},``0)" /></summary>
        /// <param name="value">The value to get the contexts for the descendants</param>
        /// <seealso cref="M:Sawmill.Rewriter.SelfAndDescendantsInContext``1(Sawmill.IRewriter{``0},``0)"/>
        public static IEnumerable<(System.Linq.Expressions.Expression item, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> replace)> SelfAndDescendantsInContext(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.SelfAndDescendantsInContext(value);

        //!pastedoc M:Sawmill.Rewriter.DescendantsAndSelfInContext``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Yields each node in the tree represented by <paramref name="value" />
        ///     paired with a function to replace the node, starting at the bottom.
        ///     This is typically useful when you need to replace nodes one at a time,
        ///     such as during mutation testing.
        ///     
        ///     <para>
        ///     The replacement function can be seen as the "context" of the node; calling the
        ///     function with a new node "plugs the hole" in the context.
        ///     </para><seealso cref="M:Sawmill.Rewriter.DescendantsAndSelf``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.ChildrenInContext``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.SelfAndDescendantsInContext``1(Sawmill.IRewriter{``0},``0)" /></summary>
        /// <param name="value">The value to get the contexts for the descendants</param>
        /// <seealso cref="M:Sawmill.Rewriter.DescendantsAndSelfInContext``1(Sawmill.IRewriter{``0},``0)"/>
        public static IEnumerable<(System.Linq.Expressions.Expression item, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> replace)> DescendantsAndSelfInContext(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.DescendantsAndSelfInContext(value);

        //!pastedoc M:Sawmill.Rewriter.SelfAndDescendantsInContextBreadthFirst``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Yields each node in the tree represented by <paramref name="value" />
        ///     paired with a function to replace the node, in a breadth-first traversal order.
        ///     This is typically useful when you need to replace nodes one at a time,
        ///     such as during mutation testing.
        ///     
        ///     <para>
        ///     The replacement function can be seen as the "context" of the node; calling the
        ///     function with a new node "plugs the hole" in the context.
        ///     </para><seealso cref="M:Sawmill.Rewriter.SelfAndDescendants``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.ChildrenInContext``1(Sawmill.IRewriter{``0},``0)" /><seealso cref="M:Sawmill.Rewriter.DescendantsAndSelfInContext``1(Sawmill.IRewriter{``0},``0)" /></summary>
        /// <param name="value">The value to get the contexts for the descendants</param>
        /// <seealso cref="M:Sawmill.Rewriter.SelfAndDescendantsInContextBreadthFirst``1(Sawmill.IRewriter{``0},``0)"/>
        public static IEnumerable<(System.Linq.Expressions.Expression item, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> replace)> SelfAndDescendantsInContextBreadthFirst(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.SelfAndDescendantsInContextBreadthFirst(value);

        //!pastedoc M:Sawmill.Rewriter.DescendantAt``1(Sawmill.IRewriter{``0},System.Collections.Generic.IEnumerable{Sawmill.Direction},``0)
        /// <summary>
        ///     Returns the descendant at a particular location in <paramref name="value" /></summary>
        /// <param name="value">The rewritable tree type</param>
        /// <param name="path">The route to take to find the descendant</param>
        /// <exception cref="T:System.InvalidOperationException">
        ///     Thrown if <paramref name="path" /> leads off the edge of the tree
        ///     </exception>
        /// <returns>The descendant found by following the directions in <paramref name="path" /></returns>
        /// <seealso cref="M:Sawmill.Rewriter.DescendantAt``1(Sawmill.IRewriter{``0},System.Collections.Generic.IEnumerable{Sawmill.Direction},``0)"/>
        public static System.Linq.Expressions.Expression DescendantAt(this System.Linq.Expressions.Expression value, IEnumerable<Direction> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return ExpressionRewriter.Instance.DescendantAt(path, value);
        }

        //!pastedoc M:Sawmill.Rewriter.ReplaceDescendantAt``1(Sawmill.IRewriter{``0},System.Collections.Generic.IEnumerable{Sawmill.Direction},``0,``0)
        /// <summary>
        ///     Replaces the descendant at a particular location in <paramref name="value" /></summary>
        /// <param name="value">The rewritable tree type</param>
        /// <param name="path">The route to take to find the descendant</param>
        /// <param name="newDescendant">The replacement descendant</param>
        /// <exception cref="T:System.InvalidOperationException">
        ///     Thrown if <paramref name="path" /> leads off the edge of the tree
        ///     </exception>
        /// <returns>
        ///     A copy of <paramref name="value" /> with <paramref name="newDescendant" /> placed at the location indicated by <paramref name="path" /></returns>
        /// <seealso cref="M:Sawmill.Rewriter.ReplaceDescendantAt``1(Sawmill.IRewriter{``0},System.Collections.Generic.IEnumerable{Sawmill.Direction},``0,``0)"/>
        public static System.Linq.Expressions.Expression ReplaceDescendantAt<T>(this System.Linq.Expressions.Expression value, IEnumerable<Direction> path, System.Linq.Expressions.Expression newDescendant)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return ExpressionRewriter.Instance.ReplaceDescendantAt(path, newDescendant, value);
        }

        //!pastedoc M:Sawmill.Rewriter.RewriteDescendantAt``1(Sawmill.IRewriter{``0},System.Collections.Generic.IEnumerable{Sawmill.Direction},System.Func{``0,``0},``0)
        /// <summary>
        ///     Apply a function at a particular location in <paramref name="value" /></summary>
        /// <param name="value">The rewritable tree type</param>
        /// <param name="path">The route to take to find the descendant</param>
        /// <param name="transformer">A function to calculate a replacement for the descendant</param>
        /// <exception cref="T:System.InvalidOperationException">
        ///     Thrown if <paramref name="path" /> leads off the edge of the tree
        ///     </exception>
        /// <returns>
        ///     A copy of <paramref name="value" /> with the result of <paramref name="transformer" /> placed at the location indicated by <paramref name="path" /></returns>
        /// <seealso cref="M:Sawmill.Rewriter.RewriteDescendantAt``1(Sawmill.IRewriter{``0},System.Collections.Generic.IEnumerable{Sawmill.Direction},System.Func{``0,``0},``0)"/>
        public static System.Linq.Expressions.Expression RewriteDescendantAt<T>(this System.Linq.Expressions.Expression value, IEnumerable<Direction> path, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> transformer)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            return ExpressionRewriter.Instance.RewriteDescendantAt(path, transformer, value);
        }

        //!pastedoc M:Sawmill.Rewriter.Cursor``1(Sawmill.IRewriter{``0},``0)
        /// <summary>
        ///     Create a <see cref="M:Sawmill.Rewriter.Cursor``1(Sawmill.IRewriter{``0},``0)" /> focused on the root node of <paramref name="value" />.
        ///     </summary>
        /// <param name="value">The root node on which the newly created <see cref="M:Sawmill.Rewriter.Cursor``1(Sawmill.IRewriter{``0},``0)" /> should be focused</param>
        /// <returns>A <see cref="M:Sawmill.Rewriter.Cursor``1(Sawmill.IRewriter{``0},``0)" /> focused on the root node of <paramref name="value" /></returns>
        /// <seealso cref="M:Sawmill.Rewriter.Cursor``1(Sawmill.IRewriter{``0},``0)"/>
        public static Cursor<System.Linq.Expressions.Expression> Cursor(this System.Linq.Expressions.Expression value)
            => ExpressionRewriter.Instance.Cursor(value);

        //!pastedoc M:Sawmill.Rewriter.Fold``2(Sawmill.IRewriter{``0},System.Func{``0,Sawmill.Children{``1},``1},``0)
        /// <summary>
        ///     Flattens all the nodes in the tree represented by <paramref name="value" /> into a single result,
        ///     using an aggregation function to combine each node with the results of folding its children.
        ///     </summary>
        /// <param name="func">The aggregation function</param>
        /// <param name="value">The value to fold</param>
        /// <returns>The result of aggregating the tree represented by <paramref name="value" />.</returns>
        /// <seealso cref="M:Sawmill.Rewriter.Fold``2(Sawmill.IRewriter{``0},System.Func{``0,Sawmill.Children{``1},``1},``0)"/>
        public static T Fold<T>(this System.Linq.Expressions.Expression value, Func<System.Linq.Expressions.Expression, Children<T>, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return ExpressionRewriter.Instance.Fold(func, value);
        }

        //!pastedoc M:Sawmill.Rewriter.ZipFold``2(Sawmill.IRewriter{``0},System.Func{``0[],System.Collections.Generic.IEnumerable{``1},``1},``0[])
        /// <summary>
        ///     Flatten all of the nodes in the trees represented by <paramref name="values" />
        ///     into a single value at the same time, using an aggregation function to combine
        ///     nodes with the results of aggregating their children.
        ///     The trees are iterated in lock-step, much like an n-ary <see cref="M:System.Linq.Enumerable.Zip``3(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEnumerable{``1},System.Func{``0,``1,``2})" />.
        ///     
        ///     When trees are not the same size, the larger ones are
        ///     truncated both horizontally and vertically.
        ///     That is, if a pair of nodes have a different number of children,
        ///     the rightmost children of the larger of the two nodes are discarded.
        ///     </summary>
        /// <example>
        ///     Here's an example of using <see cref="M:Sawmill.Rewriter.ZipFold``2(Sawmill.IRewriter{``0},System.Func{``0[],System.Collections.Generic.IEnumerable{``1},``1},``0[])" /> to test if two trees are syntactically equal.
        ///     <code>
        ///     static bool Equals(this Expr left, Expr right)
        ///         =&gt; left.ZipFold&lt;Expr, bool&gt;(
        ///             right,
        ///             (xs, results) =&gt;
        ///             {
        ///                 switch (xs[0])
        ///                 {
        ///                     case Add a1 when xs[1] is Add a2:
        ///                         return results.All(x =&gt; x);
        ///                     case Lit l1 when xs[1] is Lit l2:
        ///                         return l1.Value == l2.Value;
        ///                     default:
        ///                         return false;
        ///                 }
        ///             }
        ///         );
        ///     </code></example>
        /// <param name="func">The aggregation function</param>
        /// <param name="values">The trees to fold</param>
        /// <returns>The result of aggregating the two trees</returns>
        /// <seealso cref="M:Sawmill.Rewriter.ZipFold``2(Sawmill.IRewriter{``0},System.Func{``0[],System.Collections.Generic.IEnumerable{``1},``1},``0[])"/>
        public static U ZipFold<U>(this System.Linq.Expressions.Expression[] values, Func<System.Linq.Expressions.Expression[], IEnumerable<U>, U> func)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return ExpressionRewriter.Instance.ZipFold(func, values);
        }

#pragma warning disable CS1734, CS1572, CS1573
        //!pastedoc M:Sawmill.Rewriter.ZipFold``2(Sawmill.IRewriter{``0},System.Func{``0[],System.Collections.Generic.IEnumerable{``1},``1},``0[])
        /// <summary>
        ///     Flatten all of the nodes in the trees represented by <paramref name="values" />
        ///     into a single value at the same time, using an aggregation function to combine
        ///     nodes with the results of aggregating their children.
        ///     The trees are iterated in lock-step, much like an n-ary <see cref="M:System.Linq.Enumerable.Zip``3(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEnumerable{``1},System.Func{``0,``1,``2})" />.
        ///     
        ///     When trees are not the same size, the larger ones are
        ///     truncated both horizontally and vertically.
        ///     That is, if a pair of nodes have a different number of children,
        ///     the rightmost children of the larger of the two nodes are discarded.
        ///     </summary>
        /// <example>
        ///     Here's an example of using <see cref="M:Sawmill.Rewriter.ZipFold``2(Sawmill.IRewriter{``0},System.Func{``0[],System.Collections.Generic.IEnumerable{``1},``1},``0[])" /> to test if two trees are syntactically equal.
        ///     <code>
        ///     static bool Equals(this Expr left, Expr right)
        ///         =&gt; left.ZipFold&lt;Expr, bool&gt;(
        ///             right,
        ///             (xs, results) =&gt;
        ///             {
        ///                 switch (xs[0])
        ///                 {
        ///                     case Add a1 when xs[1] is Add a2:
        ///                         return results.All(x =&gt; x);
        ///                     case Lit l1 when xs[1] is Lit l2:
        ///                         return l1.Value == l2.Value;
        ///                     default:
        ///                         return false;
        ///                 }
        ///             }
        ///         );
        ///     </code></example>
        /// <param name="func">The aggregation function</param>
        /// <param name="values">The trees to fold</param>
        /// <returns>The result of aggregating the two trees</returns>
        /// <seealso cref="M:Sawmill.Rewriter.ZipFold``2(Sawmill.IRewriter{``0},System.Func{``0[],System.Collections.Generic.IEnumerable{``1},``1},``0[])"/>
        public static U ZipFold<U>(this System.Linq.Expressions.Expression value1, System.Linq.Expressions.Expression value2, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression, IEnumerable<U>, U> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return ExpressionRewriter.Instance.ZipFold<System.Linq.Expressions.Expression, U>((xs, cs) => func(xs[0], xs[1], cs), new[] { value1, value2 });
        }
#pragma warning restore CS1734, CS1572, CS1573

        //!pastedoc M:Sawmill.Rewriter.Rewrite``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)
        /// <summary>
        ///     Rebuild a tree by applying a transformation function to every node from bottom to top.
        ///     </summary>
        /// <example>
        ///     Given a representation of the expression <c>(1+2)+3</c>,
        ///     <code>
        ///     Expr expr = new Add(
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     );
        ///     </code><see cref="M:Sawmill.Rewriter.Rewrite``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)" /> replaces the leaves of the tree with the result of calling <paramref name="transformer" />,
        ///     then replaces their parents with the result of calling <paramref name="transformer" />, and so on.
        ///     By the end, <see cref="M:Sawmill.Rewriter.Rewrite``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)" /> has traversed the whole tree.
        ///     <code>
        ///     Expr expected = transformer(new Add(
        ///         transformer(new Add(
        ///             transformer(new Lit(1)),
        ///             transformer(new Lit(2))
        ///         )),
        ///         transformer(new Lit(3))
        ///     ));
        ///     Assert.Equal(expected, rewriter.RewriteChildren(transformer, expr));
        ///     </code></example>
        /// <param name="transformer">The transformation function to apply to every node in the tree</param>
        /// <param name="value">The value to rewrite</param>
        /// <returns>
        ///     The result of applying <paramref name="transformer" /> to every node in the tree represented by <paramref name="value" />.
        ///     </returns>
        /// <seealso cref="M:Sawmill.Rewriter.Rewrite``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)"/>
        public static System.Linq.Expressions.Expression Rewrite(this System.Linq.Expressions.Expression value, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> transformer)
        {
            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }
            return ExpressionRewriter.Instance.Rewrite(transformer, value);
        }

        //!pastedoc M:Sawmill.IRewriter`1.RewriteChildren(System.Func{`0,`0},`0)
        /// <summary>
        ///     Update the immediate children of the value by applying a transformation function to each one.
        ///     <para>
        ///     Implementations of <see cref="T:Sawmill.IRewriter`1" /> can use <see cref="M:Sawmill.Rewriter.DefaultRewriteChildren``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)" />,
        ///     or you can write your own.
        ///     </para><para>
        ///     NB: A hand-written implementation will not usually be faster
        ///     than <see cref="M:Sawmill.Rewriter.DefaultRewriteChildren``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)" />.
        ///     If your type has a fixed number of children, and that number is greater than two,
        ///     you may see some performance improvements from implementing this method yourself.
        ///     Be careful not to rebuild <paramref name="value" /> if none of the children have changed.
        ///     </para><seealso cref="M:Sawmill.IRewritable`1.RewriteChildren(System.Func{`0,`0})" /></summary>
        /// <example>
        ///     Given a representation of the expression <c>(1+2)+3</c>,
        ///     <code>
        ///     Expr expr = new Add(
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     );
        ///     </code><see cref="M:Sawmill.IRewriter`1.RewriteChildren(System.Func{`0,`0},`0)" /> only affects the immediate children of the topmost node.
        ///     <code>
        ///     Expr expected = new Add(
        ///         transformer(new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         )),
        ///         transformer(new Lit(3))
        ///     );
        ///     Assert.Equal(expected, rewriter.RewriteChildren(transformer, expr));
        ///     </code></example>
        /// <param name="transformer">A transformation function to apply to each of <paramref name="value" />'s immediate children.</param>
        /// <param name="value">The old value, whose immediate children should be transformed by <paramref name="transformer" />.</param>
        /// <returns>A copy of <paramref name="value" /> with updated children.</returns>
        /// <seealso cref="M:Sawmill.IRewriter`1.RewriteChildren(System.Func{`0,`0},`0)"/>
        public static System.Linq.Expressions.Expression RewriteChildren(this System.Linq.Expressions.Expression value, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> transformer)
        {
            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }
            return ExpressionRewriter.Instance.RewriteChildren(transformer, value);
        }

        //!pastedoc M:Sawmill.Rewriter.RewriteIter``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)
        /// <summary>
        ///     Rebuild a tree by repeatedly applying a transformation function to every node in the tree,
        ///     until a fixed point is reached. <paramref name="transformer" /> should always eventually return
        ///     its argument unchanged, or this method will loop.
        ///     That is, <c>x.RewriteIter(transformer).SelfAndDescendants().All(x =&gt; transformer(x) == x)</c>.
        ///     <para>
        ///     This is typically useful when you want to put your tree into a normal form
        ///     by applying a collection of rewrite rules until none of them can fire any more.
        ///     </para></summary>
        /// <param name="transformer">
        ///     A transformation function to apply to every node in <paramref name="value" /> repeatedly.
        ///     </param>
        /// <param name="value">The value to rewrite</param>
        /// <returns>
        ///     The result of applying <paramref name="transformer" /> to every node in the tree
        ///     represented by <paramref name="value" /> repeatedly until a fixed point is reached.
        ///     </returns>
        /// <seealso cref="M:Sawmill.Rewriter.RewriteIter``1(Sawmill.IRewriter{``0},System.Func{``0,``0},``0)"/>
        public static System.Linq.Expressions.Expression RewriteIter(this System.Linq.Expressions.Expression value, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> transformer)
        {
            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }
            return ExpressionRewriter.Instance.RewriteIter(transformer, value);
        }

        #endregion

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

        [DebuggerStepThrough]
        public static TensorOp MethodCallToTensorOp(this MethodCallExpression expr)
        {
            switch (expr.Method.Name)
            {
                case "Op_Sum": return TensorOp.Sum;
                case "Op_Square": return TensorOp.Square;
                case "Op_Sqrt": return TensorOp.Sqrt;
                default: throw new NotImplementedException();
            }
        }

        [DebuggerStepThrough]
        public static void ThrowIfNotType<T>(this Expression expr)
        {
            if (expr.Type != typeof(T))
            {
                throw new ArgumentException($"This expression does not have type {typeof(T).Name}.");
            }
        }

        [DebuggerStepThrough]
        public static bool IsUndefinedTensor(this Expression expr) => 
            (expr is ConstantExpression ce) && ce.Type == typeof(Tensor);
    }
}