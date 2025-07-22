namespace Breakout.Game.Input
{
    /// <summary>
    /// Interface for controls that can be selected/focused and activated within dialogs
    /// </summary>
    public interface ISelectableControl
    {
        /// <summary>
        /// Whether this control is currently selected/focused
        /// </summary>
        bool IsSelected { get; set; }
        
        /// <summary>
        /// Whether this control can be selected (enabled state)
        /// </summary>
        bool CanBeSelected { get; }
        
        /// <summary>
        /// Called when the control becomes selected
        /// </summary>
        void OnSelected();
        
        /// <summary>
        /// Called when the control is no longer selected
        /// </summary>
        void OnDeselected();
        
        /// <summary>
        /// Called when Fire key is pressed while this control is selected
        /// </summary>
        void OnActivated();
    }
}