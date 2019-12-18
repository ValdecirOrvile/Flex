using System;
using System.Text;

namespace ServicoNoticias.Boundaries
{
    public static class HttpHelper
    {
        static readonly StringBuilder Headers = new StringBuilder();
        static readonly StringBuilder PayLoad = new StringBuilder();

        public static string AddHttpHeader(string responseContent, int results)
        {
            PayLoad.Clear();

         //   if (callbackFunction != string.Empty)
           // {
             //   PayLoad.AppendFormat("{0}(", callbackFunction);
          //  } 
            
            PayLoad.Append("{");
            PayLoad.AppendFormat("\"total\":{0},", results);
            PayLoad.Append("\"success\":true,");
            PayLoad.Append("\"rows\":");
            PayLoad.Append(responseContent);
            PayLoad.Append("}");
            
          //  if (callbackFunction != string.Empty)
          //  {
          //      PayLoad.Append(")");
          //  }

            Headers.Clear();
            Headers.AppendLine("HTTP/1.1 200 OK");
            Headers.AppendLine("Access-Control-Allow-Credentials:true");
            Headers.AppendLine("Access-Control-Allow-Headers:Origin, x-requested-with, Content-Type, Accept, Range");
            Headers.AppendLine("Access-Control-Allow-Methods:GET,POST,PUT,DELETE,OPTIONS");
            Headers.AppendLine("Access-Control-Allow-Origin: *");
            Headers.AppendLine("Access-Control-Expose-Headers:Accept-Ranges, Content-Encoding, Content-Length, Content-Range");
            Headers.AppendLine("Content-Type:text/javascript;charset=UTF-8");
            Headers.AppendLine("Cache-Control: no-cache");
            Headers.AppendLine("");
            Headers.AppendLine("");

            return Headers + PayLoad.ToString();
        }

        public static string AddHttpHeaderLista(string tipo, string nome, string callbackFunction, Boolean sucessoNaCriacao, string mensagem)
        {
            StringBuilder payLoad = new StringBuilder();

            payLoad.Clear();

            if (callbackFunction != string.Empty)
            {
                payLoad.AppendFormat("{0}(", callbackFunction);
            }

            payLoad.Append("{");
            payLoad.AppendFormat("\"{0}\":{1},", tipo, nome);
            payLoad.Append(string.Format("\"success\": {0},", sucessoNaCriacao.ToString().ToLower()));
            payLoad.AppendFormat("\"message\":\"{0}\"", mensagem);
            payLoad.Append("}");

            if (callbackFunction != string.Empty)
            {
                payLoad.Append(")");
            }

            return Headers + payLoad.ToString();
        }

        public static string AddHttpHeaderRequests(bool reposta, string message, string callbackFunction)
        {
            PayLoad.Clear();

            if (callbackFunction != string.Empty)
            {
                PayLoad.AppendFormat("{0}(", callbackFunction);
            }

            PayLoad.Append("{");
            PayLoad.Append($"\"success\":{reposta.ToString().ToLower()},");
            PayLoad.Append($"\"message\":'{message}'");
            PayLoad.Append("}");

            Headers.Clear();
            Headers.AppendLine("HTTP/1.1 200 OK");
            Headers.AppendLine("Access-Control-Allow-Credentials:true");
            Headers.AppendLine("Access-Control-Allow-Headers:Origin, x-requested-with, Content-Type, Accept, Range");
            Headers.AppendLine("Access-Control-Allow-Methods:GET,POST,PUT,DELETE,OPTIONS");
            Headers.AppendLine("Access-Control-Allow-Origin: *");
            Headers.AppendLine("Access-Control-Expose-Headers:Accept-Ranges, Content-Encoding, Content-Length, Content-Range");
            Headers.AppendLine("Content-Type:text/javascript;charset=UTF-8");
            Headers.AppendLine("Cache-Control: no-cache");
            Headers.AppendLine("");
            Headers.AppendLine("");

            if (callbackFunction != string.Empty)
            {
                PayLoad.Append(")");
            }
            return Headers + PayLoad.ToString();
        }
    }

}
