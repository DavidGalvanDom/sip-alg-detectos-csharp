using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace server_sip_alg
{
    public class ConfigLog
    {
        public Logger CreateLogerConfig()
        {
            Logger log = new LoggerConfiguration()
                      .WriteTo.Console(theme: AnsiConsoleTheme.Code,
                                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                       .WriteTo.File(@"c:\Log\sip-alg-server.log",rollingInterval: RollingInterval.Day)                      
                      .CreateLogger();

            return log;
        }
    }
}
