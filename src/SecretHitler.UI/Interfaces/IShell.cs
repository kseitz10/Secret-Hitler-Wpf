using System;
using System.Collections.Generic;
using System.Linq;

using GalaSoft.MvvmLight;

namespace SecretHitler.UI.Interfaces
{
    public interface IShell
    {
        void UpdateMainRegion(ViewModelBase vm);
    }
}
