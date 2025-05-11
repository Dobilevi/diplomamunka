namespace Cpp.Messages
{
    public struct DespawnMessage
    {
        public Spawnable spawnable;
        public ulong id;

        public DespawnMessage(Spawnable spawnable, ulong id)
        {
            this.spawnable = spawnable;
            this.id = id;
        }
    }
}
