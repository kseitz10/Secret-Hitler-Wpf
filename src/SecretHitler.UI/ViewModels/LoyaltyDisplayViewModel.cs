using System;
using System.Collections.Generic;
using System.Linq;

using SecretHitler.Application.Common.Interfaces;
using SecretHitler.Domain.Enums;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel for revealing the loyalty of another player.
    /// </summary>
    public class LoyaltyDisplayViewModel : ModalViewModelBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="player">Player info</param>
        /// <param name="role">Player's role</param>
        public LoyaltyDisplayViewModel(IPlayerInfo player, PlayerRole role)
        {
            Name = player?.Name;
            Role = role;
        }

        public string Name { get; private set; }

        public PlayerRole Role { get; private set; }
    }
}
