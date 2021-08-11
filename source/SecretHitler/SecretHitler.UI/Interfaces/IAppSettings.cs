using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
