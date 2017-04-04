using System;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Entities
{
    public class Player : IPlayerInfo
    {
        public Guid Identifier { get; set; }

        public string Name { get; set; }

        public bool Equals(IPlayerInfo other) => other != null && other.Identifier.Equals(Identifier);
    }
}
