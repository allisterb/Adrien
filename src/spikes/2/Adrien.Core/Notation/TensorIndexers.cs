﻿using System.Linq.Expressions;

namespace Adrien.Notation
{
	public partial class Tensor
	{
		  
		public TensorIndexExpression this[Index index1]
		{
			get
			{
				ThrowIfIndicesExceedRank(1);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[] 
				{this}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2]
		{
			get
			{
				ThrowIfIndicesExceedRank(2);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,] 
				{{this}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2, Index index3]
		{
			get
			{
				ThrowIfIndicesExceedRank(3);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,,] 
				{{{this}}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id), 
					Expression.Parameter(typeof(int), index3.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2, index3);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2, Index index3, Index index4]
		{
			get
			{
				ThrowIfIndicesExceedRank(4);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,,,] 
				{{{{this}}}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id), 
					Expression.Parameter(typeof(int), index3.Id), 
					Expression.Parameter(typeof(int), index4.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2, index3, index4);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2, Index index3, Index index4, Index index5]
		{
			get
			{
				ThrowIfIndicesExceedRank(5);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,,,,] 
				{{{{{this}}}}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id), 
					Expression.Parameter(typeof(int), index3.Id), 
					Expression.Parameter(typeof(int), index4.Id), 
					Expression.Parameter(typeof(int), index5.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2, index3, index4, index5);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2, Index index3, Index index4, Index index5, Index index6]
		{
			get
			{
				ThrowIfIndicesExceedRank(6);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,,,,,] 
				{{{{{{this}}}}}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id), 
					Expression.Parameter(typeof(int), index3.Id), 
					Expression.Parameter(typeof(int), index4.Id), 
					Expression.Parameter(typeof(int), index5.Id), 
					Expression.Parameter(typeof(int), index6.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2, index3, index4, index5, index6);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2, Index index3, Index index4, Index index5, Index index6, Index index7]
		{
			get
			{
				ThrowIfIndicesExceedRank(7);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,,,,,,] 
				{{{{{{{this}}}}}}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id), 
					Expression.Parameter(typeof(int), index3.Id), 
					Expression.Parameter(typeof(int), index4.Id), 
					Expression.Parameter(typeof(int), index5.Id), 
					Expression.Parameter(typeof(int), index6.Id), 
					Expression.Parameter(typeof(int), index7.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2, index3, index4, index5, index6, index7);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
		  
		public TensorIndexExpression this[Index index1, Index index2, Index index3, Index index4, Index index5, Index index6, Index index7, Index index8]
		{
			get
			{
				ThrowIfIndicesExceedRank(8);
				return new TensorIndexExpression(Expression.ArrayAccess
					(Expression.Constant(new Tensor[,,,,,,,] 
				{{{{{{{{this}}}}}}}}), 
				new Expression[] {
					Expression.Parameter(typeof(int), index1.Id), 
					Expression.Parameter(typeof(int), index2.Id), 
					Expression.Parameter(typeof(int), index3.Id), 
					Expression.Parameter(typeof(int), index4.Id), 
					Expression.Parameter(typeof(int), index5.Id), 
					Expression.Parameter(typeof(int), index6.Id), 
					Expression.Parameter(typeof(int), index7.Id), 
					Expression.Parameter(typeof(int), index8.Id)}));
			}
			set
			{
				ThrowIfAlreadyAssiged();
				IndexSet s = new IndexSet(this, index1, index2, index3, index4, index5, index6, index7, index8);
				TensorContraction tc = new TensorContraction(value, this, s);
				ContractionDefinition = (s, tc);
			}
			
		}
			}
}