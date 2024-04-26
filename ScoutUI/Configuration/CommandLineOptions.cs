using CommandLine;

namespace ScoutUI.Configuration
{
    public class CommandLineOptions
    {
        [Option('n', "no-watchdog", Required = false,
            HelpText = "Disable the automatic startup and watchdog timer for the OPC/UA Server.")]
        public bool NoWatchdog { get; set; } = false;
    }


}