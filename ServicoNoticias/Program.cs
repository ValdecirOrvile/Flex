using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace TeclanServicoSSM
{
    static class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            if (Environment.UserInteractive)
            {
                if (args.Count() == 1)
                {
                    if (args[0] == "install")
                    {
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("Install: " + ex.Message);
                        }
                    }

                    if (args[0] == "uninstall")
                    {
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                    }
                }                
#if (!DEBUG)
                return;
#else
                // Debug code: Permite debugar um código sem se passar por um Windows Service.
                // Defina qual método deseja chamar no inicio do Debug (ex. MetodoRealizaFuncao)
                // Depois de debugar basta compilar em Release e instalar para funcionar normalmente.
                CtrlServico service1 = new CtrlServico(Log);
                // Chamada do seu método para Debug.
                service1.RodaComoAppDoVisualStudio();
                // Coloque sempre um breakpoint para o ponto de parada do seu código.
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif 
            }
            var servicesToRun = new ServiceBase[] 
            { 
                new CtrlServico(Log) 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
