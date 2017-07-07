namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for voting ja/nein
    /// </summary>
    public class VoteViewModel : ModalViewModelBase
    {
        /// <summary>
        /// President name
        /// </summary>
        public string PresidentName { get; set; }

        /// <summary>
        /// Chancellor name
        /// </summary>
        public string ChancellorName { get; set; }
    }
}
