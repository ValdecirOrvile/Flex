using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using ServicoNoticias.Models;

namespace ServicoNoticias.Boundaries
{
    public static class GerenciadorOdbc
    {
        private const string OdbcIniRegPath = @"SOFTWARE\ODBC\ODBC.INI\";
        private const string OdbcinstIniRegPath = @"SOFTWARE\ODBC\ODBCINST.INI\";

        // Método que cria o DSN de acordo com o banco utilizado
        public static void CreateDsn(string dsnName, Odbc odbc)
        {
            var dsnKey = CriaDsn(dsnName, odbc);
            var driverKey = CaminhoDriverOdbc(odbc);

            switch (odbc.Banco)
            {
                case "Oracle":
                    CreateDsnOracle(dsnKey, driverKey, dsnName, odbc);
                    break;
                case "MySQL":
                    CreateDsnMySql(dsnKey, driverKey, dsnName, odbc);
                    break;
                case "SqlServer":
                    CreateDsnSqlServer(dsnKey, driverKey, dsnName, odbc);
                    break;
                case "PostgreSQL":
                    CreateDsnPostgreSql(dsnKey, driverKey, dsnName, odbc);
                    break;
            }
            TestaConexaoOdbc(dsnName, odbc);
        }

        public static void CreateDsnMySql(RegistryKey dsnKey, RegistryKey driverKey, string dsnName, Odbc odbc)
        {
            dsnKey.SetValue("DATABASE", odbc.Base);
            dsnKey.SetValue("DESCFIPTION", dsnName);
            dsnKey.SetValue("Driver", driverKey.GetValue("Driver").ToString());
            dsnKey.SetValue("UID", odbc.Usuario);
            dsnKey.SetValue("PWD", odbc.Senha);
            dsnKey.SetValue("SERVER", odbc.Ip);
            dsnKey.SetValue("PORT", odbc.Porta);
            dsnKey.SetValue("Trusted_Connection", "Yes");
        }

        public static void CreateDsnSqlServer(RegistryKey dsnKey, RegistryKey driverKey, string dsnName, Odbc odbc)
        {
            dsnKey.SetValue("Database", odbc.Base);
            dsnKey.SetValue("Description", dsnName);
            dsnKey.SetValue("Driver", driverKey);
            dsnKey.SetValue("User", odbc.Usuario);
            dsnKey.SetValue("Password", odbc.Senha);
            dsnKey.SetValue("Server", odbc.Ip);
            try
            {
                dsnKey.SetValue("Port", odbc.Porta);
            }
            catch (Exception)
            {
                // ignored
            }
            dsnKey.SetValue("LastUser", odbc.Usuario);
            dsnKey.SetValue("Trusted_Connection", "No");
        }

        public static void CreateDsnOracle(RegistryKey dsnKey, RegistryKey driverKey, string dsnName, Odbc odbc)
        {
            dsnKey.SetValue("Description", odbc.Banco);
            dsnKey.SetValue("Driver", driverKey);
            dsnKey.SetValue("UserID", odbc.Usuario);
            dsnKey.SetValue("Password", odbc.Senha);
            dsnKey.SetValue("ServerName", odbc.Dsn);
            dsnKey.SetValue("Trusted_Connection", "Yes");
        }

        public static void CreateDsnPostgreSql(RegistryKey dsnKey, RegistryKey driverKey, string dsnName, Odbc odbc)
        {
            dsnKey.SetValue("Database", odbc.Base);
            dsnKey.SetValue("Description", dsnName);
            dsnKey.SetValue("Driver", driverKey);
            dsnKey.SetValue("Username", odbc.Usuario);
            dsnKey.SetValue("Password", odbc.Senha);
            dsnKey.SetValue("Servername", odbc.Ip);
            dsnKey.SetValue("Port", odbc.Porta);
            dsnKey.SetValue("Trusted_Connection", "Yes");
        }

        private static RegistryKey CriaDsn(string dsnName, Odbc odbc)
        {
            // Verifica se o ODBC já existe
            if (DsnExiste(dsnName))
                throw new Exception("Já existe DSN com este nome");
            var driverKey = Registry.LocalMachine.CreateSubKey(OdbcinstIniRegPath + odbc.Driver);
            if (driverKey == null)
                throw new Exception(string.Format("Driver de fonte de dados {0} não existe", odbc.Driver));
            var datasourcesKey = Registry.LocalMachine.CreateSubKey(OdbcIniRegPath + "ODBC Data Sources");
            if (datasourcesKey == null)
                throw new Exception("Chave do Registro ODBC para fontes de dados não existe.");
            datasourcesKey.SetValue(dsnName, odbc.Driver);
            RegistryKey dsnKey = Registry.LocalMachine.CreateSubKey(OdbcIniRegPath + dsnName);
            if (dsnKey == null)
                throw new Exception("Chave do Registro ODBC para o DSN não foi criado.");

            return dsnKey;
        }

        private static RegistryKey CaminhoDriverOdbc(Odbc odbc)
        {
            var driverKey = Registry.LocalMachine.CreateSubKey(OdbcinstIniRegPath + odbc.Driver);
            if (driverKey == null)
                throw new Exception(string.Format("Driver de fonte de dados {0} não existe", odbc.Driver));
            return driverKey;
        }

        public static void RemoveDsn(string dsnName)
        {
            // Remove DSN key 
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(OdbcIniRegPath + dsnName);
            }
            catch (Exception)
            {
                throw new Exception("DSN com este nome não existe.");
            }
            // Remove DSN name from values list in ODBC Data Sources key 
            var datasourcesKey = Registry.LocalMachine.CreateSubKey(OdbcIniRegPath + "ODBC Data Sources");
            if (datasourcesKey == null)
                throw new Exception("DSN com este nome não existe.");

            datasourcesKey.DeleteValue(dsnName);
        }

        public static bool DsnExiste(string dsnName)
        {
            return RetornaListaDsnSistema().Contains(dsnName);
        }

        public static List<string> RetornaListaDsnSistema()
        {
            List<string> list = new List<string>();

            // Busca DSN do sistema 
            var datasourcesKey = Registry.LocalMachine.CreateSubKey(OdbcIniRegPath + "ODBC Data Sources");
            if (datasourcesKey == null)
                throw new Exception("Não existe nenhum DSN de sistema configurado.");
            foreach (var odbc in datasourcesKey.GetValueNames())
            {
                list.Add(odbc);
            }

            return list;
        }

        public static List<string> RetornaListaDrivers()
        {

            var driversKey = Registry.LocalMachine.CreateSubKey(OdbcinstIniRegPath + "ODBC Drivers");
            if (driversKey == null)
                throw new Exception("Não existem ODBC configurados.");
            var driverNames = driversKey.GetValueNames();
            var ret = new List<string>();
            foreach (var driverName in driverNames)
            {
                if (driverName != "(Default)")
                {
                    ret.Add(driverName);
                }
            }
            return ret;
        }

        private static void TestaConexaoOdbc(string dsnName, Odbc odbc)
        {
            OdbcConnection conexaoBase;

            conexaoBase = new OdbcConnection(String.Format("DSN={0};UID={1};PWD={2}",
                  new object[] { dsnName, odbc.Usuario, odbc.Senha }));

            try
            {
                conexaoBase.Open();
                conexaoBase.Close();
            }
            catch (Exception ex)
            {
                RemoveDsn(dsnName);
                throw new Exception(ex.Message);
            }

        }

        public static Odbc RetornaInformacoesDsnSistema(string nomeOdbc /*, ComandosContatosSql comandosContatosSql*/)
        {
            Odbc odbc = new Odbc();

            // Busca DSN do sistema 
            var driverKey = Registry.LocalMachine.OpenSubKey(OdbcIniRegPath + nomeOdbc);


            if (driverKey != null)
            {
                var driverKeyDataSource = Registry.LocalMachine.OpenSubKey(OdbcIniRegPath + "ODBC Data Sources");

                if (driverKeyDataSource == null)
                {
                    throw new Exception("Erro na utilização do driver para conexão com o banco. Favor Contactar com o suporte da Teclan.");
                }

                odbc.Nome = nomeOdbc;
                odbc.Banco = "MySQL";
                odbc.Base = (string)driverKey.GetValue("DATABASE");
                odbc.Driver = (string)driverKeyDataSource.GetValue(nomeOdbc);
                switch (odbc.Banco)
                {
                    case "Oracle":
                        odbc.Usuario = (string)driverKey.GetValue("UserID");
                        odbc.Senha = (string)driverKey.GetValue("Password");
                        odbc.Dsn = (string)driverKey.GetValue("ServerName");
                        break;
                    case "MySQL":
                        odbc.Usuario = (string)driverKey.GetValue("UID");
                        odbc.Senha = (string)driverKey.GetValue("PWD");
                        if (odbc.Usuario == null)
                        {
                            odbc.Usuario = (string)driverKey.GetValue("User");
                            odbc.Senha = (string)driverKey.GetValue("Password");
                        }
                        odbc.Ip = (string)driverKey.GetValue("SERVER");
                        odbc.Porta = (string)driverKey.GetValue("PORT");
                        odbc.Base = (string)driverKey.GetValue("DATABASE");
                        break;
                    case "SqlServer":
                        odbc.Usuario = (string)driverKey.GetValue("User");
                        odbc.Senha = (string)driverKey.GetValue("Password");
                        odbc.Ip = (string)driverKey.GetValue("Server");
                        odbc.Porta = (string)driverKey.GetValue("PORT");
                        odbc.Base = (string)driverKey.GetValue("DataBase");
                        break;
                    case "PostgreSQL":
                        odbc.Usuario = (string)driverKey.GetValue("Username");
                        odbc.Senha = (string)driverKey.GetValue("Password");
                        odbc.Ip = (string)driverKey.GetValue("Servername");
                        odbc.Porta = (string)driverKey.GetValue("Port");
                        odbc.Base = (string)driverKey.GetValue("DataBase");
                        break;
                }

            }
            if (odbc.Usuario == null || odbc.Usuario == "")
            {
                odbc.Usuario = /*comandosContatosSql.OdbcContatosUsuario */ "root";
                odbc.Senha = /*comandosContatosSql.OdbcContatosSenha */ "root";
            }
            // Se a senha do ODBC esta em branco usa a do parametro (caso do Oracle)
            if (odbc.Senha == "")
            {
                odbc.Senha = /*comandosContatosSql.OdbcContatosSenha*/ "";
            }

            return odbc;
        }


        public static string RetornaDriverDaConexao(string nomeOdbc)
        {
            string driver = "";

            // Busca DSN do sistema 
            var driverKey = Registry.LocalMachine.OpenSubKey(OdbcIniRegPath + nomeOdbc);


            if (driverKey != null)
            {
                var driverKeyDataSource = Registry.LocalMachine.OpenSubKey(OdbcIniRegPath + "ODBC Data Sources");

                if (driverKeyDataSource != null)
                {
                    driver = (string)driverKeyDataSource.GetValue(nomeOdbc);
                }
            }
            return driver;
        }
    }

}
