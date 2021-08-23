using System;
using System.Collections.Generic;
using System.Linq;

using SecretHitler.Domain.Enums;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel for selecting one or more policies
    /// </summary>
    public class PolicySelectionViewModel : ModalViewModelBase
    {
        private bool _allowVeto;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="policies">
        /// The policies to choose from.
        /// </param>
        /// <param name="selectionCount">
        /// The number of policies to be picked.
        /// </param>
        /// <param name="allowVeto">
        /// Whether or not vetos are allowed.
        /// </param>
        public PolicySelectionViewModel(IEnumerable<PolicyType> policies, int selectionCount, bool allowVeto)
        {
            RequiredCount = selectionCount;
            Policies = policies.Select(_ =>
            {
                var selectable = new SelectableItem<PolicyType>(_, false);
                selectable.IsSelectedChanged += (sender, b) => RaiseCanExecuteCommands();
                return selectable;
            }).ToArray();
            _allowVeto = allowVeto;
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

        protected override bool CanExecuteCancelCommand()
        {
            return _allowVeto;
        }
    }
}
