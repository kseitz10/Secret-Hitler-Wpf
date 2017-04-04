using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SecretHitler.App.Utility;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for connecting to or configuring a server.
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HomeViewModel()
        {
            Hostname = Properties.Settings.Default.LastHostname;
            PortNumber = Properties.Settings.Default.LastPort;
            Nickname = Properties.Settings.Default.LastNickname;
        }

        private Guid _clientGuid = Guid.NewGuid();
        private RelayCommand _startCommand;
        private string _hostname;
        private ushort _portNumber;
        private string _nickname;
        private bool _isHost;
        private bool _rememberMe;

        /// <summary>
        /// The hostname or IP of the server being connected to. If hosting a local server, this is ignored.
        /// </summary>
        public string Hostname
        {
            get => _hostname;
            set
            {
                _hostname = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The port number of the server being connected to. If hosting a local server, this is ignored.
        /// </summary>
        public ushort PortNumber
        {
            get => _portNumber;
            set
            {
                _portNumber = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The user's preferred nickname.
        /// </summary>
        public string Nickname
        {
            get => _nickname;
            set
            {
                _nickname = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Upon connecting, save the connection information.
        /// </summary>
        public bool RememberMe
        {
            get => _rememberMe;
            set
            {
                _rememberMe = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Start a server
        /// </summary>
        public bool IsHost
        {
            get => _isHost;
            set
            {
                _isHost = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Command to start the game.
        /// </summary>
        public RelayCommand StartCommand => _startCommand ?? (_startCommand = new RelayCommand(async () => await StartAsync(), () => CanStart));

        /// <summary>
        /// Whether or not the start command can be executed.
        /// </summary>
        public bool CanStart => !string.IsNullOrWhiteSpace(Hostname) && !string.IsNullOrWhiteSpace(Nickname) && PortNumber > 0;

        /// <summary>
        /// Join/host a session.
        /// </summary>
        public async Task StartAsync()
        {
            if (RememberMe)
            {
                Properties.Settings.Default.LastHostname = Hostname;
                Properties.Settings.Default.LastPort = PortNumber;
                Properties.Settings.Default.LastNickname = Nickname;
                Properties.Settings.Default.Save();
            }

            if (IsHost)
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                Process.Start(Path.Combine(Path.GetDirectoryName(path), "SecretHitler.Server.exe"));
                await Task.Delay(1000);
            }

            var client = new Client($"http://{Hostname}:{PortNumber}", Nickname, _clientGuid);
            if (await client.ConnectAsync())
            {
                var gameVm = new GameSurfaceViewModel(client);
                ShellViewModel.Instance.UpdateMainRegion(gameVm);
            }
        }
    }
}
