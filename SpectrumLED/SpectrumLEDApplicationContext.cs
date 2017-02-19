using SpectrumLED.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpectrumLED
{

    /*
     * Main application context. SpectrumLED only runs in the systray (no forms). The icon is the
     * main interaction with the functionality: enable/disable on click, right click for context
     * menu to set options and exit.
     */
    public class SpectrumLEDApplicationContext : ApplicationContext
    {


        // Keyboard color options by row, top to bottom. The alpha is set programmatically so it's
        // left at 0. We use uint to get an unsigned 32-bit integer that we can break into its
        // constituent bytes.
        // Scaling the gradient based on LOGI_LED_BITMAP_HEIGHT left to the reader
        public static readonly uint[] FIRE_ARGB = { 0xFFFF00, 0xFFCC00, 0xFF9900, 0xFF6600, 0xFF3300, 0xFF0000 };
        public static readonly uint[] FIRE_INV_ARGB = { 0xFF0000, 0xFF3300, 0xFF6600, 0xFF9900, 0xFFCC00, 0xFFFF00 };
        public static readonly uint[] ICE_ARGB = { 0xFFFFFF, 0xCCCCFF, 0x9999FF, 0x6666FF, 0x3333FF, 0x0000FF };
        public static readonly uint[] ICE_INV_ARGB = { 0x0000FF, 0x3333FF, 0x6666FF, 0x9999FF, 0xCCCCFF, 0xFFFFFF };

        // Refresh rate options
        public const double SLOW_MS = 1000 / 8.0;
        public const double MED_MS = 1000 / 16.0;
        public const double FAST_MS = 1000 / 30.0;
        public const double FULL_MS = 1000 / 60.0;

        // The systray icon and main app control
        private NotifyIcon systrayIcon;
        private SpectrumApp spectrumLED;

        // Master enabled state
        private Boolean enabled = false;

        /*
         * Set up the application. Configures the main app handler, creates and initializes the
         * systray icon and its context menu, and makes the icon visible.
         */
        public SpectrumLEDApplicationContext()
        {
            spectrumLED = new SpectrumApp();
            spectrumLED.Initialize();

            systrayIcon = new NotifyIcon();
            systrayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Spectrum", new MenuItem[] {
                    new MenuItem("Fire", (s, e) => ColorOption_Click(s, FIRE_ARGB)),
                    new MenuItem("Fire (Inverse)", (s, e) => ColorOption_Click(s, FIRE_INV_ARGB)),
                    new MenuItem("Ice", (s, e) => ColorOption_Click(s, ICE_ARGB)),
                    new MenuItem("Ice (Inverse)", (s, e) => ColorOption_Click(s, ICE_INV_ARGB))
                }),
                new MenuItem("Update Speed", new MenuItem[] {
                    new MenuItem("Chill (8Hz)", (s, e) => UpdateSpeed_Click(s, SLOW_MS)),
                    new MenuItem("Medium (16Hz)", (s, e) => UpdateSpeed_Click(s, MED_MS)),
                    new MenuItem("Fast (30Hz)", (s, e) => UpdateSpeed_Click(s, FAST_MS)),
                    new MenuItem("Full (60Hz)", (s, e) => UpdateSpeed_Click(s, FULL_MS))
                }),
                new MenuItem("Exit SpectrumLED", OnApplicationExit)
            });
            systrayIcon.ContextMenu.MenuItems[0].MenuItems[0].Checked = true;
            systrayIcon.ContextMenu.MenuItems[1].MenuItems[0].Checked = true;
            systrayIcon.MouseClick += SystrayIcon_Click;
            systrayIcon.Icon = Icon.FromHandle(Resources.SpectrumLED.GetHicon());
            systrayIcon.Text = "SpectrumLED";
            systrayIcon.Visible = true;
        }

        /*
         * Application exit callback handler. Properly dispose of device capture. Set the systray
         * icon to false to keep it from hanging around until the user mouses over it.
         */
        public void OnApplicationExit(object sender, EventArgs e)
        {
            spectrumLED.Shutdown(sender, e);
            systrayIcon.Visible = false;
            Application.Exit();
        }

        /*
         * Left click callback handler. Enables/disables.
         */
        private void SystrayIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                enabled = !enabled;
                spectrumLED.SetEnabled(enabled);
            }
        }

        /*
         * Color options callback handler. Sets the current color spectrum.
         */
        private void ColorOption_Click(object sender, uint[] color)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            spectrumLED.SetSpectrum(color);
        }

        /*
         * Speed options callback handler. Sets the tick/render speed in the app.
         */
        private void UpdateSpeed_Click(object sender, double intervalMs)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            spectrumLED.UpdateTickSpeed(intervalMs);
        }

        // The definition of self-documenting code. Does this comment negate that?
        private void CheckMeAndUncheckSiblings(MenuItem me)
        {
            foreach (MenuItem child in me.Parent.MenuItems)
            {
                child.Checked = child == me;
            }
        }

    }
}