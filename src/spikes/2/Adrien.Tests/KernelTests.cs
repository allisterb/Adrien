﻿using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Adrien.Compiler;
using Adrien.Notation;

namespace Adrien.Tests
{
    public class KernelTests
    {
        [Fact]
        public void CanConstructKernel()
        {
            var A = Tensor.ThreeD("A", (2, 2, 2), "a", out Index a, out Index b, out Index c)
                        .With(out Tensor B)
                        .With(out Tensor C);
            C[a, b] = A[a, b] * B[b, a];
            Kernel<int> k = new Kernel<int>(C);

            Assert.Equal(C, k.OutputTensor);

            var V1 = new Vector("V1", out Index i).With(out Vector V2).With(out Vector V3);
            V3[i] = V1 + V2;
            Kernel<int> vk1 = new Kernel<int>(V3);
        }
    }
}
