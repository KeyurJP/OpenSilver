
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

namespace System.Windows.Input;

/// <summary>
/// Provides data for the <see cref="UIElement.MouseWheel"/> routed event.
/// </summary>
public class MouseWheelEventArgs : MouseEventArgs
{
    /// <inheritdoc />
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget) =>
        ((MouseWheelEventHandler)genericHandler)(genericTarget, this);

    /// <summary>
    /// Gets a value that indicates the amount that the mouse wheel rotated relative to its starting 
    /// state or to the last occurrence of the event.
    /// </summary>
    /// <returns>
    /// An integer value that provides a factor of how much the mouse wheel rotated. This value can 
    /// be a negative integer.
    /// </returns>
    public int Delta { get; private set; }

    internal new void FillEventArgs(UIElement element, object jsEventArg)
    {
        base.FillEventArgs(element, jsEventArg);

        double deltaY = OpenSilver.Interop.ExecuteJavaScriptDouble(
            $"{OpenSilver.Interop.GetVariableStringForJS(jsEventArg)}.deltaY", false);

        Delta = deltaY > 0 ? -120 : 120;
    }
}
