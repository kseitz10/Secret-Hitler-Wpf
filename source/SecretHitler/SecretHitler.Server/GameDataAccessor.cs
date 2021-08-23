using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SecretHitler.Game.Engine;
using SecretHitler.Game.Entities;

namespace SecretHitler.Server
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
