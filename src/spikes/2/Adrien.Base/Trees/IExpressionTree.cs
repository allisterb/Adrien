﻿using System.Collections.Generic;

namespace Adrien.Trees
{
    public interface IExpressionTree : ITreeNode, IEqualityComparer<ITreeNode>, IEqualityComparer<ITreeValueNode>
    {
        ITreeNode Root { get; }

        IEnumerable<ITreeNode> Children { get; }

        ITreeValueNode OutputNode { get; }

        IEnumerable<ITreeValueNode> TensorNodes { get; }

        IEnumerable<ITreeValueNode> IndexSetNodes { get; }

        IEnumerable<ITreeValueNode> DefinedVariableNodes { get; }

        IEnumerable<ITreeValueNode> InputVariableNodes { get; }
    }
}