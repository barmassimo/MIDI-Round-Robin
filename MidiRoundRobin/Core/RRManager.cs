using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Commons.Music.Midi;

namespace MB.MidiRoundRobin.Core
{
    public class RRManager
    {
        private IMidiInput _midiInput;
        private IMidiOutput _midiOutput;
        private int _channelIndex = 0;
        private byte[] _channels;
        private IDictionary<int, byte> _noteChannel;

        public IEnumerable<KeyValuePair<string, string>> EnumerateMidiOutputs()
        {
            var access = MidiAccessManager.Default;
            return access.Outputs.ToArray().Select(x => new KeyValuePair<string, string>(x.Id, x.Name));
        }

        public IEnumerable<KeyValuePair<string, string>> EnumerateMidiInputs()
        {
            var access = MidiAccessManager.Default;
            return access.Inputs.ToArray().Select(x => new KeyValuePair<string, string>(x.Id, x.Name));
        }

        public void StartRoundRobin(string midiFromId, string midiToId, byte[] channels)
        {
            var access = MidiAccessManager.Default;
            _midiInput = access.OpenInputAsync(midiFromId).Result;
            _midiOutput = access.OpenOutputAsync(midiToId).Result;
            _channels = channels;
            _noteChannel = new Dictionary<int, byte>();

            _midiInput.MessageReceived += Input_MessageReceived;
        }

        public void StopRoundRobin()
        {
            _midiInput.CloseAsync();
            _midiOutput.CloseAsync();
        }

        private void Input_MessageReceived(object sender, MidiReceivedEventArgs e)
        {
            var data = string.Join(", ", e.Data);
            // Console.WriteLine($"[{data}]       {e.Start} - {e.Length} - {e.Timestamp}");

            var eventType = e.Data[0];

            if (eventType >= MidiEvent.NoteOn && eventType <= MidiEvent.NoteOn + 15)
            {
                var note = e.Data[1];
                var velocity = e.Data[2];

                var busyChannels = _noteChannel.Values;
                var freeChannels = _channels.Where(x => !busyChannels.Contains(x)).ToArray();

                var outputChannel = _channels[_channelIndex % _channels.Length];
                if (busyChannels.Contains(outputChannel) && freeChannels.Count() > 0)
                {
                    outputChannel = freeChannels[_channelIndex % freeChannels.Length];
                }

                // Console.WriteLine($"Sending note on to channel {outputChannel}");

                var dataToSend = new byte[] { (byte)(MidiEvent.NoteOn + outputChannel - 1), note, velocity };
                _midiOutput.Send(dataToSend, 0, 3, 0);


                _noteChannel[note] = outputChannel;

                _channelIndex++;
            }
            else if (eventType >= MidiEvent.NoteOff && eventType <= MidiEvent.NoteOff + 15)
            {
                var note = e.Data[1];
                var velocity = e.Data[2];

                var outputChannel = _noteChannel[note];
                _noteChannel.Remove(note);

                // Console.WriteLine($"Sending note off to channel {outputChannel}");

                var dataToSend = new byte[] { (byte)(MidiEvent.NoteOff + outputChannel - 1), note, velocity };
                _midiOutput.Send(dataToSend, 0, 3, 0);
            }

            if (eventType == MidiEvent.Pitch)
            {
                //var note = e.Data[1];
                //var velocity = e.Data[2];
                //
                //_midiOutput.Send(e.Data, 0, 3, 0);
            }
        }

        public string GetVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}
