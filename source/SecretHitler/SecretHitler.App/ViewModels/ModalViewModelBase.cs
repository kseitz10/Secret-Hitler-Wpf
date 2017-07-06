using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Windows.Input;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// A viewmodel that supports common commands logic for modal windows.
    /// </summary>
    public abstract class ModalViewModelBase : InteractionViewModelBase
    {
        #region Methods

        /// <summary>
        /// Mark this modal user interaction as having completed successfully if <see cref="OnAccepting"/> returns true.
        /// </summary>
        private void Accept()
        {
            var eventArgs = new CancelEventArgs();
            OnAccepting(eventArgs);

            if (eventArgs.Cancel)
                return;

            OnAccepted();
            EndInteraction(true);
        }

        /// <summary>
        /// Mark this modal user interaction as having completed unsuccessfully and/or dismissed by the user.
        /// </summary>
        private void Cancel()
        {
            OnCanceled();
            EndInteraction(false);
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// When overridden in a derived class, performs checks to see if the interaction can be completed successfully when
        /// <see cref="AcceptCommand"/> has been invoked. For example, it might check IsValid or user permissions, provided
        /// that the CanExecute for the command was true.
        /// </summary>
        /// <param name="e">
        /// The cancel event args.
        /// </param>
        /// <remarks>
        /// The developer does not have to invoke base if inheriting directly from <see cref="ModalViewModelBase"/>.
        /// </remarks>
        protected virtual void OnAccepting(CancelEventArgs e) { }

        /// <summary>
        /// When overridden in a derived class, performs operations when <see cref="AcceptCommand"/> has been invoked successfully.
        /// </summary>
        /// <remarks>
        /// The developer does not have to invoke base if inheriting directly from <see cref="ModalViewModelBase"/>.
        /// </remarks>
        protected virtual void OnAccepted() { }

        /// <summary>
        /// When overridden in a derived class, specifies actions that should occur once the <see cref="CancelCommand"/> has been invoked.
        /// </summary>
        /// <remarks>
        /// The developer does not have to invoke base if inheriting directly from <see cref="ModalViewModelBase"/>.
        /// </remarks>
        protected virtual void OnCanceled() { }

        /// <summary>
        /// A predicate that determines if the <see cref="AcceptCommand"/> can be executed.
        /// </summary>
        /// <returns>
        /// True if the command can be executed, otherwise false.
        /// </returns>
        protected virtual bool CanExecuteAcceptCommand()
        {
            return true;
        }

        /// <summary>
        /// A predicate that determines if the <see cref="CancelCommand"/> can be executed.
        /// </summary>
        /// <returns>
        /// True if the command can be executed, otherwise false.
        /// </returns>
        /// <remarks>
        /// This will not affect the ability to call the EndInteraction method directly.
        /// </remarks>
        protected virtual bool CanExecuteCancelCommand()
        {
            return true;
        }

        /// <summary>
        /// Reevaluate the state of all commands.
        /// </summary>
        protected virtual void RaiseCanExecuteCommands()
        {
            ((RelayCommand)AcceptCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelCommand).RaiseCanExecuteChanged();
        }

        #endregion

        #region Commands

        private RelayCommand _acceptCommand;
        private RelayCommand _cancelCommand;

        public ICommand AcceptCommand
        {
            get { return _acceptCommand ?? (_acceptCommand = new RelayCommand(() => Accept(), () => CanExecuteAcceptCommand())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(() => Cancel(), () => CanExecuteCancelCommand())); }
        }

        #endregion
    }
}
