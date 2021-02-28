

using Newtonsoft.Json;

public class Config
{
    public Config()
    {
        var jsonString = System.IO.File.ReadAllText("./Config/config.json");
        ConfigData = JsonConvert.DeserializeObject<Data>(jsonString);
    }
    public Data ConfigData { get; }
    
    public class Data
    {
        public string Token { get; set; }
        public string DBConnectionString { get; set; }
    }
}

