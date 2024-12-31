
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

internal readonly struct RouteItem
{
    private readonly RoutedEventHandlerInfo _routedEventHandlerInfo;

    internal RouteItem(object target, RoutedEventHandlerInfo routedEventHandlerInfo)
    {
        Target = target;
        _routedEventHandlerInfo = routedEventHandlerInfo;
    }

    internal object Target { get; }

    internal void InvokeHandler(RoutedEventArgs routedEventArgs) => _routedEventHandlerInfo.InvokeHandler(Target, routedEventArgs);

    public override bool Equals(object o) => Equals((RouteItem)o);

    public bool Equals(RouteItem routeItem) => routeItem.Target == Target && routeItem._routedEventHandlerInfo == _routedEventHandlerInfo;

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(RouteItem routeItem1, RouteItem routeItem2) => routeItem1.Equals(routeItem2);

    public static bool operator !=(RouteItem routeItem1, RouteItem routeItem2) => !routeItem1.Equals(routeItem2);
}
