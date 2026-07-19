# Universal Game Controller Profiler
After many years of developing drivers and tools for the Saitek series I am moving to a more generic approach.
Based on the XHOTAS code this new software will be able to program any HID game controller device.

## v11

### Status

- 99% Working (may have some bugs).
- C++ service code has been ported to high performance c# code. Hidden memory corruption has been fixed.

### Devices

Universal support is ready but some specific HID configurations may not work (continuous range hats).

### Driver

Windows 10/11 driver signing is now impossible to accomplish for personal/open source projects
 due to the expensive EV certificates. UGC Profiler is now based on [vJoy Driver v2.1.9.1](https://github.com/jshafer817/vJoy).
 This version is signed and works without any trick.

*X52 MFD Note:* In order to make X52 MFD programmable a specific driver is still required. To make this possible I have
use WinUSB as it can be signed with a cheaper code signing certificate.

It is not a requirement but [HidHide](https://github.com/ViGEm/HidHide) or similar tools could be usefull to hide devices
in games so only vJoy controllers are shown.

### Profiler

This is a work in process. Actually most of the XHOTAS driver is used. But the objetive is to make gui even more flexible,
keeping it easy to use, with an event based system.

The other important feature that is on the roadmap is to make the profiles script based so you can go to a lower level and
do almost anything (v12?).

The GUI has been ported to WinUI 3 (Uno/SkiaRender).
<br>
<br>

## Source code

All the source code has been moved to user layer, no WDK required. The most recent implementation use only C#/.NET.
Can be compiled with Visual Studio Community 2022.
<br>
<br>

## Language

The GUI can be translated to other languages using the xaml files included for that purpose.





