using System;
using System.Collections.Generic;
using log4net;
using ServicoNoticias.Controllers;
using ServicoNoticias.Boundaries;
using System.Linq;

namespace ServicoNoticias.Views
{

    public static class ViewUsuarios
    {
        // Validação do usuário não implementada (qualquer usuário e senha será permitido entrar)
        public static string ValidaUsuario(ILog log, IList<KeyValuePair<string, string>> paramList, ControllerGatewayNoticias controllerGatewaySMS)
        {
            var callbackFunction = paramList.FirstOrDefault(t => t.Key == "callback").Value ?? "";
            try
            {
                return HttpHelper.AddHttpHeaderRequests(true, "true|0|0", callbackFunction);
            }
            catch (Exception ex)
            {
                log.Error("Exceção: " + ex);
                return HttpHelper.AddHttpHeaderLista("ServicoMensageria", "[{}]", callbackFunction, false, "Identificação invalida");
            }
        }

    }
}
