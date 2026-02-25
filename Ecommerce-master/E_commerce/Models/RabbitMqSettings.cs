namespace E_commerce.Models
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Exchange { get; set; }
        public string Queue { get; set; }

        public int PoolSize { get; set; }
    }
}
