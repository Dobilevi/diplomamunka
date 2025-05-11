using System;
using System.Runtime.InteropServices;

namespace Cpp
{
    public class StructConverter
    {
        public static T ReadStruct<T>(byte[] inBuffer, int offset) where T: struct
        {
            T message = new T();

            int len = Marshal.SizeOf(typeof(T));
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(len);

                Marshal.Copy(inBuffer, offset, ptr, len);

                message = (T)Marshal.PtrToStructure(ptr, message.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return message;
        }

        public static byte[] WriteStruct<T>(T message) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] outBuffer = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(message, ptr, true);
                Marshal.Copy(ptr, outBuffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return outBuffer;
        }
    }
}
