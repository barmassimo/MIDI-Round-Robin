using System;
using System.Collections.Generic;
using MB.MidiRoundRobin.Core;

namespace MidiRoundRobin
{
    public class Cli
    {
        public void Go()
        {
            var manager = new RRManager();

            var ins = manager.EnumerateMidiInputs();
            var outs = manager.EnumerateMidiOutputs();

            Console.WriteLine($"MidiRoudRobin v.{manager.GetVersion()}");

            Console.WriteLine("MIDI input ports:");
            for (var n = 0; n < ins.Count; n++)
            {
                var port = ins[n];
                Console.WriteLine($"{n + 1} - {port.Description}");
            }

            Console.WriteLine();

            Console.WriteLine("MIDI output ports:");
            for (var n = 0; n < outs.Count; n++)
            {
                var port = outs[n];
                Console.WriteLine($"{n + 1} - {port.Description}");
            }

            Console.WriteLine();

            if (ins.Count == 0)
            {
                Console.WriteLine("No MIDI input port available. Exiting.");
            }

            if (outs.Count == 0)
            {
                Console.WriteLine("No MIDI output port available. Exiting.");
            }

            var selectedMidiIn = ins.Count == 1 ? 0 : GetNumber($"Select a MIDI input port (1..{ins.Count})", 1, ins.Count) - 1;
            var selectedMidiOut = ins.Count == 1 ? 0 : GetNumber($"Select a MIDI output port (1..{outs.Count})", 1, outs.Count) - 1;

            var midiIn = ins[selectedMidiIn];
            var midiOut = outs[selectedMidiOut];
            var midiChannels = GetNumbers("Select 2 or more midi channels to roud robin (e.g. 1,3,4)", 1, 16, 2, 16);

            Console.WriteLine($"Round robin from '{midiIn.Description}' to '{midiOut.Description}' on channels {string.Join(", ", midiChannels)}.");
            Console.WriteLine($"Press a key to exit.");

            manager.StartRoundRobin(midiIn, midiOut, midiChannels);

            Console.ReadKey();
            Console.WriteLine($"Exiting.");

            manager.StopRoundRobin();
        }

        private int GetNumber(string message, int min, int max)
        {
            while (true)
            {
                Console.Write($"{message} ");

                if (int.TryParse(Console.ReadLine(), out int n))
                {
                    if (n >= min && n <= max)
                        return n;
                }
            }
        }

        private byte[] GetNumbers(string message, int min, int max, int nMin, int nMax)
        {
            while (true)
            {
                Console.Write($"{message} ");
                var numbersStr = Console.ReadLine().Split(',');
                var numbers = new List<byte>();

                foreach (var nStr in numbersStr)
                {
                    if (byte.TryParse(nStr, out byte n))
                    {
                        if (n < min || n > max)
                            continue;

                        numbers.Add(n);
                    }
                    else
                    {
                        continue;
                    }
                }

                if (numbers.Count < nMin || numbers.Count > nMax)
                    continue;

                return numbers.ToArray();
            }
        }
    }
}
