using CommandLine;
using SecretHitler.App.ViewModels;
using System;
using System.Windows;

using SecretHitler.Application.LegacyEngine;
using SecretHitler.UI.Utility;
using SecretHitler.UI.ViewModels;

namespace SecretHitler.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await Parser.Default.ParseArguments<StartupOptions>(e.Args).MapResult(async opts =>
            {
                if (!string.IsNullOrEmpty(opts.Hostname) &&
                    !string.IsNullOrEmpty(opts.Nickname) &&
                    opts.Port.HasValue)
                {
                    if (opts.UseAi)
                    {
                        var id = Guid.NewGuid();
                        var client = new Client($"https://{opts.Hostname}:{opts.Port}/hub", opts.Nickname, id);

                        try
                        {
                            if (await client.ConnectAsync())
                            {
                                var ai = new SimpleAI();
                                client.ClientUI = ai;
                                MainWindow.WindowState = WindowState.Minimized;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Could not connect. {ex.Message}");
                        }
                    }
                    else
                    {
                        var home = (HomeViewModel)ShellViewModel.Instance.MainRegion;
                        home.PortNumber = Convert.ToUInt16(opts.Port.Value);
                        home.Hostname = opts.Hostname;
                        home.Nickname = opts.Nickname;
                        await home.StartAsync();
                    }
                }
            }, err => throw new Exception());
        }
    }

    /// <summary>
    /// Command-line arguments.
    /// </summary>
    public class StartupOptions
    {
        [Option('h', "hostname", HelpText = "DNS or IP address")]
        public string Hostname { get; set; }

        [Option('p', "port", HelpText = "Port number")]
        public int? Port { get; set; }

        [Option('n', "nickname", HelpText = "Nickname")]
        public string Nickname { get; set; }

        [Option('a', "ai", HelpText = "Use automated player for testing purposes")]
        public bool UseAi { get; set; }
    }
}
