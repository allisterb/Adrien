﻿using System;
using System.Collections.Generic;
using System.Text;

using Sawmill;
using Xunit;

using Adrien.Notation;
using Adrien.Trees;

namespace Adrien.Tests
{
    public class ExpressionTreeTests
    {
        [Fact]
        public void TreeNodeFollowsSawmillSpec()
        {
            var A = Tensor.ThreeD("A", (2, 2, 2), "a", out Index a, out Index b, out Index c)
                        .With(out Tensor B)
                        .With(out Tensor C);
            ExpressionTree tree =  (A[a, b] * B[b, a]).ToTree();
            Children<ITreeNode> children = tree.Right.GetChildren();
            Assert.Equal(tree.Right.Left, children.First);
            Assert.Equal(tree.Right.Right, children.Second);


            OperatorNode left = tree.CreateOperatorNode(tree.Right as OperatorNode, TensorOp.NoOp);
            OperatorNode right = tree.CreateOperatorNode(tree.Right as OperatorNode, TensorOp.Sub);
            tree.Right.SetChildren(Children.Two<ITreeNode>(left, right));
            children = tree.Right.GetChildren();
            Assert.Equal(left, children.First);
            Assert.Equal(right, children.Second);
            tree.Right.SetChildren(Children.Two<ITreeNode>(right, left)).SetChildren(Children.Two<ITreeNode>(right, left));

            children = tree.Right.GetChildren();
            Assert.Equal(right, children.First);
            Assert.Equal(left, children.Second);

            
        }
    }
}
