using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using SecretHitler.Game.Interfaces;
using SecretHitler.App.Interfaces;
using GalaSoft.MvvmLight.CommandWpf;
using System;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for playing the game.
    /// </summary>
    public class GameSurfaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Constructor for the game surface viewmodel.
        /// </summary>
        /// <param name="client">Client class used to coordinate with the server.</param>
        public GameSurfaceViewModel(IClient client)
        {
            Client = client;
        }

        private IClient _client;
        private ObservableCollection<IPlayerInfo> _players = new ObservableCollection<IPlayerInfo>();
        private string _messages = string.Empty;
        private string _messageToSend = string.Empty;
        private RelayCommand _sendMessageCommand;

        #region Properties

        private IClient Client
        {
            get => _client;
            set
            {
                if (_client != null)
                {
                    _client.MessageReceived -= OnMessageReceived;
                }

                _client = value;

                if (_client != null)
                {
                    _client.MessageReceived += OnMessageReceived;
                }
            }
        }

        /// <summary>
        /// The players in the lobby/game.
        /// </summary>
        public ObservableCollection<IPlayerInfo> Players => _players;

        /// <summary>
        /// Gets the messages that have been received.
        /// </summary>
        public string Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Message text to be sent to other clients.
        /// </summary>
        public string MessageToSend
        {
            get => _messageToSend;
            set
            {
                _messageToSend = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageToSend))
                return;

            Client.SendMessage(MessageToSend);
            MessageToSend = string.Empty;
        }

        #region Event Handlers

        private void OnMessageReceived(string message)
        {
            Messages = string.Concat(Messages, Environment.NewLine, message);
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to send the message from <see cref="MessageToSend"/> to other clients.
        /// </summary>
        public RelayCommand SendMessageCommand => _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(SendMessage));

        #endregion
    }
}
