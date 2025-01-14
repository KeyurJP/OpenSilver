
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

using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Contains the event data for the <see cref="Application.Startup"/> event.
/// </summary>
public sealed class StartupEventArgs : EventArgs
{
    internal StartupEventArgs() { }

    /// <summary>
    /// Gets the initialization parameters that were passed as part of HTML initialization of a 
    /// Silverlight plug-in.
    /// </summary>
    /// <returns>
    /// The set of initialization parameters, as a dictionary with key strings and value strings.
    /// </returns>
    public IDictionary<string, string> InitParams => Application.Current.Host.InitParams;
}

/// <summary>
/// Represents the method that will handle the <see cref="Application.Startup"/> event.
/// </summary>
/// <param name="sender">
/// The object that raised the event.
/// </param>
/// <param name="e">
/// The event data.
/// </param>
public delegate void StartupEventHandler(object sender, StartupEventArgs e);
