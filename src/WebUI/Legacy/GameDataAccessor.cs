using System;
using System.Collections.Generic;
using System.Linq;

using SecretHitler.Application.LegacyEngine;

namespace SecretHitler.WebUI.Legacy
{
    public class GameDataAccessor
    {
        public GameDataAccessor()
        {
            GameData = new GameData();
            var _ = new PolicyDeck(GameData.DrawPile, GameData.DiscardPile, true);
        }

        public GameData GameData { get; set; }
    }
}
