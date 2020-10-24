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
        private byte[] _midiChannelsIn;
        private byte[] _midiChannelsOut;
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

        public void StartRoundRobin(InputMidiPortInfo midiFrom, OutputMidiPortInfo midiTo, byte[] midiChannelsIn, byte[] midiChannelsOut)
        {
            var access = MidiAccessManager.Default;
            _midiInput = access.OpenInputAsync(midiFrom.Id).Result;
            _midiOutput = access.OpenOutputAsync(midiTo.Id).Result;
            _midiChannelsIn = midiChannelsIn;
            _midiChannelsOut = midiChannelsOut;
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
            var eventType = e.Data[0];
            var eventTypeNormalized = GetEventTypeNormalized(eventType);

            if (eventTypeNormalized == null) return;

            var inputChannel = (byte)(eventType - eventTypeNormalized.Value + 1);

            // note on
            if (eventTypeNormalized == MidiEvent.NoteOn)
            {
                var note = e.Data[1];
                var velocity = e.Data[2];

                if (_noteChannel.ContainsKey(note))
                    return; // note already pressed. Nothing to do

                byte outputChannel;

                if (_midiChannelsIn.Contains(inputChannel)) // input channel round robin to the output channels
                {
                    var busyChannels = _noteChannel.Values;
                    var freeChannels = _midiChannelsOut.Where(x => !busyChannels.Contains(x)).ToArray();

                    outputChannel = _midiChannelsOut[_channelIndex % _midiChannelsOut.Length];
                    if (busyChannels.Contains(outputChannel) && freeChannels.Count() > 0)
                    {
                        outputChannel = freeChannels[_channelIndex % freeChannels.Length];
                    }

                    _channelIndex++;
                }
                else // input channel excluded from round robin: echoing the message on the same channel
                {
                    outputChannel = inputChannel;
                }

                var dataToSend = new byte[] { (byte)(MidiEvent.NoteOn + outputChannel - 1), note, velocity };
                _midiOutput.Send(dataToSend, 0, dataToSend.Length, 0);

                _noteChannel[note] = outputChannel;
            }
            // note off
            else if (eventTypeNormalized == MidiEvent.NoteOff)
            {
                var note = e.Data[1];
                var velocity = e.Data[2];

                if (!_noteChannel.ContainsKey(note))
                    return; // note already stopped. Nothing to do

                var outputChannel = _noteChannel[note];
                _noteChannel.Remove(note);

                var dataToSend = new byte[] { (byte)(MidiEvent.NoteOff + outputChannel - 1), note, velocity };
                _midiOutput.Send(dataToSend, 0, dataToSend.Length, 0);
            }
            // clock: forwarding
            else if (eventTypeNormalized == MidiEvent.MidiClock)
            {
                _midiOutput.Send(e.Data, 0, e.Data.Length, 0);
            }
            // other type of events: forwarding on every channel
            else
            {
                var dataToSend = e.Data.ToArray();

                // 1->N channels
                foreach (var channel in _midiChannelsOut)
                {
                    dataToSend[0] = (byte)(eventTypeNormalized + channel - 1);
                    _midiOutput.Send(dataToSend, 0, dataToSend.Length, 0);
                }
            }
        }

        private byte? GetEventTypeNormalized(byte eventType)
        {
            // events with channel
            if (CheckEventType(eventType, MidiEvent.NoteOn)) return MidiEvent.NoteOn;
            if (CheckEventType(eventType, MidiEvent.NoteOff)) return MidiEvent.NoteOff;
            if (CheckEventType(eventType, MidiEvent.Pitch)) return MidiEvent.Pitch;
            if (CheckEventType(eventType, MidiEvent.CAf)) return MidiEvent.CAf;
            if (CheckEventType(eventType, MidiEvent.CC)) return MidiEvent.CC;
            if (CheckEventType(eventType, MidiEvent.Program)) return MidiEvent.Program;

            // events without channel
            if (eventType == MidiEvent.MidiClock) return MidiEvent.MidiClock;

            return null; // not handled
        }

        private bool CheckEventType(byte eventType, byte eventTypeToCheck)
        {
            return (eventType >= eventTypeToCheck && eventType <= eventTypeToCheck + 15);
        }

        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
