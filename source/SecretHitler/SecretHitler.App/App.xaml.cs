using CommandLine;
using SecretHitler.App.ViewModels;
using System;
using System.Windows;

namespace SecretHitler.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = new StartupOptions();
            var parser = new Parser();
            if (parser.ParseArguments(e.Args, options) &&
                !string.IsNullOrEmpty(options.Hostname) &&
                !string.IsNullOrEmpty(options.Nickname) && options.Port.HasValue)
            {
                var home = (HomeViewModel)ShellViewModel.Instance.MainRegion;
                home.PortNumber = Convert.ToUInt16(options.Port.Value);
                home.Hostname = options.Hostname;
                home.Nickname = options.Nickname;
                await home.StartAsync();
            }
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
    }
}
