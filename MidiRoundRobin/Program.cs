using MB.MidiRoundRobin.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MidiRoundRobin
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Configuration settings:
             * 
             * 1. configuration file
             * see appsettings.json (empty by default)
             * 
             * 2. command line arguments (overwrites configuration file)
             * > MB.MidiRoundRobin.exe --midiIn="Arturia KeyStep 32" --midiOut="Elektron Model:Cycles" --channels="1,2,3,4,5,6"
             * 
             * 3. interactive (actuve if appsettings.json is empty and command line arguments are not used)
             * > MB.MidiRoundRobin.exe
             * 
             */

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args);

            var configuration = builder.Build();

            var rrConfiguration = new RRConfiguration
            {
                MidiIn = configuration?["midiIn"],
                MidiOut = configuration?["midiOut"],
                Channels = configuration?["channels"]
            };

            var cli = new Cli(rrConfiguration);
            cli.Go();
        }
    }
}
