using SecretHitler.Game.Enums;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for selecting one or more policies
    /// </summary>
    public class PolicySelectionViewModel : ModalViewModelBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="policies">
        /// The policies to choose from.
        /// </param>
        /// <param name="selectionCount">
        /// The number of policies to be picked.
        /// </param>
        public PolicySelectionViewModel(IEnumerable<PolicyType> policies, int selectionCount)
        {
            RequiredCount = selectionCount;
            Policies = policies.Select(_ => new SelectableItem<PolicyType>(_, false)).ToArray();
        }

        /// <summary>
        /// The number of policies to be chosen.
        /// </summary>
        public int RequiredCount { get; private set; }

        /// <summary>
        /// Selectable items
        /// </summary>
        public IReadOnlyCollection<SelectableItem<PolicyType>> Policies { get; private set; }

        protected override bool CanExecuteAcceptCommand()
        {
            return Policies.Count(_ => _.IsSelected) == RequiredCount;
        }
    }
}
