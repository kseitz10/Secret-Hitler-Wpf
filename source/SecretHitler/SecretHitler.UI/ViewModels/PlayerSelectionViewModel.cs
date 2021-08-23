using System;
using System.Collections.Generic;
using System.Linq;

using SecretHitler.Game.Interfaces;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel for selecting a player
    /// </summary>
    public class PlayerSelectionViewModel : ModalViewModelBase
    {
        private string _prompt;
        private List<IPlayerInfo> _players;
        private IPlayerInfo _selectedPlayer;

        /// <summary>
        /// Message to be displayed (why we are selecting a player.)
        /// </summary>
        public string Prompt
        {
            get => _prompt;
            set
            {
                _prompt = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The available players for selection
        /// </summary>
        public List<IPlayerInfo> Players
        {
            get => _players;
            set
            {
                _players = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The selected player
        /// </summary>
        public IPlayerInfo SelectedPlayer
        {
            get => _selectedPlayer;
            set
            {
                _selectedPlayer = value;
                RaisePropertyChanged();
                RaiseCanExecuteCommands();
            }
        }

        protected override bool CanExecuteAcceptCommand()
        {
            return SelectedPlayer != null;
        }
    }
}
