namespace DisplayJSON.Serializers
{
    public class Log
    {
        public int? guid { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Level { get; set; }
        public string? MessageTemplate { get; set; }
        public Properties? Properties { get; set; }
    }
}
