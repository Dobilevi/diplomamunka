namespace Cpp.Messages
{
    public struct SpawnMessage
    {
        public ulong clientId;
        public float x;
        public float y;
        public float rotation;

        public SpawnMessage(ulong clientId, float x, float y, float rotation)
        {
            this.clientId = clientId;
            this.x = x;
            this.y = y;
            this.rotation = rotation;
        }
    }
}
