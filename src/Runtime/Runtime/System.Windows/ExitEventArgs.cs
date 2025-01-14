
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
/// Event arguments for the <see cref="Application.Exit"/> event.
/// </summary>
public class ExitEventArgs : EventArgs
{
    internal ExitEventArgs(int exitCode)
    {
        ExitCode = exitCode;
    }

    /// <summary>
    /// Gets or sets the exit code that an application returns to the operating system when the application exits.
    /// </summary>
    /// <returns>
    /// The exit code that an application returns to the operating system when the application exits.
    /// </returns>
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the user should be prompted with a confirmation dialog when the application 
    /// is about to be closed.
    /// </summary>
    /// <returns>
    /// A value indicating if the user needs to confirm that he really wants to terminate the application. The default 
    /// is false.
    /// </returns>
    public bool Handled { get; set; }
}

/// <summary>
/// Represents the method that handles the <see cref="Application.Exit"/> event.
/// </summary>
/// <param name="sender">
/// The source of the event.
/// </param>
/// <param name="e">
/// The event data.
/// </param>
public delegate void ExitEventHandler(object sender, ExitEventArgs e);
