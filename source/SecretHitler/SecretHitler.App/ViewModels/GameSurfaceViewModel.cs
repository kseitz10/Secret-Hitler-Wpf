using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SecretHitler.App.Interfaces;
using System;
using System.Collections.Generic;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using SecretHitler.Game.Entities;
using System.Linq;
using SecretHitler.Game.Utility;

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
        /// <param name="myGuid">Guid for this client. Used to look up my player information.</param>
        /// <param name="client">SignalR class used to coordinate with the server.</param>
        public GameSurfaceViewModel(Guid myGuid, IClient client)
        {
            Client = client;
            _myGuid = myGuid;
        }

        private Guid _myGuid;
        private string _messages = string.Empty;
        private string _messageToSend = string.Empty;
        private RelayCommand _sendMessageCommand;
        private GameData _gameData;

        #region Properties

        private IClient Client { get; set; }

        /// <summary>
        /// This player's data.
        /// </summary>
        public IPlayerInfo Me => Players.FirstOrDefault(_ => _.Identifier == _myGuid);

        /// <summary>
        /// The players in the game.
        /// </summary>
        public IEnumerable<IPlayerInfo> Players => _gameData?.Players ?? new List<PlayerData>();

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

        private void Dispatch(Action callback)
        {
            // TODO Better dispatching. Where's CheckAccess?
            Application.Current.Dispatcher.Invoke(callback);
        }

        #region Event Handlers

        public void UpdateGameData(GameData gameData)
        {
            Dispatch(() =>
            {
                // New game clears all history.
                if (gameData?.GameGuid != _gameData?.GameGuid)
                    _gameData = null;

                // This shouldn't happen...
                if (gameData == null)
                    return;

                var oldRole = Me?.Role;

                _gameData = gameData;

                if (Me.Role != oldRole)
                {
                    MessageReceived("Your role in this game is: " + Me.Role);
                    MessageReceived("The new presidential rotation is as follows: " +
                        string.Join(", ", gameData.PresidentialQueue.Select(guid => gameData.Players.First(p => p.Identifier == guid).Name)));
                }

                RaisePropertyChanged(nameof(Players));
                RaisePropertyChanged(nameof(Me));
            });
        }

        public void MessageReceived(string message)
        {
            Dispatch(() => Messages = string.Concat(Messages, Environment.NewLine, message));
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
