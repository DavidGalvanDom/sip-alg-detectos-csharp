using System;
using Topshelf;

using server_sip_alg;

namespace server_sip_alg_windows_service
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1); // restart the service after 1 minute
                });

                x.Service<ServiceWrapper>(s =>
                {
                    s.ConstructUsing(instance => new ServiceWrapper());
                    s.WhenStarted(instance => instance.Start());
                    s.WhenStopped(instance => instance.Stop());
                });

                x.RunAsLocalSystem();
                x.StartAutomatically();

                x.SetDescription("Wrapper around Server SIP ALG, listening TCP and UDP to test SIP / ALG.");
                x.SetDisplayName("server-sip-alg-windows-service");
                x.SetServiceName("server-sip-alg-windows-service");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
