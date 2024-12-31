
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
/// Indicates the routing strategy of a routed event.
/// </summary>
public enum RoutingStrategy
{
    /// <summary>
    /// The routed event uses a tunneling strategy, where the event instance routes downwards through 
    /// the tree, from root to source element.
    /// </summary>
    Tunnel,

    /// <summary>
    /// The routed event uses a bubbling strategy, where the event instance routes upwards through the 
    /// tree, from event source to root.
    /// </summary>
    Bubble,

    /// <summary>
    /// The routed event does not route through an element tree.
    /// </summary>
    Direct,
}
