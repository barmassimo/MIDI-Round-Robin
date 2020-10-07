using MB.MidiRoundRobin.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MB.MidiRoundRobin.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("MidiRR.settings.json", true)
                .AddCommandLine(args);

            var configuration = builder.Build();

            var rrConfiguration = new RRConfiguration
            {
                MidiIn = configuration?["midiIn"],
                MidiOut = configuration?["midiOut"],
                Channels = configuration?["channels"]
            };

            var cli = new CliManager(rrConfiguration);
            cli.Go();
        }
    }
}
