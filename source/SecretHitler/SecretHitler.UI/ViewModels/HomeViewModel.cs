using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using SecretHitler.UI.Interfaces;
using SecretHitler.UI.Utility;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel for connecting to or configuring a server.
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        private readonly IAppSettings _settings;
        private readonly IShell _shell;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HomeViewModel(IAppSettings settings, IShell shell)
        {
            _settings = settings;
            _shell = shell;
            Hostname = settings.LastHostname;
            PortNumber = settings.LastPort;
            Nickname = settings.LastNickname;
        }

        private readonly Guid _clientGuid = Guid.NewGuid();
        private RelayCommand _startCommand;
        private string _hostname;
        private ushort _portNumber;
        private string _nickname;
        private bool _isHost;
        private bool _rememberMe;
        private bool _isBusy;

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
        /// Whether or not is busy connecting.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
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
                _settings.LastHostname = Hostname;
                _settings.LastPort = PortNumber;
                _settings.LastNickname = Nickname;
                _settings.Save();
            }

            var client = new Client($"https://{Hostname}:{PortNumber}/hub", Nickname, _clientGuid);

            try
            {
                IsBusy = true;
                if (await client.ConnectAsync())
                {
                    var gameVm = new GameSurfaceViewModel(_clientGuid, client);
                    client.ClientUI = gameVm;
                    _shell.UpdateMainRegion(gameVm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect. {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
