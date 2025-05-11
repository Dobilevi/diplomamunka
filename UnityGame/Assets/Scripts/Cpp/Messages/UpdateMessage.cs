namespace Cpp.Messages
{
    public struct UpdateMessage
    {
        public UpdateType updateType;
        public ulong clientId;
        public float x;
        public float y;
        public float rotation;

        public UpdateMessage(UpdateType updateType, ulong clientId, float x, float y, float rotation)
        {
            this.updateType = updateType;
            this.clientId = clientId;
            this.x = x;
            this.y = y;
            this.rotation = rotation;
        }
    }
}
