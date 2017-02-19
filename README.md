SpectrumLED
===========

Render your audio data on your Logitech RGB keyboard
----------------------------------------------------

SpectrumLED is a C# project that uses [CSCore](http://cscore.codeplex.com/) and the [Logitech LED Illumation SDK](http://gaming.logitech.com/en-us/developers) to capture and render WASAPI audio data as bars on your full-spectrum, per-key RGB Logitech keyboard (tested on an Orion Spectrum). Visualize your music even when your audio player isn't in the foreground! It runs as a simple Windows systray application and allows changing the render frequency on the fly. Please note it is sloppy and needs a lot of work. It has a completely open license (MIT) so grab it and do whatever you'd like with it.

I used nuget to get the CSCore package. Also, you must **manually copy** the LogitechLedEnginesWrapper.dll file from the SDK package into the directory where you run SpectrumLED.exe. *Thanks, Logitech*.

### Future Plans
I'd like to keep working on this, cleaning it up, commenting, and eventually making certain functionality plug-in capable. Specifically, the rendering device, the rendering algorithm, the audio input, and the systray context menu. Maybe add per-user settings read in at startup?!

Ok, maybe not all that.  But maybe changing from a yellow-red spectrum to blue-white or something cool.  *Nice*.

### Please Note
Bear in mind I am not a C# developer, but the Logitech SDK was way easier to use in C# than the Java JNI version. Ugh.