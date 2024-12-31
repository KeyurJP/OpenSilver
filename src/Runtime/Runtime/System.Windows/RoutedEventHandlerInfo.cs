
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

internal readonly struct RoutedEventHandlerInfo
{
    /// <summary>
    ///     Construtor for RoutedEventHandlerInfo
    /// </summary>
    /// <param name="handler">
    ///     Non-null handler
    /// </param>
    /// <param name="handledEventsToo">
    ///     Flag that indicates if or not the handler must 
    ///     be invoked for already handled events
    /// </param>
    internal RoutedEventHandlerInfo(Delegate handler, bool handledEventsToo)
    {
        Handler = handler;
        InvokeHandledEventsToo = handledEventsToo;
    }

    /// <summary>
    ///     Returns associated handler instance
    /// </summary>
    public Delegate Handler { get; }

    /// <summary>
    ///     Returns HandledEventsToo Flag
    /// </summary>
    public bool InvokeHandledEventsToo { get; }

    // Invokes handler instance as per specified 
    // invocation preferences
    internal void InvokeHandler(object target, RoutedEventArgs routedEventArgs)
    {
        if (!routedEventArgs.Handled || InvokeHandledEventsToo)
        {
            if (Handler is RoutedEventHandler handler)
            {
                // Generic RoutedEventHandler is called directly here since
                //  we don't need the InvokeEventHandler override to cast to
                //  the proper type - we know what it is.
                handler(target, routedEventArgs);
            }
            else
            {
                // NOTE: Cannot call protected method InvokeEventHandler directly
                routedEventArgs.InvokeHandler(Handler, target);
            }
        }
    }

    /// <summary>
    ///     Is the given object equivalent to the current one
    /// </summary>
    public override bool Equals(object obj) => obj is RoutedEventHandlerInfo info && Equals(info);

    /// <summary>
    ///     Is the given RoutedEventHandlerInfo equals the current
    /// </summary>
    public bool Equals(RoutedEventHandlerInfo handlerInfo)
        => Handler == handlerInfo.Handler && InvokeHandledEventsToo == handlerInfo.InvokeHandledEventsToo;

    /// <summary>
    ///     Serves as a hash function for a particular type, suitable for use in 
    ///     hashing algorithms and data structures like a hash table
    /// </summary>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>
    ///     Equals operator overload
    /// </summary>
    public static bool operator ==(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
        => handlerInfo1.Equals(handlerInfo2);

    /// <summary>
    ///     NotEquals operator overload
    /// </summary>
    public static bool operator !=(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
        => !handlerInfo1.Equals(handlerInfo2);
}