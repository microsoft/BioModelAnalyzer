using MvvmFx.Common.ViewModels;

namespace BioCheck.ViewModel.Editing
{
    /// <summary>
    /// Interface for Copy and Pasting ViewModel instances
    /// </summary>
    public interface ICopyable
    {
        /// <summary>
        /// Determines whether this instance can paste the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if this instance can paste the specified source; otherwise, <c>false</c>.
        /// </returns>
        bool CanPaste(ViewModelBase source);

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns></returns>
        ViewModelBase Copy();

        /// <summary>
        /// Pastes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        void Paste(ViewModelBase source);
    }
}