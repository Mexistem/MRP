namespace MRP.Server
{
    public sealed class ServerSettings
    {
        public int Port { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
    }
}
