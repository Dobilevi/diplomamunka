using System;
using System.Runtime.InteropServices;

using Cpp.Messages;

namespace Cpp
{
    public class NetworkBufferReader
    {
        private int bufferSize;
        private int index = 0;
        private byte[] buffer;

        public byte[] Buffer
        {
            set
            {
                buffer = value;
                index = 0;
            }
        }

        public NetworkBufferReader(byte[] buffer = null)
        {
            this.buffer = buffer;
        }

        void CheckSize(int size)
        {
            if ((index + size) > buffer.Length)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public UpdateMessage ReadUpdateMessage()
        {
            CheckSize(Marshal.SizeOf(typeof(UpdateMessage)));

            UpdateMessage value = StructConverter.ReadStruct<UpdateMessage>(buffer, index);

            index += Marshal.SizeOf<UpdateMessage>();

            ByteOrderConverter.NetworkToHostOrder(ref value);

            return value;
        }

        public SpawnMessage ReadSpawnMessage()
        {
            CheckSize(Marshal.SizeOf(typeof(SpawnMessage)));

            SpawnMessage value = StructConverter.ReadStruct<SpawnMessage>(buffer, index);

            index += Marshal.SizeOf<SpawnMessage>();

            ByteOrderConverter.NetworkToHostOrder(ref value);

            return value;
        }

        public SpawnProjectileMessage ReadSpawnProjectileMessage()
        {
            CheckSize(Marshal.SizeOf(typeof(SpawnProjectileMessage)));

            SpawnProjectileMessage value = StructConverter.ReadStruct<SpawnProjectileMessage>(buffer, index);

            index += Marshal.SizeOf<SpawnProjectileMessage>();

            ByteOrderConverter.NetworkToHostOrder(ref value);

            return value;
        }

        public DespawnMessage ReadDespawnMessage()
        {
            CheckSize(Marshal.SizeOf(typeof(DespawnMessage)));

            DespawnMessage value = StructConverter.ReadStruct<DespawnMessage>(buffer, index);

            index += Marshal.SizeOf<DespawnMessage>();

            ByteOrderConverter.NetworkToHostOrder(ref value);

            return value;
        }

        public ushort ReadUint16()
        {
            CheckSize(sizeof(ushort));

            ushort data = ByteOrderConverter.NetworkToHostOrder(BitConverter.ToUInt16(buffer, index));

            index += sizeof(ushort);

            return data;
        }

        public ulong ReadUint64()
        {
            CheckSize(sizeof(ulong));

            ulong data = ByteOrderConverter.NetworkToHostOrder(BitConverter.ToUInt64(buffer, index));

            index += sizeof(ulong);

            return data;
        }
        
        public MessageType ReadMessageType()
        {
            return (MessageType)ReadUint16();
        }

        public Spawnable ReadSpawnable()
        {
            return (Spawnable)ReadUint16();
        }

        public string ReadString(ushort length)
        {
            CheckSize(2 * length);

            byte[] inBuffer = new byte[2 * length];
            System.Buffer.BlockCopy(buffer, index, inBuffer, 0, 2 * length);

            if (BitConverter.IsLittleEndian)
            {
                byte first;
                for (int i = 0; i < 2 * length; i += 2)
                {
                    first = inBuffer[i];
                    inBuffer[i] = inBuffer[i + 1];
                    inBuffer[i + 1] = first;
                }
            }

            index += 2 * length;

            return System.Text.Encoding.Unicode.GetString(inBuffer);
        }
    }
}
