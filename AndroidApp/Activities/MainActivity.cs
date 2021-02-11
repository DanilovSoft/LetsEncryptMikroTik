using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LetsEncryptMikroTik.Core;

namespace AndroidApp.Activities
{
    [Activity(Label = "MainActivity", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private Button button1;
        private TextView textView_status;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            // Create your application here

            button1 = FindViewById<Button>(Resource.Id.button1);
            textView_status = FindViewById<TextView>(Resource.Id.textView_status);

            button1.Click += Button1_Click;

            var cert = new X509Certificate2();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Test();
        }

        private async void Test()
        {
            var config = new ConfigClass
            {
                DomainName = "where.now.im",
                Email = "vitaliy931@gmail.com",
                WanIface = "ether1-wan",
                MikroTikAddress = "10.0.0.1",
                MikroTikPort = 8728,
                FtpLogin = "ftp",
                FtpPassword = "",
                MikroTikLogin = "certes",
                MikroTikPassword = "",
                UseAlpn = true,
                Force = true,
                LetsEncryptAddress = LeUri.StagingV2,
            };

            var p = new LetsEncryptMikroTik.Core.Program(config);
            
            try
            {
                await Task.Run(() => p.RunAsync(false, new LogSink(this)));
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }

        }

        private sealed class LogSink : InMemorySink
        {
            private readonly MainActivity _activity;

            public LogSink(MainActivity activity)
            {
                _activity = activity;
            }

            public override void NewEntry(string message)
            {
                if (Looper.MyLooper() != Looper.MainLooper)
                {
                    _activity.RunOnUiThread(() => 
                    {
                        _activity.textView_status.Text = message;
                    });
                }
                else
                {
                    _activity.textView_status.Text = message;
                }
            }
        }
    }
}