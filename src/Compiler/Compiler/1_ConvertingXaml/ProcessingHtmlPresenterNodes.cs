
/*===================================================================================
* 
*   Copyright (c) Userware (OpenSilver.net, CSHTML5.com)
*      
*   This file is part of both the OpenSilver Compiler (https://opensilver.net), which
*   is licensed under the MIT license (https://opensource.org/licenses/MIT), and the
*   CSHTML5 Compiler (http://cshtml5.com), which is dual-licensed (MIT + commercial).
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using System;
using System.Security;
using System.Text.RegularExpressions;

namespace OpenSilver.Compiler;

internal static class ProcessingHtmlPresenterNodes
{
    //------------------------------------------------------------
    // This class process the "HtmlPresenter" nodes in order to
    // "escape" its content so that it is not processed during
    // the rest of the compilation, and it is instead considered
    // like plain text.
    //------------------------------------------------------------

    public static string Process(string xaml)
    {
        return Regex.Replace(xaml,
            @"(?:(<\w+:HtmlPresenter[^>]*?/>)|(<\w+:HtmlPresenter[\s\S]*?>)([\s\S]*?)(</\w+:HtmlPresenter\s*>))",
            match =>
            {
                if (match.Groups.Count >= 3 && !string.IsNullOrEmpty(match.Groups[4].Value))
                {
                    string content = match.Groups[3].Value;
                    ReadOnlySpan<char> span = content.AsSpan().Trim();
                    if (!span.StartsWith("<![CDATA[".AsSpan()))
                    {
                        return match.Groups[2].Value + SecurityElement.Escape(content) + match.Groups[4].Value;
                    }
                }

                return match.Value;
            });
    }
}
