MidiRoundRobin
==============
_MidiRoundRobin is a command line utility that performs a round robin between MIDI channels._

![screenshot](screenshot.png)

## How it works
Suppose you play some notes on a MIDI keyboard, let's say C, D, E, F on channel 1; here's what happens:

**[MIDI keyboard]** -> C (ch **1**), D (ch **1**), E (ch **1**), F (ch **1**) -> **[MidiRoundRobin utility]** -> C (ch **1**), D (ch **2**), E (ch **3**), F (ch **1**)

As you can see, output channels are rotated (round robin). The output channels sequence can be configurated (see below).

This is useful if you whant to "spread" your notes between different MIDI channels, for example if you have three monophonic synths and you want to play polyphonic melodies.

I created this application to play with Elektron Model:Cycles, a monophonic, six trax FM groovebox: all you have to do is:

- set the same instrument on all six tracks of the Moidel:Cycles
- connect the Model:Cycles via MIDI to the PC
- launch MidiRoundRobin (MidiRR.exe) and, when asked:
	- select your midi keyboard as MIDI in, and the Model:Cycles as MIDI out
	- select the channels 1,2,3,4,5,6 as MIDI output to feed all the tracks
- play your six notes chords!

You can also set only one output channel, for a simple MIDI routing without a full blown DAW.

## Configuration
There are 3 ways to configure MidiRoundRobin:

### 1. interactive mode
The default, active if methods 2 and 3 are not used); you are asked for MIDI in/out ports and channels:
```
> MidiRR.exe
MidiRoundRobin v.2.1.0.0

MIDI input ports:
1 - Arturia KeyStep 32
2 - nanoKONTROL2
3 - Roland Digital Piano
4 - Elektron Model:Cycles
Select a MIDI input port (from 1 to 4): 1

MIDI output ports:
1 - Microsoft GS Wavetable Synth
2 - Arturia KeyStep 32
3 - nanoKONTROL2
4 - Roland Digital Piano
5 - Elektron Model:Cycles
Select a MIDI output port (from 1 to 5): 4

Select 1 or more MIDI channels to round robin (e.g. 1,3,4): 1,2,3,4,5,6

Round robin from 'Arturia KeyStep 32' to 'Roland Digital Piano' on channels 1,2,3,4,5,6.
Press [Enter] to exit.
```

### 2. configuration file
You can create a json file named **MidiRR.settings.json** to store the settings. Put it on the same fordel as the executable.  
Here's an example:
```json
// MidiRR.settings.json
{
  "midiIn": "Arturia KeyStep 32",
  "midiOut": "Elektron Model:Cycles",
  "channels": "1,2,3,4,5,6"
}
```

### 3. command line arguments
Overwrites the configuration file:
```
> MidiRR.exe --midiIn="Arturia KeyStep 32" --midiOut="Elektron Model:Cycles" --channels="1,2,3,4,5,6"
```

You can mix 1, 2 and 3 (some settings passed by argument, some in MidiRR.settings.json and the rest entered in interactive mode).

## Limitations
Only note on/off MIDI messages are currently handled - I'm working on this.

MidiRoundRobin can be used together with a DAW (e.g. Ableton Live), but the choosen in and out MIDI ports will _NOT_ be available to the DAW.

## Todo
- ~~Handle configuration files.~~
- Handle control change / program change / pitch bend / aftertouch messages
- Investigate Linux timing issues

## Environment
* .NET Core 3
* managed-midi package dependency
* tested on Windows 10 and Linux (some timing issues on Ubuntu)

## License
GNU GENERAL PUBLIC LICENSE V 3

---

Copyright (C) [Massimo Barbieri](http://www.massimobarbieri.it) 
