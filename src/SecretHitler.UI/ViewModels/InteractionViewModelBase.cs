using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel which supports notifying it is done collecting/displaying information from/to the user.
    /// </summary>
    public class InteractionViewModelBase : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the interaction task completion source.
        /// </summary>
        protected TaskCompletionSource<bool> InteractionTaskCompletionSource { get; set; }

        /// <summary>
        /// Gets the Task
        /// </summary>
        public Task<bool> InteractionTask
        {
            get { return InteractionTaskCompletionSource?.Task; }
        }

        /// <summary>
        /// Registers a TaskCompletionSource that is used by the viewmodel to indicate it is done with whatever interactions it was intended to perform. 
        /// </summary>
        /// <param name="tcs">
        /// The task completion source.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Cannot set a new task completion source if an existing one has not yet ran to completion.
        /// </exception>
        public void RegisterInteractionTaskCompletionSource(TaskCompletionSource<bool> tcs)
        {
            var current = InteractionTaskCompletionSource;
            if (current != null && current.Task.Status == TaskStatus.Running)
                throw new InvalidOperationException("Cannot replace the task completion source on the base view model if the current TCS has not been given a result.");

            InteractionTaskCompletionSource = tcs;
        }

        /// <summary>
        /// Notifies anyone waiting the registered task completion source that the interaction has ended.
        /// The result provided to awaiters will be "false" i.e. the positive result was not explicitly chosen.
        /// </summary>
        public virtual void EndInteraction()
        {
            EndInteraction(false);
        }

        /// <summary>
        /// Notifies anyone waiting the registered task completion source that the interaction has ended with a specified result.
        /// </summary>
        /// <param name="result">The result, typically true if explicitly successful, otherwise false.</param>
        protected void EndInteraction(bool result)
        {
            if (InteractionTaskCompletionSource == null)
                return;

            InteractionTaskCompletionSource.TrySetResult(result);
        }
    }
}
