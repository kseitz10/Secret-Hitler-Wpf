using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.UI.Interfaces
{
    public interface IAppSettings
    {
        ushort LastPort { get; set; }
        string LastHostname { get; set; }
        string LastNickname { get; set; }
        void Save();
    }
}
