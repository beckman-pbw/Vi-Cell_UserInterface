using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ScoutUI.Configuration
{
    /// <summary>
    /// Convert command line options into an IConfiguration implementation.
    /// </summary>
    public class CommandLineProvider : ConfigurationProvider
    {
        private Dictionary<string, string> _commandLineOptions = new Dictionary<string, string>();

        /// <summary>
        /// Set values as discovered by the commandline parser.
        /// </summary>
        /// <param name="name">Configuration's key name for the command line parameter.</param>
        /// <param name="value">Value of the command line parameter</param>
        public void SetOption(string name, string value)
        {
            _commandLineOptions[name] = value;
        }

        public override void Load()
        {
            foreach (var commandLineOption in _commandLineOptions)
            {
                Set(commandLineOption.Key, commandLineOption.Value);
            }
        }
    }
}