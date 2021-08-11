using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using SecretHitler.Game.Entities;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.UI.Utility;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel for playing the game.
    /// </summary>
    public class GameSurfaceViewModel : ViewModelBase, IPlayerInterface
    {
        /// <summary>
        /// Constructor for the game surface viewmodel.
        /// </summary>
        /// <param name="myGuid">Guid for this client. Used to look up my player information.</param>
        /// <param name="client">SignalR class used to coordinate with the server.</param>
        public GameSurfaceViewModel(Guid myGuid, Client client)
        {
            Client = client;
            _myGuid = myGuid;
        }

        private readonly Guid _myGuid;
        private string _messages = string.Empty;
        private string _messageToSend = string.Empty;
        private RelayCommand _sendMessageCommand;
        private GameData _gameData;
        private InteractionViewModelBase _activeModal;

        #region Properties

        private Client Client { get; set; }

        /// <summary>
        /// This player's data.
        /// </summary>
        public IPlayerInfo Me => Players.FirstOrDefault(_ => _.Identifier == _myGuid);

        /// <summary>
        /// The players in the game.
        /// </summary>
        public IEnumerable<IPlayerInfo> Players => _gameData?.Players ?? new List<PlayerData>();

        /// <summary>
        /// The number of enacted fascist policies.
        /// </summary>
        public int FascistPolicyCount => _gameData?.EnactedFascistPolicyCount ?? 0;

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int LiberalPolicyCount => _gameData?.EnactedLiberalPolicyCount ?? 0;

        /// <summary>
        /// The count of players alive at the start of the game. This determines which fascist board is displayed.
        /// </summary>
        public int PlayerCount { get; private set; }

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

        /// <summary>
        /// The active viewmodel requesting input from the user.
        /// </summary>
        public InteractionViewModelBase ActiveModal
        {
            get => _activeModal;
            private set
            {
                _activeModal = value;
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

        private T Dispatch<T>(Func<T> callback)
        {
            // TODO Better dispatching. Where's CheckAccess?
            return Application.Current.Dispatcher.Invoke(callback);
        }

        #region Event Handlers

        public Task UpdateGameData(GameData gameData)
        {
            Dispatch(() =>
            {
                // New game clears all history.
                if (gameData?.GameGuid != _gameData?.GameGuid)
                    _gameData = null;

                // This shouldn't happen...
                if (gameData == null)
                    return;

                var oldGuid = _gameData?.GameGuid;

                _gameData = gameData;

                if (gameData.GameGuid != oldGuid)
                {
                    PlayerCount = Players.Count(_ => _.IsAlive);
                    RaisePropertyChanged(nameof(PlayerCount));
                }

                RaisePropertyChanged(nameof(Players));
                RaisePropertyChanged(nameof(Me));
                RaisePropertyChanged(nameof(LiberalPolicyCount));
                RaisePropertyChanged(nameof(FascistPolicyCount));
            });

            return Task.CompletedTask;
        }

        public Task MessageReceived(string message)
        {
            Dispatch(() => Messages = string.Concat(Messages, Environment.NewLine, message));
            return Task.CompletedTask;
        }

        public Task<Guid> SelectPlayer(GameState gameState, IEnumerable<Guid> candidates)
        {
            return Dispatch(async () =>
            {
                var selectionVm = new PlayerSelectionViewModel();
                switch (gameState)
                {
                    case GameState.ChancellorNomination:
                        selectionVm.Prompt = "Please select your desired chancellor.";
                        break;
                    case GameState.Execution:
                        selectionVm.Prompt = "Please a player to execute.";
                        break;
                    case GameState.InvestigateLoyalty:
                        selectionVm.Prompt = "Please select a player to see their loyalty card.";
                        break;
                    case GameState.SpecialElection:
                        selectionVm.Prompt = "Please select a player to serve as president in a special election.";
                        break;
                }

                selectionVm.Players = _gameData.Players.Join(candidates, p => p.Identifier, c => c, (p, g) => p).Cast<IPlayerInfo>().ToList();
                await ShowModalAsync(selectionVm);
                return selectionVm.SelectedPlayer.Identifier;
            });
        }

        public Task<bool> GetVote()
        {
            return Dispatch(() => ShowModalAsync(new VoteViewModel()
            {
                PresidentName = _gameData.President.Name,
                ChancellorName = _gameData.Chancellor.Name
            }));
        }

        public Task<IList<PolicyType>> SelectPolicies(IEnumerable<PolicyType> drawnPolicies, int allowedCount, bool allowVeto)
        {
            return Dispatch(async () =>
            {
                var vm = new PolicySelectionViewModel(drawnPolicies, allowedCount, allowVeto);
                var consented = await ShowModalAsync(vm);
                if (consented)
                    return (IList<PolicyType>)vm.Policies.Where(_ => _.IsSelected).Select(_ => _.Item).ToList();
                else
                    return new[] { PolicyType.None };
            });
        }

        public Task ShowPolicies(IEnumerable<PolicyType> deckTopThree)
        {
            return Dispatch(async () => await ShowModalAsync(new PolicyDisplayViewModel(deckTopThree)));
        }

        public Task RevealLoyalty(Guid playerGuid, PlayerRole role)
        {
            return Dispatch(async () => await ShowModalAsync(new LoyaltyDisplayViewModel(_gameData.Players.First(_ => _ == playerGuid), role)));
        }

        public Task<bool> PromptForVetoApproval()
        {
            return Dispatch(async () => await ShowModalAsync(
                new GeneralPromptViewModel("The chancellor wants to veto the provided policies.")
                {
                    AcceptText = "Allow Veto",
                    CancelText = "Deny Veto"
                }));
        }

        #endregion

        private async Task<bool> ShowModalAsync(InteractionViewModelBase vm)
        {
            var tcs = new TaskCompletionSource<bool>();
            vm.RegisterInteractionTaskCompletionSource(tcs);
            ActiveModal = vm;
            var result = await tcs.Task;
            ActiveModal = null;
            return result;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to send the message from <see cref="MessageToSend"/> to other clients.
        /// </summary>
        public RelayCommand SendMessageCommand => _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(SendMessage));

        #endregion
    }
}
