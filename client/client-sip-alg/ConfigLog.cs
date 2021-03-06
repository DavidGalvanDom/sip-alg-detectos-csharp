using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using Serilog.Core;

namespace client_sip_alg
{
    public class ConfigLog
    {
        public Logger CreateLogerConfig ()
        {
            Logger log = new LoggerConfiguration()
                      .WriteTo.Console(theme: AnsiConsoleTheme.Code,
                                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                      .WriteTo.File("sip-alg-log.txt")
                      .CreateLogger();

            return log;
        }
    }
}
