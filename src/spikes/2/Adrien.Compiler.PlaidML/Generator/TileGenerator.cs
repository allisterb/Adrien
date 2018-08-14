﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Adrien.Generator;
using Adrien.Trees;

namespace Adrien.Compiler.PlaidML.Generator
{
    public class TileGenerator : LanguageGenerator<TensorOp, TileWriter>
    {
        public override List<TensorOp> NestedBinaryOperators { get; } = new List<TensorOp>()
        {
            TensorOp.Mul, TensorOp.Add, TensorOp.Sub, TensorOp.Div, TensorOp.Square
        };

        public List<ITermShape> InputShapes => Tree.InputVariableNodes.Select(t => t.ValueAs<ITermShape>()).ToList();
            
        public Dictionary<ITreeValueNode, string> TensorDimensionVariables { get; protected set; }

        public string FunctionText { get; protected set; }

        public TileGenerator(IExpressionTree tree) : base(tree)
        {
            Context = new TileGeneratorContext(tree);
            Writer = new TileWriter();
            GetDimensionVariableNames();
            this.VisitTree();
        }

        
        public override void AfterVisitTree()
        {
            base.AfterVisitTree();
            WriteFunctionPrologue();
        }

        public override void VisitInternal(ITreeOperatorNode<TensorOp> on)
        {
            switch(on.Op)
            {
                case TensorOp.ElementWiseAssign:
                    string lhs = "", rhs = "";
                    using (Context.Internal(Writer.GetOperatorTemplate(on)))
                    {                        
                        base.Visit(on.Right);
                        rhs = (string)Context.Pop();
                           
                        base.Visit(on.Left);
                        lhs = (string)Context.Pop();
                        
                    }
                    Writer.VariableDefinitions.Enqueue(string.Format("{0} = {1};", lhs, rhs) + Environment.NewLine);
                    Context.Push(Writer.WriteOperator(on.Op, lhs));
                    return;

                case TensorOp.Index:
                    if (Context.Count <= 1 && ContextIsOpStart(TensorOp.Assign)) //Start of an assignment op
                    {
                        base.VisitInternal(on);
                        return;
                    }

                    using (Context.Internal(Writer.GetOperatorTemplate(on)))
                    {
                        base.Visit(on.Right);
                        rhs = (string)Context.Pop();
                        base.Visit(on.Left);
                        lhs = (string)Context.Pop();
                    }
                    ITreeValueNode tensor;
                    if (on.Left is ITreeValueNode)
                    {
                        tensor = on.Left as ITreeValueNode;
                        if (tensor.NodeType != ValueNodeType.TENSOR)
                        {
                            throw new TileGeneratorException(this, "The LHS of the Index operation is not a tensor.");
                        }
                    }
                    else if (on.Left is ITreeOperatorNode<TensorOp> && 
                        (on.Left as ITreeOperatorNode<TensorOp>).Op == TensorOp.ElementWiseAssign)
                    {
                        tensor = (on.Left as ITreeOperatorNode<TensorOp>).Left as ITreeValueNode;
                        if (tensor.NodeType != ValueNodeType.TENSOR)
                        {
                            throw new TileGeneratorException(this, "The LHS of the Index operation is not a tensor.");
                        }
                    }
                    else
                    {
                        throw new TileGeneratorException(this, "Could not determine the LHS of the Index operation");
                    }
                    if (Tree.InputVariableNodes.Contains(tensor))
                    {
                        var dim = TensorDimensionVariables[tensor];
                        Context.Push(Writer.WriteOperator(on.Op, lhs, rhs + ":" + dim));
                    }
                    else
                    {
                        Context.Push(Writer.WriteOperator(on.Op, lhs, rhs));
                    }
                    return;

                default:
                    base.VisitInternal(on);
                    return;
            }
        }

        protected void GetDimensionVariableNames()
        {
            TensorDimensionVariables = new Dictionary<ITreeValueNode, string>(Tree.InputVariableNodes.Count());
            foreach(ITreeValueNode v in Tree.InputVariableNodes)
            {
                string name = v.Label + "N";
                int n = 0;
                while (TensorDimensionVariables.Values.Contains(name))
                {
                    name = name + (++n).ToString();
                }
                TensorDimensionVariables.Add(v, name.ToUpper());
            }
        }

        protected void WriteFunctionPrologue()
        {
            StringBuilder prologue = new StringBuilder("function(");
            foreach(ITreeValueNode tensor in Tree.InputVariableNodes)
            {
                if (!Tree.IndexSetNodes.Any(set => set.Parent.Label == tensor.Label))
                {
                    prologue.AppendFormat("{0}, ", tensor.Label.ToUpper());
                }
            }
            prologue.Remove(prologue.Length - 2, 2);
            prologue.Append(") -> ");
            if (!Tree.IndexSetNodes.Any(set => set.Parent.Label == Tree.OutputNode.Label))
            {
                prologue.AppendFormat("({0})", Tree.OutputNode.Label.ToUpper());
            }
            prologue.Append(" { ");
            FunctionText = prologue + this.Text + "}";
        }
    }
}

