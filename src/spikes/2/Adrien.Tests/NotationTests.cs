﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using System.Text;

using Xunit;

using Adrien.Notation;

namespace Adrien.Tests
{
    public class NotationTests
    {
        [Fact]
        public void CanConstructTensorNotation()
        {
            Tensor A = Tensor.TwoD("A", (7,7), "a", out Index a, out Index b);
            Assert.Equal("A", A.Name);
            Assert.Equal(2, A.Rank);
            Assert.Equal("a", a.Name);
            Assert.Equal("b", b.Name);
            var A1 = A[a, b];
            Assert.IsType<IndexExpression>((Expression) A1);

            var B = Tensor.ThreeD("B", (5, 6, 7), "i", out var ijk);
            var (i, j, k) = ijk;
            Assert.Equal("i", i.Name);
            Assert.Equal("j", j.Name);
            Assert.Equal("k", k.Name);
            Assert.Equal(1, j.Order);

            A[0] = B;
            Assert.IsType<IndexExpression>((Expression) B[i]);
            Assert.IsType<IndexExpression>((Expression) B[ijk]);
            B[ijk] = A[i];
            Assert.True(B.IsAssigned);
            Assert.True(B.IndexedAssignment.IndexSet[1].Equals(j));
        }

        [Fact]
        public void CanUseFluentTensorConstruction()
        {
            Tensor D = Tensor.TwoD("D", (11, 12))
                .With(out Tensor E)
                .With(out Tensor F, 4, 3);

            Assert.Equal("D", D.Name);
            Assert.Equal(12, D.Dimensions[1]);

            Assert.Equal("E", E.Name);
            Assert.Equal(12, E.Dimensions[1]);

            Assert.Equal("F", F.Name);
            Assert.Equal(4, F.Dimensions[0]);
            Assert.Equal(3, F.Dimensions[1]);
            Assert.Equal(2, F.Rank);
            Assert.Throws<ArgumentException>(() => F.With(out Tensor G, 3, 2, 1));

        }

        [Fact]
        public void CanUseTupleTensorConstructors()
        {
            var (H, I, J) = Tensor.ThreeD("H", (2, 2, 2)).Three();
            Assert.Equal("H", H.Label);
            Assert.Equal("I", I.Label);
            Assert.Equal("J", J.Label);
            Assert.Equal(3, J.Rank);
            var (M1, M2, M3) = Tensor.ThreeD((2, 2, 2)).Three("M1", "M2", "M3");
            Assert.Equal("M1", M1.Name);
            Assert.Equal(3, M1.Rank);
            Assert.Equal("M3", M3.Name);
            Assert.Equal(8, M3.NumberofElements);
        }

        [Fact]
        public void CanGenerateTermNames()
        {
            var B = Tensor.FiveD(tn.B, (4, 5, 6, 7, 8));
            Assert.Equal("B", B.Name);
            var I = new IndexSet(B, "m0", B.Dimensions);
            Assert.Equal("m0", I[0].Name);
            var J = new IndexSet(B, "i0", B.Dimensions);
            Assert.Equal("i0", J[0].Name);
            var (m0, m1, m2, m3, m4) = I;
            Assert.Equal("m4", m4.Name);
        }

        [Fact]
        public void CanAssignTensorExpression()
        {
            var A = Tensor.TwoD("A", (4,3), "a", out Index a, out Index b);
            var B = Tensor.TwoD("B", (6,7));
            var C = Tensor.TwoD("C", (8,9));
            C[a,b] = B[a,b] * C[b,a];
            Assert.True(C.IsAssigned);
        }

        [Fact]
        public void CanUseIndexExpressions()
        {
            
            var (A, B, C) = Tensor.TwoD((4, 3), out Index i, out Index j).Three();
            var (M, N, O) = Tensor.ThreeD("M", (7, 9, 6), "m", out Index m, out Index n, out Index o).Three();
            A[i + 5, j + 7] = B[i, j] + C[i, j];
            Assert.True(A.IsAssigned);
            Assert.True(A.IndexedAssignment.IndexSet[0].Type == IndexType.Expression);
            M[6, n - 4] = M[7] + N[n];
            Assert.True(M.IsAssigned);
        }

        [Fact]
        public void CanConstructVectorExpression()
        {
            var (V0, V1) = new Vector(5).Two();
            Assert.Equal("V0", V0.Name);
            Assert.Equal("V1", V1.Name);
            var (c0, c1, c2) = new Scalar().Three();

            V1[V0] = c0 * V0 + c1;
            Assert.True(V1.IsAssigned);
            Assert.Equal(3, V1.ElementwiseAssignment.Expression.Tensors.Count);

            var (x, y) = new Vector("x", 2).Two();
            Assert.Equal("x", x.Name);
            Assert.Equal("y", y.Name);
            var (a, b) = new Scalar("a").Two();
            y[x] = a * x + b;
            Assert.True(y.IsAssigned);
            Assert.Equal(3, y.ElementwiseAssignment.Expression.Tensors.Count);
        }
    }
}
