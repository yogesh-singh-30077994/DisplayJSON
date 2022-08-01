using DisplayJSON.Serializers;
using System.Text.Json;

namespace DisplayJSON.Utlis
{
    public class Deserializer
    {
        public static Log? DeserializeLog(string jsonString)
        {
            var log = JsonSerializer.Deserialize<Log>(jsonString);
            return log;
        }
    }
}
