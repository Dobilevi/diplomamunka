using System;
using System.Runtime.InteropServices;
using System.Text;

using Cpp.Messages;

namespace Cpp
{
    public class HostBufferWriter
    {
        private int index = 0;
        public int Size => index;

        private byte[] buffer;
        public ref byte[] Buffer => ref buffer;

        public HostBufferWriter(ushort size)
        {
            buffer = new byte[size];
        }

        public void Reset()
        {
            index = 0;
        }

        public void WriteUInt16(ushort value)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(ByteOrderConverter.HostToNetworkOrder(value)), 0, buffer, index, Marshal.SizeOf(typeof(ushort)));
        
            index += Marshal.SizeOf(typeof(ushort));
        }

        public void WriteUInt64(ulong value)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(ByteOrderConverter.HostToNetworkOrder(value)), 0, buffer, index, Marshal.SizeOf(typeof(ulong)));
        
            index += Marshal.SizeOf(typeof(ulong));
        }
        
        public void WriteFloat(float value)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(ByteOrderConverter.HostToNetworkOrder(value)), 0, buffer, index, Marshal.SizeOf(typeof(float)));

            index += Marshal.SizeOf(typeof(float));
        }

        public void WriteUpdateMessage(UpdateMessage value)
        {
            ByteOrderConverter.HostToNetworkOrder(ref value);

            System.Buffer.BlockCopy(StructConverter.WriteStruct(value), 0, buffer, index, Marshal.SizeOf(typeof(UpdateMessage)));

            index += Marshal.SizeOf(typeof(UpdateMessage));
        }
        
        public void WriteSpawnMessage(SpawnMessage value)
        {
            ByteOrderConverter.HostToNetworkOrder(ref value);

            System.Buffer.BlockCopy(StructConverter.WriteStruct(value), 0, buffer, index, Marshal.SizeOf(typeof(SpawnMessage)));

            index += Marshal.SizeOf(typeof(SpawnMessage));
        }
        
        public void WriteSpawnProjectileMessage(SpawnProjectileMessage value)
        {
            ByteOrderConverter.HostToNetworkOrder(ref value);

            System.Buffer.BlockCopy(StructConverter.WriteStruct(value), 0, buffer, index, Marshal.SizeOf(typeof(SpawnProjectileMessage)));

            index += Marshal.SizeOf(typeof(SpawnProjectileMessage));
        }
        
        public void WriteDespawnMessage(DespawnMessage value)
        {
            ByteOrderConverter.HostToNetworkOrder(ref value);

            System.Buffer.BlockCopy(StructConverter.WriteStruct(value), 0, buffer, index, Marshal.SizeOf(typeof(DespawnMessage)));

            index += Marshal.SizeOf(typeof(DespawnMessage));
        }

        public void WriteString(string value)
        {
            System.Buffer.BlockCopy(Encoding.Unicode.GetBytes(value), 0, buffer, index, 2 * value.Length);

            if (BitConverter.IsLittleEndian)
            {
                byte first;
                for (int i = 0; i < 2 * value.Length; i += 2)
                {
                    first = buffer[index + i];
                    buffer[index + i] = buffer[index + i + 1];
                    buffer[index + i + 1] = first;
                }
            }

            index += 2 * value.Length;
        }
    }
}
