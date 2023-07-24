using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AWRP {
    internal class Config {
        internal const string CONFIG_NAME = "awrp.json";
        internal const string PASSWORD_NULL = "__NULL__";

        internal static bool Load(string path, out Config config) {
            try {
                
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                return true;
            } catch (JsonException) {
                config = null;
                return false;
            }    
        }   

        internal static void Write(string path, Config config) {
            string content = JsonConvert.SerializeObject(config);
            File.WriteAllText(path, content);
        }

        internal class UploadItem {
            [JsonProperty]
            public string Src;
            [JsonProperty]
            public string ResoucrePath;
            
            [JsonProperty]
            public string Description;

            [JsonProperty]
            public string SolutionUniqueName;
        }

        [JsonProperty]
        public string Host;
        [JsonProperty]
        public string User;
        [JsonProperty]
        public string Password;

        [JsonProperty]
        public string Prefix;

        [JsonProperty]
        public List<UploadItem> Uploads;
    }
}