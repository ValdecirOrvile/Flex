using System;
using System.Collections.Generic;
using ServicoNoticias.Boundaries;
using ServicoNoticias.DAOs;
using ServicoNoticias.Models;

namespace ServicoNoticias.Controllers
{
    public class ControllerGatewayNoticias
    {
        private DAOBaseNoticias _daoBaseNoticias;

        public ControllerGatewayNoticias(Parameters parametros)
        {
            _daoBaseNoticias = new DAOBaseNoticias(parametros.odbcNoticias, parametros.odbcUsuario, parametros.odbcSenha);
        }

        // Retorna a lista de noticias
        public List<NoticiaAutor> ListaNoticias()
        {
            return _daoBaseNoticias.LeNoticias();
        }

        // Retorna a lista de notícias do filtro
        public List<NoticiaAutor> ListaNoticiasFiltro(string filtro)
        {
            return _daoBaseNoticias.PesquisaNoticias(filtro);
        }

        // Retorna a lista de autores
        public List<Autor> ListaAutores()
        {
            return _daoBaseNoticias.LeAutores();
        }

        // Retorna a texto de uma notia
        public List<Noticia> LetextoNoticia(string id_noticia)
        {
            return _daoBaseNoticias.LeTextoNoticia(id_noticia);
        }
        // Cadastra uma nova noticia
        public Boolean CriaNoticia(string titulo, string autor, string noticia)
        {
            try
            {
                _daoBaseNoticias.CriaNoticia(titulo, autor, noticia);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Metodo que executa um comando na base das notícias.
        public Boolean ExecutaComando(string sql)
        {
            try
            {
                _daoBaseNoticias.ExecutaComando(sql);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
