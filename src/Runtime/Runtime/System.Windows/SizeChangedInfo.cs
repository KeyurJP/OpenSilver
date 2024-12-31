
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

namespace System.Windows;

/// <summary>
/// Report the specifics of a value change involving a <see cref="Size"/>. This is used as a parameter in 
/// <see cref="UIElement.OnRenderSizeChanged(SizeChangedInfo)"/> overrides.
/// </summary>
public class SizeChangedInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SizeChangedInfo"/> class.
    /// </summary>
    /// <param name="element">
    /// The element where the size is being changed.
    /// </param>
    /// <param name="previousSize">
    /// The previous size, before the change.
    /// </param>
    /// <param name="widthChanged">
    /// true if the Width component of the size changed.
    /// </param>
    /// <param name="heightChanged">
    /// true if the Height component of the size changed.
    /// </param>
    public SizeChangedInfo(UIElement element, Size previousSize, bool widthChanged, bool heightChanged)
    {
        Element = element;
        PreviousSize = previousSize;
        WidthChanged = widthChanged;
        HeightChanged = heightChanged;
    }

    /// <summary>
    /// Gets the previous size of the size-related value being reported as changed.
    /// </summary>
    /// <returns>
    /// The previous size.
    /// </returns>
    public Size PreviousSize { get; }

    /// <summary>
    /// Gets the new size being reported.
    /// </summary>
    /// <returns>
    /// The new size.
    /// </returns>
    public Size NewSize => Element.RenderSize;

    /// <summary>
    /// Gets a value that declares whether the Width component of the size changed.
    /// </summary>
    /// <returns>
    /// true if the width changed; otherwise, false.
    /// </returns>
    public bool WidthChanged { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="SizeChangedInfo"/> reports a size change that includes 
    /// a significant change to the Height component.
    /// </summary>
    /// <returns>
    /// true if there is a significant Height component change; otherwise, false.
    /// </returns>
    public bool HeightChanged { get; private set; }

    //this method is used by UIElement to "accumulate" several cosequitive layout updates
    //into the single args object cahced on UIElement. Since the SizeChanged is deferred event,
    //there could be several size changes before it will actually fire.
    internal void Update(bool widthChanged, bool heightChanged)
    {
        WidthChanged |= widthChanged;
        HeightChanged |= heightChanged;
    }

    internal UIElement Element { get; }

    internal SizeChangedInfo Next;
}
