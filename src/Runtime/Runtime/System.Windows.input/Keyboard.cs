﻿
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
/// Represents the keyboard device.
/// </summary>
public static class Keyboard
{
    /// <summary>
    /// Gets the set of <see cref="ModifierKeys"/> that are currently pressed.
    /// </summary>
    public static ModifierKeys Modifiers => InputManager.Current.GetKeyboardModifiers();

    internal static bool IsFocusable(UIElement uie) => KeyboardNavigation.Current.IsTabStop(uie);
}
