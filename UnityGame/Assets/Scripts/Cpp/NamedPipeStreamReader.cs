using System;
using System.IO;
using System.Runtime.InteropServices;

using Cpp.Messages;

namespace Cpp
{
    // Defines the data protocol for reading and writing strings on our stream
    public class NamedPipeStreamReader
    {
        private Stream ioStream;

        private int readBytes;

        public NamedPipeStreamReader(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

        public ulong ReadUint64()
        {
            int len = Marshal.SizeOf(typeof(ulong));

            byte[] inBuffer = new byte[sizeof(ulong)];
            readBytes = ioStream.Read(inBuffer, 0, len);

            if (readBytes != len)
            {
                throw new EndOfStreamException();
            }

            return ByteOrderConverter.NetworkToHostOrder(BitConverter.ToUInt64(inBuffer));
        }

        public ushort ReadUint16()
        {
            int len = Marshal.SizeOf(typeof(ushort));

            byte[] inBuffer = new byte[sizeof(ushort)];
            readBytes = ioStream.Read(inBuffer, 0, len);

            if (readBytes != len)
            {
                throw new EndOfStreamException();
            }

            return ByteOrderConverter.NetworkToHostOrder(BitConverter.ToUInt16(inBuffer));
        }

        public MessageType ReadMessageType()
        {
            return (MessageType)ReadUint16();
        }

        private void ReadStruct<T>(ref T var) where T : struct
        {
            int len = Marshal.SizeOf(typeof(T));

            byte[] inBuffer = new byte[len];
            readBytes = ioStream.Read(inBuffer, 0, len);

            if (readBytes != len)
            {
                throw new EndOfStreamException();
            }

            len = Marshal.SizeOf(var);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(len);

                Marshal.Copy(inBuffer, 0, ptr, len);

                var = (T)Marshal.PtrToStructure(ptr, var.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public void ReadUpdateMessage(ref UpdateMessage var)
        {
            ReadStruct(ref var);

            ByteOrderConverter.NetworkToHostOrder(ref var);
        }

        public void ReadSpawnMessage(ref SpawnMessage var)
        {
            ReadStruct(ref var);

            ByteOrderConverter.NetworkToHostOrder(ref var);
        }

        public void ReadSpawnProjectileMessage(ref SpawnProjectileMessage var)
        {
            ReadStruct(ref var);

            ByteOrderConverter.NetworkToHostOrder(ref var);
        }

        public void ReadDespawnMessage(ref DespawnMessage var)
        {
            ReadStruct(ref var);

            ByteOrderConverter.NetworkToHostOrder(ref var);
        }

        public string ReadString(ushort length)
        {
            int size = 2 * length;

            byte[] inBuffer = new byte[size];
            readBytes = ioStream.Read(inBuffer, 0, size);

            if (readBytes != size)
            {
                throw new EndOfStreamException();
            }

            if (BitConverter.IsLittleEndian)
            {
                byte first;
                for (int i = 0; i < size; i += 2)
                {
                    first = inBuffer[i];
                    inBuffer[i] = inBuffer[i + 1];
                    inBuffer[i + 1] = first;
                }
            }

            return System.Text.Encoding.Unicode.GetString(inBuffer);
        }
    }
}
