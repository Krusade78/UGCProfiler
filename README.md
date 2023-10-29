# Universal Game Controller Profiler
After many years of developing drivers and tools for the Saitek X series I am moving to a more generic approach.
Based on the XHOTAS code this new software will be able to program any HID game controller device.

## v11

### Devices

As my free time is very limited now, universal support is going to take some time so, at this moment only
four devices are available, those that I own for testing:
- Saitek X52 joystick and throttle (internally the devices are separated into two different HardwareIds).
- Saitek Pro flight rudder pedals.
- VKB Gladiator NXT.

### Driver

As Windows 10/11 driver signing have been made impossible to accomplish for personal/open source projects
 due to the expensive EV certificates, UGC Profiler is based on the [vJoy Driver v2.1.9.1](https://github.com/jshafer817/vJoy).
 This version is signed and works without any trick.

*X52 MFD Note:* In order to make X52 MFD programmable a specific driver is still required. To make this possible I have
use WinUSB as it can be signed with a normal code signing certificate.

It is not a requirement but [HidHide](https://github.com/ViGEm/HidHide) or similar tools could be usefull to hide devices
in games son only vJoy controllers need to be configured.

### Profiler

This is a work in process. Actually most of the XHOTAS driver is used. But the objetive is to make gui even more flexibe,
keeping it easy to use, with an event based system.

The other important feature that is on the roadmap is to make the profiles script based so you can go to a lower level and
do almost anything (v12?).

The gui programs will be ported to Windows App SDK in the future but at this moment the framework has too many problems.
<br>
<br>

## Source code

All the source code has been moved to user layer, no WDK required. C# .NET is used for gui and C++ for background processing to make it as fast as possible.
Can be compiled with Visual Studio Community 2022.
<br>
<br>

## Language

The original code was translated to spanish and english with external translation files. In the actual version only spanish is fully 
translated. As the gui needs a very important rework I will review english translation.

The source code (basically var names) are mostly in spanish. I'll try to rename them to english ;-)





