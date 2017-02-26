SpectrumLED
===========

Render your audio data on your Logitech RGB keyboard
----------------------------------------------------

SpectrumLED is a C# project that uses [CSCore](http://cscore.codeplex.com/) and the [Logitech LED Illumation SDK](http://gaming.logitech.com/en-us/developers) to capture and render WASAPI audio data as bars on your full-spectrum, per-key RGB Logitech keyboard (tested on an Orion Spectrum). Visualize your music even when your audio player isn't in the foreground! It runs as a simple Windows systray application and allows changing the render frequency on the fly. Please note it is sloppy and needs a lot of work. It has a completely open license (MIT) so grab it and do whatever you'd like with it.

I used nuget to get the CSCore package. The LogitechLedEnginesWrapper.dll file from the SDK package is included in the source and copied into the build directory during build because there's no repo package for it. *Thanks, Logitech*.

In version 0.2.0, the color mappings are read in from a file, so you can experiement with new colors without rebuilding the app (just restarting). I've found that brighter colors work best, as the LED rendering is not what you see on your screen, and darker colors (closer to #000000) all sort of muddle together.

### Future Plans
I'd like to keep working on this, cleaning it up, commenting, and eventually making certain functionality plug-in capable. Specifically, the rendering device, the rendering algorithm, the audio input, and the systray context menu. Maybe add per-user settings read in at startup?!

Ok, maybe not all that.  But maybe changing from a yellow-red spectrum to blue-white or something cool.  *Nice*.

### Please Note
Bear in mind I am not a C# developer, but the Logitech SDK was way easier to use in C# than the Java JNI version. Ugh.

#### Version History
0.2.0:
- Read in color mappings from file
- Copy LogitechLedEnginesWrapper.dll to output at build

0.1.0:
- Initial release
- Windows system tray application with context menu
- Change render rate
- Change color scheme
- Start/stop with left click