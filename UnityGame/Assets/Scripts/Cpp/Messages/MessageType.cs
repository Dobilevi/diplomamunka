namespace Cpp.Messages
{
    public enum MessageType: ushort
    {
        None = 0,
        Exit,
        Connect,
        Disconnect,
        SpawnProjectile,
        InitializeProjectile,
        Despawn,
        Update,
        SpawnEnemy,
        SpawnPlayer,
        InitializeEnemy,
        InitializePlayer,
        RespawnAck,
        ErrorServerFull,
        ErrorObjectDoesNotExist
    }
}
