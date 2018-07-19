﻿using System;
using System.Collections.Generic;
using System.Text;

using N = Adrien.Notation;
using Adrien.Generator;
using Adrien.Trees;

namespace Adrien.Compiler.PlaidML.Generator
{
    public class TileWriter : LanguageWriter<TensorOp> 
    {
        protected override Dictionary<TensorOp, string> OperatorTemplate { get; } = new Dictionary<TensorOp, string>
        {
            { TensorOp.Assign, "{0} = {1};" },
            { TensorOp.Index, "{0}[{1}]" },
            { TensorOp.Add, "{0} + {1}" },
            { TensorOp.Mul, "{0} * {1}" },
            { TensorOp.Sub, "{0} - {1}" },
            { TensorOp.Div, "{0} / {1}" }
        };

        public override string WriteValueText(ITreeValueNode vn)
        {
            switch (vn.NodeType)
            {
                case ValueNodeType.TENSOR: return base.WriteValueText(vn).ToUpper();
                case ValueNodeType.INDEXSET: return base.WriteValueText(vn).ToLower();
                default: return base.WriteValueText(vn);
            }

        }

    }
}
