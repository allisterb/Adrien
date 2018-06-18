﻿using System;
using System.Collections.Generic;
using System.Text;

using Adrien.Compiler.PlaidML.Bindings;

namespace Adrien.Compiler.PlaidML
{
    public class Device : PlaidMLApi<Device>
    {
        public DeviceConfig DeviceConfig { get; protected set; }
        public bool IsOpen { get; protected set; }
        public bool IsClosed { get; protected set; }
        public List<DeviceBuffer> Buffers { get; protected set; }
        

        public Device(Context ctx, DeviceConfig devconf = null) : base(ctx)
        {
            if (devconf != null)
            {
                ptr = plaidml.__Internal.PlaidmlOpenDevice(context, devconf);
            }
            else
            {
                ptr = plaidml.__Internal.PlaidmlOpenDevice(context, IntPtr.Zero);
            }
            if (ptr.IsZero())
            {
                ReportApiCallError("plaidml_open_device");
                return;
            }
            else
            {
                DeviceConfig = devconf;
                Buffers = new List<DeviceBuffer>();
                IsAllocated = true;
                IsOpen = true;
            }
        }


        public override void Free()
        {
            if (this.IsOpen)
            {
                this.Close();
            }
            base.Free();
        }
        

        public void Close()
        {
            ThrowIfNotAllocated();
            ThrowIfNotOpen();
            if (Buffers != null && Buffers.Count > 0)
            {
                Buffers.ForEach(buffer => buffer.Free());
            }
            plaidml.__Internal.PlaidmlCloseDevice(this);
            this.IsOpen = false;
            this.IsClosed = true;
        }

        public DeviceBuffer CreateBuffer(Shape shape)
        {
            return new DeviceBuffer(this.context, this, shape);
        }

        internal void ThrowIfNotOpen()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException("This device is not open.");
            }
        }

    }
}
