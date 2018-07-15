﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Newtonsoft.Json;

using Adrien.Compiler.PlaidML.Bindings;
using Adrien.Compiler.PlaidML.Generator;

namespace Adrien.Compiler.PlaidML
{
    public class TileCompiler : PlaidMLApi<TileCompiler>, ICompiler 
    {
        public Dictionary<string, object> Options { get; }

        public ITensorContext TensorContext { get; }

        public bool Initialized { get; protected set; }

        public CompilerStatus Status { get; protected set; }

        public string CompilerStatusMessage { get; protected set; }
        
        public Settings Settings => _Context.settings;

        public string SessionId { get; protected set; }

        public DeviceEnumerator DeviceEnumerator { get; protected set; }

        public int DeviceCount => DeviceEnumerator.Count;
       
        public Device KernelDevice { get; protected set; }

        public Dictionary<string, object> KernelDeviceProperties { get; protected set; }

        
        public TileCompiler(Dictionary<string, object> options = null) : base(new Context())
        {
            if (!_Context.IsAllocated)
            {
                CompilerStatusMessage = _Context.LastStatusString;
                return;
            }
            Options = options;
            SessionId = Settings.StartNewSession();
            DeviceEnumerator = new DeviceEnumerator(_Context);
            if (!DeviceEnumerator.IsAllocated)
            {
                CompilerStatusMessage = DeviceEnumerator.LastStatusString;
                return;
            }
            if (DeviceEnumerator.ValidDevices.Count < 1)
            {
                Error("No valid devices available.");
                return;
            }
            IsAllocated = true;
            KernelDevice = OpenFirstDevice();
            KernelDeviceProperties = JsonConvert.DeserializeObject<Dictionary<string, object>>
               (KernelDevice.DeviceConfig.Details);
            Initialized = true;
        }

        public bool Compile<TKernel>(IKernel<TKernel> kernel, out IRunnable<TKernel> result)
            where TKernel : unmanaged, IEquatable<TKernel>, IComparable<TKernel>, IConvertible
        {
            result = null;
            ThrowIfNotInitialized();
            Status = CompilerStatus.Compiling;
            TileGenerator g = new TileGenerator(kernel.ExpressionTree);
            if (!g.Success)
            {
                Status = CompilerStatus.ErrorGeneratingCode;
                CompilerStatusMessage = g.Text;
                return false;
            }
            Function f = CreateFunction(g);
            if (!f.IsAllocated)
            {
                Status = CompilerStatus.ErrorGeneratingCode;
                CompilerStatusMessage = f.LastStatusString;
                return false;
            }
            
            DeviceTensor[] inputTensors = kernel.InputShapes
                .Select(i => CreateTensor(CreateShape<TKernel>(i.Dimensions), i.Label))
                .ToArray();
            DeviceTensor outputTensor = CreateTensor(CreateShape<TKernel>(kernel.OutputShape.Dimensions),
                kernel.OutputShape.Label);
            Invoker<TKernel> invoker = new Invoker<TKernel>(Context, f, outputTensor, inputTensors);
            if (invoker.IsAllocated && invoker.AllVariablesSet)
            {
                result = invoker;
                return true;
            }
            else
            {
                Status = CompilerStatus.ErrorGeneratingCode;
                CompilerStatusMessage = invoker.LastStatusString;
                return false;
            }   
        }

        public bool Compile<TKernel>(IEnumerable<IVariableShape> inputShapes, IVariableShape outputShape, string code, 
            out IRunnable<TKernel> result)
                where TKernel : unmanaged, IEquatable<TKernel>, IComparable<TKernel>, IConvertible
        {
            result = null;
            ThrowIfNotInitialized();
            Status = CompilerStatus.Compiling;
            Function f = CreateFunction(code);
            if (!f.IsAllocated)
            {
                CompilerStatusMessage = f.LastStatusString;
                return false;
            }

            DeviceTensor[] inputTensors = inputShapes
                .Select(i => CreateTensor(CreateShape<TKernel>(i.Dimensions), i.Label))
                .ToArray();
            DeviceTensor outputTensor = CreateTensor(CreateShape<TKernel>(outputShape.Dimensions),
                outputShape.Label);
            Invoker<TKernel> invoker = new Invoker<TKernel>(Context, f, outputTensor, inputTensors);
            if (invoker.IsAllocated && invoker.AllVariablesSet)
            {
                result = invoker;
                return true;
            }
            else
            {
                CompilerStatusMessage = invoker.LastStatusString;
                return false;
            }
        }

        public bool Compile<TKernel>(IVariableShape inputShape, IVariableShape outputShape, string code,
            out IRunnable<TKernel> result)
            where TKernel : unmanaged, IEquatable<TKernel>, IComparable<TKernel>, IConvertible
            => Compile(new IVariableShape[] { inputShape }, outputShape, code, out result);

        public bool Compile<TVectorKernel>(int vectorRank, string code, out IRunnable<TVectorKernel> result)
            where TVectorKernel : unmanaged, IEquatable<TVectorKernel>, IComparable<TVectorKernel>, IConvertible
        {
            IVariableShape input = new DeviceTensor(OpenFirstDevice(), CreateShape<TVectorKernel>(vectorRank), "I");
            IVariableShape output = new DeviceTensor(OpenFirstDevice(), CreateShape<TVectorKernel>(vectorRank), "O");
            return Compile(input, output, code, out result);
        }

        public Device OpenFirstDevice()
        {
            ThrowIfNotAllocated();
            if (DeviceEnumerator.Count == 0)
            {
                throw new Exception("No devices were enumerated.");
            }
            else
            {
                return new Device(_Context, DeviceEnumerator.ValidDevices[0]);
            }
        }

        public Function CreateFunction(TileGenerator generator)
        {
            ThrowIfNotInitialized();
            return new Function(Context, generator.Text);
           
        }

        public Function CreateFunction(string code)
        {
            ThrowIfNotAllocated();
            return new Function(_Context, code);
        }

        public Shape CreateShape<T>(params int[] dimensions) where T : unmanaged
        {
            ThrowIfNotAllocated();
            PlaidmlDatatype datatype = Shape.ToDataType<T>();
            return new Shape(_Context, datatype, dimensions);
        }

        public DeviceTensor CreateTensor(Shape shape, string name)
        {
            ThrowIfNotAllocated();
            return new DeviceTensor(KernelDevice, shape, name);
        }


        [DebuggerStepThrough]
        internal void ThrowIfNotInitialized()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("This compiler instance is not initialized.");
            }
        }
    }
}
