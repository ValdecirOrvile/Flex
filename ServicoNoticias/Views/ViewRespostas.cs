using System;
using System.Collections.Generic;
using log4net;
using ServicoNoticias.Controllers;
using ServicoNoticias.Boundaries;
using Newtonsoft.Json;
using System.Linq;

namespace ServicoNoticias.Views
{
    public static class ViewRespostas
    {
        public static string ListaNoticias(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            try
            {
                var filtro = paramList.FirstOrDefault(t => t.Key == "filtro").Value ?? "";

                // Verifica se recebeu o parâmetro filtro neste caso roda o metodo com o filtro 
                if (filtro == "")
                {
                    var lista = controllerGatewaySMS.ListaNoticias();

                    return HttpHelper.AddHttpHeader(JsonConvert.SerializeObject(lista, Formatting.Indented),
                        lista.Count);
                }
                else
                {
                    var lista = controllerGatewaySMS.ListaNoticiasFiltro(filtro);

                    return HttpHelper.AddHttpHeader(JsonConvert.SerializeObject(lista, Formatting.Indented),
                        lista.Count);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeader("[]", 0);
            }
        }

        public static string ListaNoticiasFiltro(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            try
            {
                var callbackFunction = paramList.FirstOrDefault(t => t.Key == "callback").Value ?? "";
                var filtro = paramList.FirstOrDefault(t => t.Key == "filtro").Value ?? "";

                // Testa se recebeu filtro em branco
                if (filtro == "")
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Nenhum dado de filtro foi passado.", callbackFunction);
                }

                var lista = controllerGatewaySMS.ListaNoticiasFiltro(filtro);

                return HttpHelper.AddHttpHeader(JsonConvert.SerializeObject(lista, Formatting.Indented),
                    lista.Count);
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeader("[]", 0);
            }
        }

        public static string ListaAutores(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            try
            {
                var lista = controllerGatewaySMS.ListaAutores();

                return HttpHelper.AddHttpHeader(JsonConvert.SerializeObject(lista, Formatting.Indented),
                    lista.Count);
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeader("[]", 0);
            }
        }

        public static string LeTextoNoticia(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            try
            {
                var callbackFunction = paramList.FirstOrDefault(t => t.Key == "callback").Value ?? "";
                var id_noticia = paramList.FirstOrDefault(t => t.Key == "id_noticia").Value ?? "";

                // Testa se recebeu o id_noticia em branco
                if (id_noticia == "")
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Seleção de uma nóticia é obrigatória na leitura.", callbackFunction);
                }

                var lista = controllerGatewaySMS.LetextoNoticia(id_noticia);

                return HttpHelper.AddHttpHeader(JsonConvert.SerializeObject(lista, Formatting.Indented),
                    lista.Count);
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeader("[]", 0);
            }
        }
        public static string CriaNoticia(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            var callbackFunction = paramList.FirstOrDefault(t => t.Key == "callback").Value ?? "";
            try
            {
                var autor = paramList.FirstOrDefault(t => t.Key == "autor").Value ?? "";
                var titulo = paramList.FirstOrDefault(t => t.Key == "titulo").Value ?? "";
                var texto = paramList.FirstOrDefault(t => t.Key == "texto").Value ?? "";

                // Testa id do autor em branco não enviado (nome não selecionado)
                if (autor == "")
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Nome do autor da notícia é obrigatório.", callbackFunction);
                }

                // Testa título da notícia em branco
                if (titulo == "")
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Título da notícia é obrigatório.", callbackFunction);
                }

                // Testa texto da notícia em branco
                if (texto == "")
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Texto da notícia é obrigatório.", callbackFunction);
                }

                if (!controllerGatewaySMS.CriaNoticia(titulo, autor, texto))
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Falha ao criar a notícia.", callbackFunction);
                }
                return HttpHelper.AddHttpHeaderRequests(true, "Sucesso na criação da noticia", callbackFunction);
                
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeaderRequests(false, "Falha no comando de criar noticia", callbackFunction);
            }
        }

        public static string ExcluiNoticia(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            var id_noticia = paramList.FirstOrDefault(t => t.Key == "id_noticia").Value ?? "0";
            var callbackFunction = paramList.FirstOrDefault(t => t.Key == "callback").Value ?? "";
            try
            {

                // Testa se recebeu o id_noticia em branco
                if (id_noticia == "")
                {
                    return HttpHelper.AddHttpHeaderRequests(false, "Seleção de uma nóticia é obrigatória na exclusão.", callbackFunction);
                }

                if (controllerGatewaySMS.ExecutaComando($"delete from noticias where id_noticia = {id_noticia};"))
                {
                    return HttpHelper.AddHttpHeaderLista("ServicoNoticias", "[{}]", callbackFunction, true, "Noticia excluída com sucesso");
                }
                else
                {
                    return HttpHelper.AddHttpHeaderLista("ServicoMensageria", "[{}]", callbackFunction, false, "Falha no comando de excluir campanha. Tente novamente.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeaderLista("ServicoMensageria", "[{}]", callbackFunction, false, "Falha no comando de excluir campanha.");
            }
        }


    }
}
