using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Loading_a_3D_model
{
    class ByteMarshal
    {
        public static Byte[]  WriteDataMarshalCopy<T>(T[] src) {

            var sizeOfData = Marshal.SizeOf(typeof(T));
            var length = sizeOfData * src.Length;
            var bytes = new byte[length];

            var handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(src, GCHandleType.Pinned);
                var ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }
    }
}
