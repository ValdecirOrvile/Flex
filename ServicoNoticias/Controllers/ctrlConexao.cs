using System;
using System.Data;
using System.Data.Odbc;
using System.Reflection;
using log4net;

namespace ServicoNoticias.DAOs
{
    class CtrlConexao
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string _odbcNome;
        private string _odbcUsuario;
        private string _odbcSenha;

        private OdbcConnection _conexaoBase;

        public enum Bases { BaseGravacao };

        public CtrlConexao(string odbcNome, string odbcUsuario, string odbcSenha)
        {
            _odbcNome = odbcNome;
            _odbcUsuario = odbcUsuario;
            _odbcSenha = odbcSenha;
            CtrlFazerConexoes();

            if (!CtrlConectado())
            {
                Log.ErrorFormat("Não conseguiu conectar com a base no odbc: {0}", odbcSenha);
                Environment.Exit(1);
            }
        }


        public void CtrlFazerConexoes()
        {
            ConfiguraConexao();
            Conecta();
        }

        public void CtrlFazerDesconexoes()
        {
            Desconecta();
        }
        public Boolean CtrlConectado()
        {
            if (_conexaoBase.State == ConnectionState.Closed)
                return false;
            else
                return true;
        }

        private void ConfiguraConexao()
        {
              _conexaoBase = new OdbcConnection(String.Format("DSN={0};UID={1};PWD={2}",
                  new object[] { _odbcNome, _odbcUsuario, _odbcSenha }));
        }

        private void Conecta()
        {
            try
            {

                 if (_conexaoBase.State == ConnectionState.Closed)
                 {
                     _conexaoBase.Open();
                 }

            }
            catch (Exception ex)
            {
                Log.Error("Erro de conexão: " + ex);
            }
        }

        private void Desconecta()
        {
            _conexaoBase.Close();
        }

        public bool ExecSql(string sql)
        {
            try
            {
                CheckConexao();

                OdbcConnection conexao;
                conexao = _conexaoBase;

                OdbcCommand odbcComando = conexao.CreateCommand();
                odbcComando.CommandText = sql;

                odbcComando.ExecuteNonQuery();
                odbcComando.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                string mensagem = ex.Message;
                // Se o motivo da exceção for duplicação de chave primary lança outra exceção para não gravar o registro
                if (mensagem.Contains("PRIMARY")) 
                    throw new Exception();
                else
                {
                    // Se o motivo da exceção for falta de conexão com o banco grava o camando para inserir mais tarde
                    if (mensagem.Contains("The connection has been disabled") ||
                        sql.Contains("ADD INDEX"))
                    {
                        return false;
                    }
                    else
                    {
                        // Se o motivo da exceção for desconhecida finaliza o programa
                        Log.Error(ex);
                        throw new Exception("Fechar Aplicacao");
                    }
                }
            }            
        }

        public OdbcDataReader ExecQuery(string sql)
        {
            CheckConexao();

            OdbcConnection conexao;
            conexao = _conexaoBase;

            OdbcCommand odbcComando = conexao.CreateCommand();
            odbcComando.CommandText = sql;

            // Pode ser que a conexao tenha sido perdida e o estado dela seja open.
            // Neste caso tenta conectar novamente e executar o comando.
            try
            {
                OdbcDataReader queryReader = odbcComando.ExecuteReader();
                odbcComando.Dispose();
                return queryReader;
            }
            catch (Exception)
            {
                Desconecta();
                Conecta();
                OdbcDataReader queryReader = odbcComando.ExecuteReader();
                odbcComando.Dispose();
                return queryReader;
            }
        }

        private void CheckConexao()
        {
            if (_conexaoBase.State != ConnectionState.Open)
                Conecta();
        }

        public Boolean CheckEstaConectado()
        {
            if (_conexaoBase.State == ConnectionState.Closed)
                Conecta();

            if (_conexaoBase.State == ConnectionState.Open)
                return true;
            else
                return false;
        }
    
    }
}
