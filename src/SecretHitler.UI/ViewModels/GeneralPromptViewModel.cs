using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.UI.ViewModels
{
    /// <summary>
    /// Viewmodel for prompting the user
    /// </summary>
    public class GeneralPromptViewModel : ModalViewModelBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prompt">The prompt to show</param>
        public GeneralPromptViewModel(string prompt)
        {
            Prompt = prompt;
        }

        /// <summary>
        /// The prompt to display
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Text for accept button
        /// </summary>
        public string AcceptText { get; set; } = "Accept";

        /// <summary>
        /// Text for cancel button
        /// </summary>
        public string CancelText { get; set; } = "Cancel";
    }
}
