
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
using System.Diagnostics;
using OpenSilver.Internal;

namespace System.Windows;

/// <summary>
///     Container for the event handlers
/// </summary>
/// <remarks>
///     EventHandlersStore is a hashtable of handlers for a given RoutedEvent
/// </remarks>
internal sealed class EventHandlersStore
{
    private readonly Dictionary<int, List<RoutedEventHandlerInfo>> _entries;

    /// <summary>
    ///     Constructor for EventHandlersStore
    /// </summary>
    public EventHandlersStore()
    {
        _entries = new Dictionary<int, List<RoutedEventHandlerInfo>>();
    }

    /// <summary>
    ///     Adds a routed event handler for the given 
    ///     RoutedEvent to the store
    /// </summary>
    public void AddRoutedEventHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
        Debug.Assert(routedEvent is not null);
        Debug.Assert(handler is not null);
        Debug.Assert(routedEvent.IsLegalHandler(handler), Strings.HandlerTypeIllegal);

        // Create a new RoutedEventHandler
        var routedEventHandlerInfo = new RoutedEventHandlerInfo(handler, handledEventsToo);

        if (!_entries.TryGetValue(routedEvent.GlobalIndex, out List<RoutedEventHandlerInfo> handlers))
        {
            _entries[routedEvent.GlobalIndex] = handlers = new List<RoutedEventHandlerInfo>(1);
        }

        handlers.Add(routedEventHandlerInfo);
    }

    /// <summary>
    ///     Removes an instance of the specified 
    ///     routed event handler for the given 
    ///     RoutedEvent from the store
    /// </summary>
    /// <remarks>
    ///     NOTE: This method does nothing if no 
    ///     matching handler instances are found 
    ///     in the store
    /// </remarks>
    public void RemoveRoutedEventHandler(RoutedEvent routedEvent, Delegate handler)
    {
        Debug.Assert(routedEvent is not null);
        Debug.Assert(handler is not null);
        Debug.Assert(routedEvent.IsLegalHandler(handler), Strings.HandlerTypeIllegal);

        if (_entries.TryGetValue(routedEvent.GlobalIndex, out List<RoutedEventHandlerInfo> handlers))
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                if (handlers[i].Handler == handler)
                {
                    handlers.RemoveAt(i);
                    break;
                }
            }
        }
    }

    // Returns Handlers for the given key
    public List<RoutedEventHandlerInfo> Get(RoutedEvent routedEvent)
    {
        return _entries.TryGetValue(routedEvent.GlobalIndex, out List<RoutedEventHandlerInfo> handlers) ? handlers : null;
    }
}
