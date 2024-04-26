using Microsoft.Extensions.Configuration;

namespace ScoutUI.Configuration
{
    public class CommandLineSource : IConfigurationSource
    {
        private readonly CommandLineProvider _commandLineProvider;

        public CommandLineSource(CommandLineProvider commandLineProvider)
        {
            _commandLineProvider = commandLineProvider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return _commandLineProvider;
        }
    }
}