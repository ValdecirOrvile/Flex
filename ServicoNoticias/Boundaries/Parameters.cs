using System.Configuration;
using log4net;
using System;

namespace ServicoNoticias.Boundaries
{
    public class Parameters
    {  
        public int serverPort { get; set; }
        public string odbcNoticias { get; set; }
        public string odbcUsuario { get; set; }
        public string odbcSenha { get; set; }

        public Parameters(ILog log)
        {
            try
            {
                log.Info("Iniciando leitura dos parâmetros:");
                serverPort = int.Parse(ConfigurationManager.AppSettings["serverPort"]);
                odbcNoticias = ConfigurationManager.AppSettings["odbcNoticias"];
                odbcUsuario = ConfigurationManager.AppSettings["obdcUsuario"];
                odbcSenha = ConfigurationManager.AppSettings["odbcSenha"];

                log.Info("Parâmetros carregados.");
                log.Debug("Porta do Servidor: " + serverPort);
                log.Debug("Odbc Notícias: " + odbcNoticias);
            }
            catch (Exception ex)
            {
                log.Error("Não conseguiu ler os parametros para este serviço. Verifique o arquivo de config.");
                log.Error("Exceção: " + ex.Message);
                Environment.Exit(1);
            }

        }

    }
}
