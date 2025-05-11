using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System;

using Cpp.Messages;

namespace Cpp
{
    public class NamedPipeStreamWriter
    {
        private Stream ioStream;

        public NamedPipeStreamWriter(Stream ioStream)
        {
            this.ioStream = ioStream;
        }
    
        public void WriteUint16(ushort value)
        {
            value = ByteOrderConverter.HostToNetworkOrder(value);

            byte[] outBuffer = BitConverter.GetBytes(value);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = UInt16.MaxValue;
            }
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
        }

        public void WriteUint64(ulong value)
        {
            value = ByteOrderConverter.HostToNetworkOrder(value);

            byte[] outBuffer = BitConverter.GetBytes(value);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = UInt16.MaxValue;
            }
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
        }

        public void WriteMessageType(MessageType messageType)
        {
            WriteUint16((ushort)messageType);
        }

        public void WriteSpawnable(Spawnable spawnable)
        {
            WriteUint16((ushort)spawnable);
        }

        private void WriteStruct<T>(T message) where T : struct
        {
            int size = Marshal.SizeOf(message);
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

            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = UInt16.MaxValue;
            }
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
        }

        public void WriteUpdateMessage(UpdateMessage updateMessage)
        {
            ByteOrderConverter.HostToNetworkOrder(ref updateMessage);

            WriteStruct(updateMessage);
        }
        
        public void WriteSpawnMessage(SpawnMessage spawnMessage)
        {
            ByteOrderConverter.HostToNetworkOrder(ref spawnMessage);

            WriteStruct(spawnMessage);
        }
        
        public void WriteSpawnProjectileMessage(SpawnProjectileMessage spawnProjectileMessage)
        {
            ByteOrderConverter.HostToNetworkOrder(ref spawnProjectileMessage);

            WriteStruct(spawnProjectileMessage);
        }
        
        public void WriteDespawnMessage(DespawnMessage despawnMessage)
        {
            ByteOrderConverter.HostToNetworkOrder(ref despawnMessage);

            WriteStruct(despawnMessage);
        }

        public void WriteString(string value)
        {
            int size = 2 * value.Length;
            byte[] outBuffer = new byte[size];
            Buffer.BlockCopy(Encoding.Unicode.GetBytes(value), 0, outBuffer, 0, size);

            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = UInt16.MaxValue;
            }

            if (BitConverter.IsLittleEndian)
            {
                byte first;
                for (int i = 0; i < size; i += 2)
                {
                    first = outBuffer[i];
                    outBuffer[i] = outBuffer[i + 1];
                    outBuffer[i + 1] = first;
                }
            }

            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
        }
    }
}
