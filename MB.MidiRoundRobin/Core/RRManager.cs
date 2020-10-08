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

        public IList<OutputMidiPortInfo> EnumerateMidiOutputs()
        {
            var access = MidiAccessManager.Default;
            return access.Outputs.ToArray().Select(x => new OutputMidiPortInfo { Id = x.Id, Description = x.Name }).ToArray();
        }

        public IList<InputMidiPortInfo> EnumerateMidiInputs()
        {
            var access = MidiAccessManager.Default;
            return access.Inputs.ToArray().Select(x => new InputMidiPortInfo { Id = x.Id, Description = x.Name }).ToArray();
        }

        public void StartRoundRobin(InputMidiPortInfo midiFrom, OutputMidiPortInfo midiTo, byte[] channels)
        {
            var access = MidiAccessManager.Default;
            _midiInput = access.OpenInputAsync(midiFrom.Id).Result;
            _midiOutput = access.OpenOutputAsync(midiTo.Id).Result;
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

            var eventType = e.Data[0];

            if (eventType >= MidiEvent.NoteOn && eventType <= MidiEvent.NoteOn + 15)
            {
                var note = e.Data[1];
                var velocity = e.Data[2];

                if (_noteChannel.ContainsKey(note))
                    return; // note already pressed. Nothing to do

                var busyChannels = _noteChannel.Values;
                var freeChannels = _channels.Where(x => !busyChannels.Contains(x)).ToArray();

                var outputChannel = _channels[_channelIndex % _channels.Length];
                if (busyChannels.Contains(outputChannel) && freeChannels.Count() > 0)
                {
                    outputChannel = freeChannels[_channelIndex % freeChannels.Length];
                }

                var dataToSend = new byte[] { (byte)(MidiEvent.NoteOn + outputChannel - 1), note, velocity };
                _midiOutput.Send(dataToSend, 0, 3, 0);

                _noteChannel[note] = outputChannel;

                _channelIndex++;
            }
            else if (eventType >= MidiEvent.NoteOff && eventType <= MidiEvent.NoteOff + 15)
            {
                var note = e.Data[1];
                var velocity = e.Data[2];

                if (!_noteChannel.ContainsKey(note))
                    return; // note already stopped. Nothing to do

                var outputChannel = _noteChannel[note];
                _noteChannel.Remove(note);

                var dataToSend = new byte[] { (byte)(MidiEvent.NoteOff + outputChannel - 1), note, velocity };
                _midiOutput.Send(dataToSend, 0, 3, 0);
            }
            else if (eventType >= MidiEvent.Pitch && eventType <= MidiEvent.Pitch + 15)
            {
                // 1->N channels
                foreach (var channel in _channels)
                {
                    var dataToSend = new byte[] { (byte)(MidiEvent.Pitch + channel - 1), e.Data[1], e.Data[2] };
                    _midiOutput.Send(dataToSend, 0, 3, 0);
                }
            }
            else if (eventType >= MidiEvent.CC && eventType <= MidiEvent.CC + 15)
            {
                //Console.WriteLine($"[{string.Join(", ", data)}] {e.Start} - {e.Length} - {e.Timestamp}");

                // 1->N channels
                foreach (var channel in _channels)
                {
                    var dataToSend = new byte[] { (byte)(MidiEvent.CC + channel - 1), e.Data[1], e.Data[2] };
                    _midiOutput.Send(dataToSend, 0, 3, 0);
                }
            }
        }

        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
