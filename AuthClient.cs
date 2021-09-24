using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;

namespace TwitchVMCAuthClient
{
    public partial class AuthClient : Form
    {
        public SemaphoreSlim signal = new SemaphoreSlim(0, 1);
        public ChromiumWebBrowser cwb;
        public string authOut;

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            cwb = new ChromiumWebBrowser("https://lunar.serealia.ca/cadyn/twitch-token/");
            this.Controls.Add(cwb);
            cwb.Dock = DockStyle.Fill;
        }

        public AuthClient()
        {
            InitializeComponent();
            InitializeChromium();
            cwb.AddressChanged += onChanged;
        }

        public async void onChanged(object sender, System.EventArgs e)
        {
            await Task.Delay(250);
            string s = await cwb.GetTextAsync();
            while (s.Length < 0)
            {
                await Task.Delay(250);
                s = await cwb.GetTextAsync();
            }

            if (s.Length > 0 && s[0].Equals('{'))
            {
                authOut = s;
                signal.Release();
            }
            return;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
