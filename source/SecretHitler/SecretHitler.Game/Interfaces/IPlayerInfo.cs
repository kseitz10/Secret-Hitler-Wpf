using System;

namespace SecretHitler.Game.Interfaces
{
    public interface IPlayerInfo : IEquatable<IPlayerInfo>
    {
        Guid Identifier { get; }

        string Name { get; }
    }
}
