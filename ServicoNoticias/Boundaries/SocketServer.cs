using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;
using ServicoNoticias.Views;
using ServicoNoticias.Models;
using ServicoNoticias.Controllers;
using System.Threading.Tasks;

namespace ServicoNoticias.Boundaries
{
    public class SocketServer : IDisposable
    {
        private readonly ILog _log;
        private readonly Socket _serverSocket;
        private readonly Parameters _parameters;
        private ControllerGatewayNoticias _controllerGatewaySMS;

        private List<BufferSocketServer> _listSocket;
        private object lockSocket = new object();

        public SocketServer(ILog log, Parameters parameters, ControllerGatewayNoticias controllerGatewaySMS)
        {
            _log = log;
            _parameters = parameters;
            _listSocket = new List<BufferSocketServer>();
            _controllerGatewaySMS = controllerGatewaySMS;

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        }

        public bool Start()
        {
            try
            {
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _parameters.serverPort));
                _serverSocket.Listen(1);
                _serverSocket.BeginAccept(ServerSocketAcceptCallback, null);
                _log.Info("Socket Server iniciado.");
                return true;

            }
            catch (Exception ex)
            {
                _log.Error("Exceção no start do socket server: " + ex.Message);
                return false;
            }
        }

        private void ServerSocketAcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (_serverSocket == null)
                    return;

                var clientSocket = _serverSocket.EndAccept(ar);

                var buffer = new BufferSocketServer
                {
                    Handle = clientSocket.Handle.ToString(),
                    Buffer = new byte[30720],
                    BufferTime = DateTime.Now
                };

                lock (lockSocket)
                {
                    _listSocket.Add(buffer);

                    
                    Task.Run(() => clientSocket?.BeginReceive(buffer.Buffer, 0, buffer.Buffer.Length, SocketFlags.None,
                       SocketClientReceiveCallback, clientSocket));

                }

                _serverSocket.BeginAccept(ServerSocketAcceptCallback, null);
            }
            catch (Exception ex)
            {
                _log.Error("ERRO OPERAÇAO DE SOCKET: " + ex);
                _serverSocket.BeginAccept(ServerSocketAcceptCallback, null);
            }
        }

        private void SocketClientReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var socket = (Socket)ar.AsyncState;

                if (socket == null)
                    return;

                if (socket.Connected)
                {

                    int received;
                    try
                    {
                        received = socket.EndReceive(ar);

                        if (received <= 0)
                        {
                            // encerra comunicação com o cliente.
                            socket.Close();
                            socket.Dispose();
                            socket = null;
                            return;
                        }


                        byte[] buffer = new byte[30720];
                        lock (lockSocket)
                        {
                            try
                            {
                                buffer = _listSocket.FirstOrDefault(s => s.Handle == socket.Handle.ToString()).Buffer;
                                _listSocket.RemoveAll(s => s.Handle == socket.Handle.ToString() ||
                                                           (DateTime.Now - s.BufferTime).TotalSeconds > 30);
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Não encontrou a associação com o socket: " + ex.Message);
                                socket.Close();
                                socket.Dispose();
                                socket = null;
                                return;
                            }
                        }

                        Array.Copy(buffer, buffer, received);
                        var textData = Encoding.UTF8.GetString(buffer);
                        var request = textData.Split('\n');

                        if (request.Length > 1)
                        {
                            var requestPage = request[0].Split(' ')[1];
                            _log.Debug("Client [" + socket.Handle + "] <- " + requestPage);

                            // Faz o tratamento da requisição conforme a página problema na liberacao do socket deixar sequencia por hora
                            SwitchRequestPage(socket, requestPage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Exceção ao receber dados do socket cliente: " + ex.Message);
                        socket.Close();
                        socket.Dispose();
                        socket = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Exceção: " + ex.Message);
            }
        }

        private void SendClientData(Socket socket, string message)
        {
            try
            {
                
                _log.Debug("Client [" + socket.Handle + "] -> Respondeu");                

                socket.Send(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetBytes(message).Length, SocketFlags.None);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
                socket = null;
            }
            catch (Exception ex)
            {
                _log.Error("Exceção: " + ex.Message);
            }
        }

        private void SwitchRequestPage(Socket socket, string requestPage)
        {
            requestPage = Uri.UnescapeDataString(requestPage);
            var paramList = GetRequestParameters(requestPage);
            try
            {

                switch (paramList.First().Key)
                {
                    case "/ValidaUsuario":
                        SendClientData(socket, ViewUsuarios.ValidaUsuario(_log, paramList, _controllerGatewaySMS));
                        break;

                    case "/ListaNoticias":
                        SendClientData(socket, ViewRespostas.ListaNoticias(_log, paramList, _controllerGatewaySMS));
                        break;

                    case "/ListaNoticiasFiltro":
                        SendClientData(socket, ViewRespostas.ListaNoticiasFiltro(_log, paramList, _controllerGatewaySMS));
                        break;

                    case "/ListaAutores":
                        SendClientData(socket, ViewRespostas.ListaAutores(_log, paramList, _controllerGatewaySMS));
                        break;

                    case "/LeDadosNoticia":
                        SendClientData(socket, ViewRespostas.LeTextoNoticia(_log, paramList, _controllerGatewaySMS));
                        break;

                    case "/CriaNoticia":
                        SendClientData(socket, ViewRespostas.CriaNoticia(_log, paramList, _controllerGatewaySMS));
                        break;     

                    case "/ExcluiNoticia":
                        SendClientData(socket, ViewRespostas.ExcluiNoticia(_log, paramList, _controllerGatewaySMS));
                        break;

                    default:
                        throw new Exception("Solicitação desconhecida");
                }

                paramList.Clear();
                paramList = null;

                // encerra comunicação com o cliente.
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket.Dispose();
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex);
             //   List<string> listaDeMailings = new List<string>();
                var callbackFunction = paramList.FirstOrDefault(t => t.Key == "callback").Value ?? "";
                SendClientData(socket,
                    HttpHelper.AddHttpHeaderLista("ServicoMensageria",
                        "[{}]", callbackFunction, false,
                        ex.Message.Split('\n')[0].Replace('"', ' ')));
            }
        }

        private IList<KeyValuePair<string, string>> GetRequestParameters(string request)
        {
            var list = new List<KeyValuePair<string, string>>();

            if (request.IndexOf('?') < 0)
            {
                list.Add(new KeyValuePair<string, string>(request, ""));
                return list;
            }

            var page = request.Substring(0, request.IndexOf('?'));
            list.Add(new KeyValuePair<string, string>(page, ""));

            request = request.Remove(0, request.IndexOf('?') + 1);

            var parameters = request.Split('&');

            foreach (var parameter in parameters)
            {
                if (parameter.IndexOf('=') >= 0)
                {
                    var chave = parameter.Substring(0, parameter.IndexOf('='));
                    var dados = parameter.Remove(0, parameter.IndexOf('=') + 1);
                    list.Add(new KeyValuePair<string, string>(chave, dados));
                }
            }

            return list;
        }

        public void Dispose()
        {
            _serverSocket.Shutdown(SocketShutdown.Both);
            _serverSocket.Close();
            _log.Warn("Socket server fechado.");
        }
    }
}
