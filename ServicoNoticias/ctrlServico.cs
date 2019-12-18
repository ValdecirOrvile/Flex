using System;
using System.ServiceProcess;
using ServicoNoticias.Controllers;
using ServicoNoticias.Boundaries;
using log4net;

namespace TeclanServicoSSM
{
    public partial class CtrlServico : ServiceBase
    {
        private ILog _log;

        private Parameters _parameters;
        private SocketServer _serverSocket;
        private readonly object _lockSocketServer = new object();
        private ControllerGatewayNoticias _controllerGatewaySMS;

        public CtrlServico(ILog log)
        {
            InitializeComponent();
            _log = log;
        }

        public void RodaComoAppDoVisualStudio()
        {
            OnStart(null);
        }
        private void ImprimeCabecalhoLog()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly()
                                                       .GetName()
                                                       .Version
                                                       .ToString();            
            _log.Info("=====================================================================");
            _log.Info($"Serviço Noticias - versão {version} - iniciando");
            _log.Info("=====================================================================");

        }

        protected override void OnStart(string[] args)
        {
            ImprimeCabecalhoLog();

            // código do serviço começa aqui!
            try
            {
                _log.Debug("Inicializando Parametros...");
                _parameters = new Parameters(_log);

                _log.Debug("Inicializando Consumidor...");
                _controllerGatewaySMS = new ControllerGatewayNoticias(_parameters);

                _log.Debug("Inicializando Socket Server...");
                _serverSocket = new SocketServer(_log, _parameters, _controllerGatewaySMS);

                if (!_serverSocket.Start())
                    Stop();
            }
            catch (Exception ex)
            {
                _log.Debug("Exception: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _log.Info("Serviço Encerrado");
            }
            catch (Exception)
            {
                _log.Info("Serviço Encerrado");
            }
        }
    }
}
