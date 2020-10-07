using MB.MidiRoundRobin.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MB.MidiRoundRobin.Cli
{
    public class CliManager
    {
        private RRConfiguration _rrConfiguration;

        public CliManager(RRConfiguration rrConfiguration)
        {
            _rrConfiguration = rrConfiguration;
        }

        public void Go()
        {
            var manager = new RRManager();

            Console.WriteLine($"MidiRoudRobin v.{manager.GetVersion()}");

            var ins = manager.EnumerateMidiInputs();
            var outs = manager.EnumerateMidiOutputs();

            if (ins.Count == 0)
            {
                Console.WriteLine("No MIDI input ports available. Exiting.");
            }

            if (outs.Count == 0)
            {
                Console.WriteLine("No MIDI output ports available. Exiting.");
            }

            Console.WriteLine();
            Console.WriteLine("MIDI input ports:");
            for (var n = 0; n < ins.Count; n++)
            {
                var port = ins[n];
                Console.WriteLine($"{n + 1} - {port.Description}");
            }

            InputMidiPortInfo midiIn = null;
            if (!string.IsNullOrEmpty(_rrConfiguration.MidiIn))
                midiIn = ins.FirstOrDefault(x => x.Description == _rrConfiguration.MidiIn);

            if (midiIn == null)  // midi port not present in configuration or not found. Interactive mode
                midiIn = ins.Count == 1
                    ? ins[0]
                    : ins[GetNumber($"Select a MIDI input port (from 1 to {ins.Count}):", 1, ins.Count) - 1];

            Console.WriteLine();
            Console.WriteLine("MIDI output ports:");
            for (var n = 0; n < outs.Count; n++)
            {
                var port = outs[n];
                Console.WriteLine($"{n + 1} - {port.Description}");
            }

            OutputMidiPortInfo midiOut = null;
            if (!string.IsNullOrEmpty(_rrConfiguration.MidiOut))
                midiOut = outs.FirstOrDefault(x => x.Description == _rrConfiguration.MidiOut);

            if (midiOut == null)  // midi port not present in configuration or not found. Interactive mode
                midiOut = ins.Count == 1
                    ? outs[0]
                    : outs[GetNumber($"Select a MIDI output port (from 1 to {outs.Count}):", 1, outs.Count) - 1];

            IEnumerable<byte> channels = null;

            if (!string.IsNullOrEmpty(_rrConfiguration.Channels))
                channels = SplitNumbers(_rrConfiguration.Channels, 1, 16, 2, null);

            if (channels == null) // channels not present in configuration or in wrong format. Interactive mode
            {
                Console.WriteLine();
                channels = GetNumbers("Select 2 or more MIDI channels to roud robin (e.g. 1,3,4):", 1, 16, 2, null);
            }

            Console.WriteLine();
            Console.WriteLine($"Round robin from '{midiIn.Description}' to '{midiOut.Description}' on channels {string.Join(",", channels)}.");
            Console.WriteLine($"Press [ENTER] to exit.");

            manager.StartRoundRobin(midiIn, midiOut, channels.ToArray());

            Console.ReadLine();
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

        private byte[] GetNumbers(string message, int min, int max, int? nMin, int? nMax)
        {
            while (true)
            {
                Console.Write($"{message} ");
                List<byte> numbers = SplitNumbers(Console.ReadLine(), min, max, nMin, nMax);

                if (numbers != null)
                    return numbers.ToArray();
            }
        }

        private List<byte> SplitNumbers(string s, int min, int max, int? nMin, int? nMax)
        {
            var numbers = new List<byte>();

            foreach (var nStr in s.Split(','))
            {
                if (byte.TryParse(nStr, out byte n))
                {
                    if (n < min || n > max)
                        return null;

                    numbers.Add(n);
                }
                else
                {
                    return null;
                }
            }

            if (nMin.HasValue && numbers.Count < nMin)
                return null;

            if (nMax.HasValue && numbers.Count > nMax)
                return null;

            return numbers;
        }
    }
}