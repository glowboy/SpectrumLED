using LedCSharp;
using System;
using System.Linq;

namespace SpectrumLED
{
    public class KeyboardHandler
    {

        // Alpha values for keys that are "on" and "off"
        const byte ALPHA_ON = (byte)255;
        const byte ALPHA_OFF = (byte)70;

        // During normalization, scale the FFT values by the maximum value seen to get nice,
        // mostly-mid-ranged values. Reduce the maximum ever seen with each tick so giant spikes
        // don't make the pretty colors disappear
        const float MAX_ENTROPY = 0.9999f;
        float maxSeenEver = 0;

        uint[] spectrum = SpectrumLEDApplicationContext.FIRE_ARGB;

        /*
         * Connect to the Logitech SDK, setting the target devices to PERKEY_RGB only.
         * Exercise left for the reader: allow monochrome devices to work too.
         * Exercise 2: get this to work on RGB and monochrome mice.
         */
        public void Connect()
        {
            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
        }

        /*
         * Disconnect from the SDK.
         */
        public void Disconnect()
        {
            LogitechGSDK.LogiLedShutdown();
        }

        /*
         * Set the current color spectrum.
         */
        public void SetSpectrum(uint[] spectrum)
        {
            this.spectrum = spectrum;
        }

        /*
         * Here's the raw (massaged) FFT'ed spectrum data. Create a bitmap from it and send it to
         * the keyboard.
         */
        public void RenderSpectrum(float[] raw)
        {
            float[] normalized = Normalize(raw);
            byte[] bmp = CreateBitmap(normalized);
            LogitechGSDK.LogiLedSetLightingFromBitmap(bmp);
        }

        /*
         * Normalize the raw data into values between 0 and the height of the keyboard based on the
         * maximum raw value ever seen. The max value is subject to entropy so large spikes don't
         * ruin the cool.
         */
        private float[] Normalize(float[] raw)
        {
            float[] normalized = new float[raw.Length];

            // Use maxSeenEver to normalize the range into 0-6
            maxSeenEver = Math.Max(raw.Max(), maxSeenEver);

            for (int i = 0; i < raw.Length; i++)
            {
                normalized[i] = raw[i] / maxSeenEver * LogitechGSDK.LOGI_LED_BITMAP_HEIGHT;
            }

            // Slowly decrease maxEverSeen to keep things normalizing after a giant spike
            maxSeenEver *= MAX_ENTROPY;

            return normalized;
        }

        /*
         * Turn the normalized spectrum data into a BGRA-blocked bitmap. A key is either "on"
         * (alpha 255) or "off" (alpha 70) based on whether the normalized value reaches it.
         */
        private byte[] CreateBitmap(float[] normalized)
        {
            byte[] bitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
            uint[] spectrumLocal = spectrum;

            for (int i = 0; i < LogitechGSDK.LOGI_LED_BITMAP_SIZE; i += LogitechGSDK.LOGI_LED_BITMAP_BYTES_PER_KEY)
            {
                int rowFromTop = i / LogitechGSDK.LOGI_LED_BITMAP_BYTES_PER_KEY / LogitechGSDK.LOGI_LED_BITMAP_WIDTH;
                int colFromLeft = i / LogitechGSDK.LOGI_LED_BITMAP_BYTES_PER_KEY % LogitechGSDK.LOGI_LED_BITMAP_WIDTH;
                byte alpha = normalized[colFromLeft] > (LogitechGSDK.LOGI_LED_BITMAP_HEIGHT - 1 - rowFromTop)
                        ? ALPHA_ON : ALPHA_OFF;

                byte[] bgra = BitConverter.GetBytes(spectrumLocal[rowFromTop]); // swaps order
                bitmap[i] = bgra[0];
                bitmap[i + 1] = bgra[1];
                bitmap[i + 2] = bgra[2];
                bitmap[i + 3] = (byte)(alpha);
            }

            return bitmap;
        }

    }
}