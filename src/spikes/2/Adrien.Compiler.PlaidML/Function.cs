﻿using System;
using Adrien.Compiler.PlaidML.Bindings;

namespace Adrien.Compiler.PlaidML
{
    public class Function : PlaidMLApi<Function>
    {
        public string Id { get; protected set; }

        public string Code { get; protected set; }


        public Function(Context ctx, string code) : base(ctx)
        {
            string id = "id_" + Guid.NewGuid().ToString("N");
            _ptr = plaidml.__Internal.PlaidmlBuildCodedFunction(code, id);
            if (_ptr.IsZero())
            {
                ReportApiCallError("plaidml_build_coded_function");
            }
            else
            {
                Id = id;
                Code = code;
                IsAllocated = true;
                Info("Added function Id:{0} Code={1}", Id, Code);
            }
        }

        public Function(Context ctx, IntPtr functionPtr) : base(ctx)
        {
            // TODO: [vermorel] 'id' is never used, and 'Id' is never assigned. Probably not the intended behavior.
            // REMARK: [allisterb] Correctly assign Id value.
            Id = "id_" + Guid.NewGuid().ToString("N");
            _ptr = functionPtr;
            IsAllocated = true;
            Info("Added function Id:{0} from pointer {1}", Id, functionPtr);
        }


        public override void Free()
        {
            base.Free();
            plaidml.__Internal.PlaidmlFreeFunction(this);
            _ptr = IntPtr.Zero;
        }
    }
}