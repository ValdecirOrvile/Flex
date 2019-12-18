using System;
using System.Collections.Generic;
using System.Data.Odbc;
using ServicoNoticias.Models;
using System.Reflection;
using log4net;
using System.Data;

namespace ServicoNoticias.DAOs
{
    class DAOBaseNoticias
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CtrlConexao _ctrlConexao;


        public DAOBaseNoticias(string ConexaoOdbc, string usuarioOdbc, string senhaOdbc)
        {
            _ctrlConexao = new CtrlConexao(ConexaoOdbc, usuarioOdbc, senhaOdbc);
            _ctrlConexao.CtrlFazerConexoes();
        }

        public bool IsConnectedC(OdbcConnection connection)
        {
            if (connection == null) return false;

            var connectionState = connection?.State;

            return connectionState == ConnectionState.Open ||
                   connectionState == ConnectionState.Executing ||
                   connectionState == ConnectionState.Fetching;
        }

         public void CriaNoticia(string titulo, string autor, string noticia)
        {
            try
            {
                if (!_ctrlConexao.CtrlConectado())
                    _ctrlConexao.CtrlFazerConexoes();

                if (_ctrlConexao.CtrlConectado())
                {
                    try
                    {
                        // Insere noticia, o id_noticia é auto incrementado pelo banco
                       var query =  $"INSERT INTO noticias(id_autor,Titulo,Texto) VALUES('{autor}', '{titulo}', '{noticia}');";
                        
                       _ctrlConexao.ExecSql(query);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Erro na criação da notícia: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw new Exception(ex.Message);
            }
        }

        // Retorna lista com titulos das noticias nome do autor e id da noticia
        public List<NoticiaAutor> LeNoticias()
        {
            List<NoticiaAutor> listaNoticias = new List<NoticiaAutor>();

            try
            {
                // Lê noticias limitado a 500 registros
                var sql = "SELECT autores.Nome, noticias.Titulo, noticias.id_noticia FROM noticias left join autores on autores.id_autor = noticias.id_autor limit 500;";

                OdbcDataReader queryReader = _ctrlConexao.ExecQuery(sql);

                while (queryReader.Read())
                {
                    {
                        NoticiaAutor noticiaAutor = new NoticiaAutor()
                        {
                            autor = queryReader.GetString(0),
                            titulo = queryReader.GetString(1),
                            id_noticia = queryReader.GetString(2)
                        };
                        listaNoticias.Add(noticiaAutor);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return listaNoticias;
        }

        // Retorna lista com titulos das noticias nome do autor e id da noticia que atendem o filtro da pesquisa
        public List<NoticiaAutor> PesquisaNoticias(string filtro)
        {
            List<NoticiaAutor> listaNoticias = new List<NoticiaAutor>();

            try
            {
                // Lê noticias que atendem o filtro
                var sql = "SELECT * FROM " +
                          "(SELECT autores.Nome, noticias.Titulo, noticias.Texto, noticias.id_noticia FROM noticias left join autores on autores.id_autor = noticias.id_autor " +
                          $") as a where Nome like '%{filtro}%' or Titulo like '%{filtro}%' or Texto like '%{filtro}%' order by Titulo, nome;";

                OdbcDataReader queryReader = _ctrlConexao.ExecQuery(sql);

                while (queryReader.Read())
                {
                    {
                        NoticiaAutor noticiaAutor = new NoticiaAutor()
                        {
                            autor = queryReader.GetString(0),
                            titulo = queryReader.GetString(1),
                            id_noticia = queryReader.GetString(3)
                        };
                        listaNoticias.Add(noticiaAutor);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return listaNoticias;
        }

        public List<Noticia> LeTextoNoticia(string id_noticia)
        {
            List<Noticia> listaNoticias = new List<Noticia>();

            try
            {
                // Lê texto da noticia atravez da seu id_noticia
                var sql = $"SELECT autores.Nome, noticias.Titulo, noticias.texto FROM noticias left join autores on autores.id_autor = noticias.id_autor where noticias.id_noticia = {id_noticia};";

                OdbcDataReader queryReader = _ctrlConexao.ExecQuery(sql);

                while (queryReader.Read())
                {
                    {
                        Noticia noticias = new Noticia()
                        {
                            autor = queryReader.GetString(0),
                            titulo = queryReader.GetString(1),
                            texto = queryReader.GetString(2)
                        };
                        listaNoticias.Add(noticias);
                    }

                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return listaNoticias;
        }
        // Lista dos autores cadastrados na base
        public List<Autor> LeAutores()
        {
            List<Autor> listaAutores = new List<Autor>();

            try
            {
                // Le autores limitado a 500 registros
                var sql = "SELECT id_autor, Nome FROM autores limit 500;";

                OdbcDataReader queryReader = _ctrlConexao.ExecQuery(sql);

                while (queryReader.Read())
                {
                    {
                        Autor autor = new Autor()
                        {
                            id_autor = queryReader.GetString(0),
                            Nome = queryReader.GetString(1)
                        };
                        listaAutores.Add(autor);
                    }

                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return listaAutores;
        }
        // Executa comandos
        public void ExecutaComando(string query)
        {
            try
            {
                if (!_ctrlConexao.CtrlConectado())
                    _ctrlConexao.CtrlFazerConexoes();

                if (_ctrlConexao.CtrlConectado())
                {
                    try
                    {
                        _ctrlConexao.ExecSql(query);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Erro abtenção dos dados na query: {query}, {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw new Exception(ex.Message);
            }
        }

    }
}
