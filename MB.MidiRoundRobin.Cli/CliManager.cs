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

            Console.WriteLine($"MidiRoundRobin v.{manager.GetVersion()}");

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

            // MIDI IN
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
                    : ins[GetNumber($"Select a MIDI input port (from 1 to {ins.Count}):", 1, (byte)ins.Count) - 1];

            // MIDI OUT
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
                    : outs[GetNumber($"Select a MIDI output port (from 1 to {outs.Count}):", 1, (byte)outs.Count) - 1];

            // MIDI channels IN
            IEnumerable<byte> midiChannelsIn = null;
            if (!string.IsNullOrEmpty(_rrConfiguration.MidiChannelsIn))
                midiChannelsIn = SplitNumbers(_rrConfiguration.MidiChannelsIn, 1, 16, 0, null);

            if (midiChannelsIn == null)
            {
                Console.WriteLine();
                midiChannelsIn = GetNumbers("Select one or more MIDI input channels: (e.g. 1,2,4-6, default: all)", 1, 16, 0, 16);
            }

            if (midiChannelsIn.Count() == 0)
                midiChannelsIn = SplitNumbers("1-16", 1, 16, 0, null);

            // MIDI channels OUT
            IEnumerable<byte> midiChannelsOut = null;
            if (!string.IsNullOrEmpty(_rrConfiguration.MidiChannelsOut))
                midiChannelsOut = SplitNumbers(_rrConfiguration.MidiChannelsOut, 1, 16, 2, null);

            if (midiChannelsOut == null) // channels not present in configuration or in wrong format. Interactive mode
            {
                Console.WriteLine();
                midiChannelsOut = GetNumbers("Select one or more MIDI channels to round robin (e.g. 1,2,4-6):", 1, 16, 1, null);
            }

            Console.WriteLine();
            Console.WriteLine($"Round robin from '{midiIn.Description}' to '{midiOut.Description}'.");
            Console.WriteLine($"MIDI input channels: { string.Join(",", midiChannelsIn)}.");
            Console.WriteLine($"MIDI output channels (round robin): {string.Join(",", midiChannelsOut)}.");
            Console.WriteLine($"Press [Enter] to exit.");

            manager.StartRoundRobin(midiIn, midiOut, midiChannelsIn.ToArray(), midiChannelsOut.ToArray());

            Console.ReadLine();
            Console.Write($"Disposing MIDI port connections...");

            manager.StopRoundRobin();

            Console.WriteLine($" Ok.");
            Console.WriteLine($"Exiting.");
        }

        private byte GetNumber(string message, byte min, byte max)
        {
            while (true)
            {
                Console.Write($"{message} ");

                if (byte.TryParse(Console.ReadLine(), out byte n))
                {
                    if (n >= min && n <= max)
                        return n;
                }
            }
        }

        private byte[] GetNumbers(string message, byte min, byte max, int? nMin, int? nMax)
        {
            while (true)
            {
                Console.Write($"{message} ");
                List<byte> numbers = SplitNumbers(Console.ReadLine(), min, max, nMin, nMax);

                if (numbers != null)
                    return numbers.ToArray();
            }
        }

        private List<byte> SplitNumbers(string s, byte min, byte max, int? nMin, int? nMax)
        {
            var numbers = new List<byte>();

            if (s == "" && nMin == 0) // "" = empty list
                return numbers;

            foreach (var nStr in s.Split(','))
            {
                if (byte.TryParse(nStr, out byte n)) // single number
                {
                    if (n < min || n > max)
                        return null;

                    numbers.Add(n);
                }
                else if (nStr.Contains("-")) // range (5-8)
                {
                    var nStrRange = nStr.Split("-");
                    if (nStrRange.Length != 2)
                        return null;

                    if (!byte.TryParse(nStrRange[0], out byte nFrom) || !byte.TryParse(nStrRange[1], out byte nTo))
                        return null;

                    if (nFrom < min || nTo > max)
                        return null;

                    for (byte i = nFrom; i <= nTo; i++)
                        numbers.Add(i);
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