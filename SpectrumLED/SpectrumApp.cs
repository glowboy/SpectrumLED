using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Timers;

namespace SpectrumLED
{

    /*
     * The real meat of the app, outside all that Windows junk. Handles audio device initialization,
     * data capture, and keyboard rendering.
     */
    public class SpectrumApp
    {

        // Our ticker that triggers keyboard re-rendering. User-controllable via the systray menu
        private Timer ticker;

        // CSCore classes that read the WASAPI data and pass it to the SampleHandler
        private WasapiCapture capture;
        private SingleBlockNotificationStream notificationSource;
        private IWaveSource finalSource;

        // An attempt at only-just-post-stream-of-consciousness code organization
        private SampleHandler sampleHandler;
        private KeyboardHandler keyboardHandler;

        /*
         * Basic initialization. No audio is read until SetEnable(true) is called.
         */
        public SpectrumApp()
        {
            // Init the timer
            ticker = new Timer(SpectrumLEDApplicationContext.SLOW_MS);
            ticker.Elapsed += Tick;

            // Create keyboard handler
            keyboardHandler = new KeyboardHandler();
        }

        /*
         * Enable or disable audio capture and keyboard control.
         */
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                StartCapture();
                keyboardHandler.Connect();
                ticker.Start();
            }
            else
            {
                ticker.Stop();
                keyboardHandler.Disconnect();
                StopCapture();
            }
        }

        /*
         * Pass the given color through to the keyboard handler.
         */
        public void SetSpectrum(uint[] spectrum)
        {
            keyboardHandler.SetSpectrum(spectrum);
        }

        /*
         * Update the timer tick speed, which updates the FFT and keyboard rendering speeds.
         */
        public void UpdateTickSpeed(double intervalMs)
        {
            // No need to stop/start, setting Interval does that
            ticker.Interval = intervalMs;
        }

        /*
         * Cleanly release audio and keyboard resources.
         */
        public void Shutdown(object sender, EventArgs e)
        {
            SetEnabled(false);
        }

        /*
         * Ticker callback handler. Performs the actual FFT, massages the data into raw spectrum
         * data, and sends it to the keyboard handler.
         */
        private void Tick(object sender, ElapsedEventArgs e)
        {
            // Get the FFT results and send to KeyboardHandler
            float[] values = sampleHandler.GetSpectrumValues();
            if (values != null)
            {
                keyboardHandler.RenderSpectrum(values);
            }
        }

        /*
         * Begin audio capture. Connects to WASAPI, initializes the sample handler, and begins
         * sending captured data to it.
         */
        private void StartCapture()
        {
            // Initialize hardware capture
            capture = new WasapiLoopbackCapture();
            capture.Initialize();

            // Init sample handler
            sampleHandler = new SampleHandler(capture.WaveFormat.Channels, capture.WaveFormat.SampleRate);

            // Configure per-block reads rather than per-sample reads
            notificationSource = new SingleBlockNotificationStream(new SoundInSource(capture).ToSampleSource());
            notificationSource.SingleBlockRead += (s, e) => sampleHandler.Add(e.Left, e.Right);

            finalSource = notificationSource.ToWaveSource();
            capture.DataAvailable += (s, e) => finalSource.Read(e.Data, e.Offset, e.ByteCount);

            // Start capture
            capture.Start();
        }

        /*
         * Stop the audio capture, if currently recording. Properly disposes member objects.
         */
        private void StopCapture()
        {
            if (capture.RecordingState == RecordingState.Recording)
            {
                capture.Stop();

                finalSource.Dispose();
                notificationSource.Dispose();
                capture.Dispose();

                sampleHandler = null;
            }
        }

    }
}