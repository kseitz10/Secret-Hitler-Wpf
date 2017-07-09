using SecretHitler.Game.Enums;
using System.Collections.Generic;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for displaying one or more policies
    /// </summary>
    public class PolicyDisplayViewModel : ModalViewModelBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="policies">
        /// The policies to show.
        /// </param>
        public PolicyDisplayViewModel(IEnumerable<PolicyType> policies)
        {
            Policies = policies;
        }

        /// <summary>
        /// Selectable items
        /// </summary>
        public IEnumerable<PolicyType> Policies { get; private set; }
    }
}
