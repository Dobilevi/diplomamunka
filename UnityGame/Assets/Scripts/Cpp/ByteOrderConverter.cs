using System;

using Cpp.Messages;

namespace Cpp
{
    public class ByteOrderConverter
    {
        public static ushort HostToNetworkOrder(ushort hostUshort)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] buffer = new byte[sizeof(ushort)];
                Buffer.BlockCopy(BitConverter.GetBytes(hostUshort), 0, buffer, 0, sizeof(ushort));
                Array.Reverse(buffer);
                return BitConverter.ToUInt16(buffer);
            }

            return hostUshort;
        }

        public static ushort NetworkToHostOrder(ushort networkUshort)
        {
            return HostToNetworkOrder(networkUshort);
        }

        public static ulong HostToNetworkOrder(ulong hostUlong)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] buffer = new byte[sizeof(ulong)];
                Buffer.BlockCopy(BitConverter.GetBytes(hostUlong), 0, buffer, 0, sizeof(ulong));
                Array.Reverse(buffer);
                return BitConverter.ToUInt64(buffer);
            }

            return hostUlong;
        }

        public static ulong NetworkToHostOrder(ulong networkUlong)
        {
            return HostToNetworkOrder(networkUlong);
        }

        public static float HostToNetworkOrder(float hostFloat)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] buffer = new byte[sizeof(float)];
                Buffer.BlockCopy(BitConverter.GetBytes(hostFloat), 0, buffer, 0, sizeof(float));
                Array.Reverse(buffer);
                return BitConverter.ToSingle(buffer);
            }

            return hostFloat;
        }

        public static float NetworkToHostOrder(float networkFloat)
        {
            return HostToNetworkOrder(networkFloat);
        }

        public static void HostToNetworkOrder(ref UpdateMessage updateMessage)
        {
            if (BitConverter.IsLittleEndian)
            {
                updateMessage.updateType = (UpdateType)HostToNetworkOrder((ushort)updateMessage.updateType);
                updateMessage.clientId = HostToNetworkOrder(updateMessage.clientId);
                updateMessage.x = HostToNetworkOrder(updateMessage.x);
                updateMessage.y = HostToNetworkOrder(updateMessage.y);
                updateMessage.rotation = HostToNetworkOrder(updateMessage.rotation);
            }
        }

        public static void NetworkToHostOrder(ref UpdateMessage updateMessage)
        {
            HostToNetworkOrder(ref updateMessage);
        }

        public static void HostToNetworkOrder(ref SpawnMessage spawnMessage)
        {
            if (BitConverter.IsLittleEndian)
            {
                spawnMessage.clientId = HostToNetworkOrder(spawnMessage.clientId);
                spawnMessage.x = HostToNetworkOrder(spawnMessage.x);
                spawnMessage.y = HostToNetworkOrder(spawnMessage.y);
                spawnMessage.rotation = HostToNetworkOrder(spawnMessage.rotation);
            }
        }

        public static void NetworkToHostOrder(ref SpawnMessage spawnMessage)
        {
            HostToNetworkOrder(ref spawnMessage);
        }

        public static void HostToNetworkOrder(ref SpawnProjectileMessage spawnProjectileMessage)
        {
            if (BitConverter.IsLittleEndian)
            {
                spawnProjectileMessage.spawnable = (Spawnable)HostToNetworkOrder((ushort)spawnProjectileMessage.spawnable);
                spawnProjectileMessage.clientId = HostToNetworkOrder(spawnProjectileMessage.clientId);
                spawnProjectileMessage.projectileId = HostToNetworkOrder(spawnProjectileMessage.projectileId);
                spawnProjectileMessage.x = HostToNetworkOrder(spawnProjectileMessage.x);
                spawnProjectileMessage.y = HostToNetworkOrder(spawnProjectileMessage.y);
                spawnProjectileMessage.rotation = HostToNetworkOrder(spawnProjectileMessage.rotation);
            }
        }

        public static void NetworkToHostOrder(ref SpawnProjectileMessage spawnProjectileMessage)
        {
            HostToNetworkOrder(ref spawnProjectileMessage);
        }
        
        public static void HostToNetworkOrder(ref DespawnMessage despawnMessage)
        {
            if (BitConverter.IsLittleEndian)
            {
                despawnMessage.spawnable = (Spawnable)HostToNetworkOrder((ushort)despawnMessage.spawnable);
                despawnMessage.id = HostToNetworkOrder(despawnMessage.id);
            }
        }

        public static void NetworkToHostOrder(ref DespawnMessage despawnMessage)
        {
            HostToNetworkOrder(ref despawnMessage);
        }
    }
}
