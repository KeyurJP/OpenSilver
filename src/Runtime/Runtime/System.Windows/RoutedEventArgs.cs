
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

using OpenSilver.Internal;

namespace System.Windows;

/// <summary>
/// Contains state information and event data associated with a routed event.
/// </summary>
public class RoutedEventArgs : EventArgs
{
    private RoutedEvent _routedEvent;
    private bool _invokingHandler;
    private bool _userInitiated;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutedEventArgs"/> class.
    /// </summary>
    public RoutedEventArgs() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutedEventArgs"/> class, using the supplied 
    /// routed event identifier.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event identifier for this instance of the <see cref="RoutedEventArgs"/> class.
    /// </param>
    public RoutedEventArgs(RoutedEvent routedEvent)
    {
        _routedEvent = routedEvent;
    }

    /// <summary>
    /// Gets a reference to the object that raised the event.
    /// </summary>
    public object OriginalSource { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="Windows.RoutedEvent"/> associated with this <see cref="RoutedEventArgs"/> 
    /// instance.
    /// </summary>
    /// <returns>
    /// The identifier for the event that has been invoked.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Attempted to change the <see cref="RoutedEvent"/> value while the event is being routed.
    /// </exception>
    public RoutedEvent RoutedEvent
    {
        get { return _routedEvent; }
        set
        {
            if (_userInitiated && _invokingHandler)
            {
                throw new InvalidOperationException(Strings.RoutedEventCannotChangeWhileRouting);
            }

            _routedEvent = value;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates the present state of the event handling for a routed event 
    /// as it travels the route.
    /// </summary>
    /// <returns>
    /// If setting, set to true if the event is to be marked handled; otherwise false. If reading this 
    /// value, true indicates that either a class handler, or some instance handler along the route, 
    /// has already marked this event handled. False indicates that no such handler has marked the event 
    /// handled. The default value is false.
    /// </returns>
    public bool Handled { get; set; }

    /// <summary>
    /// When overridden in a derived class, provides a way to invoke event handlers in a type-specific
    /// way, which can increase efficiency over the base implementation.
    /// </summary>
    /// <param name="genericHandler">
    /// The generic handler / delegate implementation to be invoked.
    /// </param>
    /// <param name="genericTarget">
    /// The target on which the provided handler should be invoked.
    /// </param>
    protected virtual void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        if (genericHandler is null)
        {
            throw new ArgumentNullException(nameof(genericHandler));
        }

        if (genericTarget is null)
        {
            throw new ArgumentNullException(nameof(genericTarget));
        }

        if (genericHandler is RoutedEventHandler routedEventHandler)
        {
            routedEventHandler(genericTarget, this);
        }
        else
        {
            // Restricted Action - reflection permission required
            genericHandler.DynamicInvoke(new object[] { genericTarget, this });
        }
    }

    internal void InvokeHandler(Delegate handler, object target)
    {
        _invokingHandler = true;

        try
        {
            InvokeEventHandler(handler, target);
        }
        finally
        {
            _invokingHandler = false;
        }
    }

    internal void MarkAsUserInitiated() => _userInitiated = true;

    internal void ClearUserInitiated() => _userInitiated = false;

    internal object UIEventArg { get; set; }

    internal bool Cancellable { get; set; } = true;

    internal void PreventDefault()
    {
        if (!Cancellable)
        {
            return;
        }

        if (UIEventArg != null)
        {
            OpenSilver.Interop.ExecuteJavaScriptVoid(
                $"{OpenSilver.Interop.GetVariableStringForJS(UIEventArg)}.preventDefault();");
        }
    }
}
