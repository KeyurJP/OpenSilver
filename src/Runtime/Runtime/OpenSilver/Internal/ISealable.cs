
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

namespace OpenSilver.Internal;

internal interface ISealable
{
    /// <summary>
    /// Can the current instance be sealed
    /// </summary>
    bool CanSeal { get; }

    /// <summary>
    /// Is the current instance sealed
    /// </summary>
    bool IsSealed { get; }

    /// <summary>
    /// Seal the current instance by detaching from the dispatcher
    /// </summary>
    void Seal();
}
