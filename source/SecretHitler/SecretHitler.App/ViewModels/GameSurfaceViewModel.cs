using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SecretHitler.App.Interfaces;
using System;
using System.Collections.Generic;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using System.Threading.Tasks;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for playing the game.
    /// </summary>
    public class GameSurfaceViewModel : ViewModelBase, IPlayerLogic
    {
        /// <summary>
        /// Constructor for the game surface viewmodel.
        /// </summary>
        /// <param name="client">SignalR class used to coordinate with the server.</param>
        public GameSurfaceViewModel(IClient client)
        {
            Client = client;
        }

        private List<IPlayerInfo> _players = new List<IPlayerInfo>();
        private string _messages = string.Empty;
        private string _messageToSend = string.Empty;
        private RelayCommand _sendMessageCommand;

        #region Properties

        private IClient Client { get; set; }

        /// <summary>
        /// The players in the lobby/game.
        /// </summary>
        public List<IPlayerInfo> Players
        {
            get { return _players; }
            private set
            {
                _players = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the messages that have been received.
        /// </summary>
        public string Messages
        {
            get => _messages;
            private set
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

        public void UpdatePlayerStates(IEnumerable<IPlayerInfo> playerData)
        {
            Players = new List<IPlayerInfo>(playerData);
        }

        public void UpdateLoyalty(PlayerRole role)
        {
            throw new NotImplementedException();
        }

        public void MessageReceived(string message)
        {
            Messages = string.Concat(Messages, Environment.NewLine, message);
        }

        public Task<Guid> SelectPlayer(GameState gameState, IEnumerable<Guid> candidates)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetVote()
        {
            throw new NotImplementedException();
        }

        public Task<IList<PolicyType>> SelectPolicies(IList<PolicyType> drawnPolicies, int allowedCount)
        {
            throw new NotImplementedException();
        }

        public void ShowPolicies(IList<PolicyType> deckTopThree)
        {
            throw new NotImplementedException();
        }

        public void RevealLoyalty(PlayerRole role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PromptForVetoApproval()
        {
            throw new NotImplementedException();
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
