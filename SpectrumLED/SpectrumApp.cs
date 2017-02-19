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
        private IWaveSource finalSource;

        // An attempt at only-just-post-stream-of-consciousness code organization
        private SampleHandler sampleHandler;
        private KeyboardHandler keyboardHandler;

        /*
         * Initialize audio capture and our sample and keyboard handlers. No audio is read until
         * SetEnable(true) is called.
         */
        public void Initialize()
        {
            // Init the timer
            ticker = new Timer(SpectrumLEDApplicationContext.SLOW_MS);
            ticker.Elapsed += Tick;

            // Init audio capture
            capture = new WasapiLoopbackCapture();
            capture.Initialize();

            // Init sample and keyboard handlers
            sampleHandler = new SampleHandler(capture.WaveFormat.Channels, capture.WaveFormat.SampleRate);
            keyboardHandler = new KeyboardHandler();

            // Configure per-block reads rather than per-sample reads
            var notificationSource = new SingleBlockNotificationStream(new SoundInSource(capture).ToSampleSource());
            notificationSource.SingleBlockRead += (s, e) => sampleHandler.Add(e.Left, e.Right);

            finalSource = notificationSource.ToWaveSource();
            capture.DataAvailable += (s, e) => finalSource.Read(e.Data, e.Offset, e.ByteCount);
        }

        /*
         * Enable or disable audio capture and keyboard control.
         */
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                capture.Start();
                keyboardHandler.Connect();
                ticker.Start();
            }
            else
            {
                ticker.Stop();
                keyboardHandler.Disconnect();
                capture.Stop();
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
            ticker.Stop();
            keyboardHandler.Disconnect();
            if (!(capture.RecordingState == RecordingState.Stopped))
            {
                capture.Stop();
            }
            capture.Dispose();
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

    }
}