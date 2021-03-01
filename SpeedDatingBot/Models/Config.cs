

using Newtonsoft.Json;

public class Config
{
    public Config()
    {
        string jsonString = System.IO.File.ReadAllText("./Config/config.json");
        ConfigData = JsonConvert.DeserializeObject<Data>(jsonString);
    }
    public Data ConfigData { get; }
    
    public class Data
    {
        public string Token { get; set; }
        public string DbConnectionString { get; set; }
        public ulong GuildId { get; set; }
    }
}

