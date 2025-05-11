namespace Cpp.Messages
{
    public struct SpawnProjectileMessage
    {
        public Spawnable spawnable;
        public ulong clientId;
        public ulong projectileId;
        public float x;
        public float y;
        public float rotation;

        public SpawnProjectileMessage(Spawnable spawnable, ulong clientId, ulong projectileId, float x, float y, float rotation)
        {
            this.spawnable = spawnable;
            this.clientId = clientId;
            this.projectileId = projectileId;
            this.x = x;
            this.y = y;
            this.rotation = rotation;
        }
    }
}
