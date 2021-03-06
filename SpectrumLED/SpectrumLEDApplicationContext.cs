﻿using SpectrumLED.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        
        // The color input file.
        // Keyboard colors are by row, top to bottom. The alpha is set programmatically so it's
        // left at 0. We use uint to get an unsigned 32-bit integer that we can break into its
        // constituent bytes.
        // Scaling the gradient based on LOGI_LED_BITMAP_HEIGHT left to the reader
        public const string COLOR_FILE_NAME = "colors.txt";

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

            MenuItem spectrumMenu = new MenuItem("Spectrum");
            ReadColorOptions().ForEach(colorOpt => spectrumMenu.MenuItems.Add(
                    new MenuItem(colorOpt.Item1, (s, e) => ColorOption_Click(s, colorOpt.Item2))));

            systrayIcon = new NotifyIcon();
            systrayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                spectrumMenu,
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
         * Ugly but I'll take it.
         */
        private List<Tuple<string, uint[]>> ReadColorOptions()
        {
            List<Tuple<string, uint[]>> list = new List<Tuple<string, uint[]>>();

            StreamReader reader = new StreamReader(COLOR_FILE_NAME);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] opts = line.Split('=');
                list.Add(new Tuple<string, uint[]>(opts[0], opts[1].Split(',')
                        .Select(c => Convert.ToUInt32(c, 16)).ToArray()));
            }

            return list;
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