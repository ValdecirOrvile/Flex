using Newtonsoft.Json;

namespace ServicoNoticias.Models
{
    public class Odbc
    {
        public string Nome { get; set; }
        [JsonIgnore]
        public string Banco { get; set; }
        [JsonIgnore]
        public string Usuario { get; set; }
        [JsonIgnore]
        public string Senha { get; set; }
        [JsonIgnore]
        public string Ip { get; set; }
        [JsonIgnore]
        public string Porta { get; set; }
        //      [JsonIgnore]
        public string Driver { get; set; }
        [JsonIgnore]
        public string Dsn { get; set; }
        [JsonIgnore]
        public string Base { get; set; }
    }
}

