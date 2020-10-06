using System;
using MB.MidiRoundRobin.Core;

namespace MidiRoundRobin
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new RRManager();

            Console.WriteLine($"MidiRoudRobin v.{manager.GetVersion()}");

            Console.WriteLine("MIDI inputs:");
            foreach (var idName in manager.EnumerateMidiInputs())
            {
                Console.WriteLine($"{idName.Key} -  {idName.Value}");
            }

            Console.WriteLine();

            Console.WriteLine("MIDI outputs:");
            foreach (var idName in manager.EnumerateMidiOutputs())
            {
                Console.WriteLine($"{idName.Key} -  {idName.Value}");
            }

            Console.WriteLine();

            manager.StartRoundRobin("0", "4", new byte[] { 1, 2, 3, 4, 5, 6 });

            Console.ReadKey();

            manager.StopRoundRobin();
        }
    }
}
