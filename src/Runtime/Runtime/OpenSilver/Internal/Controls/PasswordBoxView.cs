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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CSHTML5.Internal;

namespace OpenSilver.Internal.Controls;

internal sealed class PasswordBoxView : TextViewBase
{
    internal PasswordBoxView(PasswordBox host)
        : base(host)
    {
    }

    internal new PasswordBox Host => (PasswordBox)base.Host;

    public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
    {
        domElementWhereToPlaceChildren = null;
        return INTERNAL_HtmlDomManager.CreatePasswordBoxViewDomElementAndAppendIt((INTERNAL_HtmlDomElementReference)parentRef, this);
    }

    protected internal override void INTERNAL_OnAttachedToVisualTree()
    {
        base.INTERNAL_OnAttachedToVisualTree();

        SetProperties();

        if (FocusManager.GetFocusedElement() == Host)
        {
            InputManager.SetFocusNative(OuterDiv);
        }
    }

    internal protected sealed override void OnInput()
    {
        Host.UpdatePasswordProperty(GetPassword());
        InvalidateMeasure();
    }

    protected sealed override Size MeasureContent(Size constraint)
    {
        int pwdLength = Host.Password.Length;

        return ParentWindow.TextMeasurementService.MeasureView(
            OuterDiv.UniqueIdentifier,
            "pre",
            string.Empty,
            constraint.Width,
            pwdLength > 0 ? new string('•', pwdLength) : "M");
    }

    internal void SelectNative()
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            Interop.ExecuteJavaScriptVoid($"{sElement}.select()");
        }
    }

    internal void OnMaxLengthChanged(int maxLength)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            if (maxLength > 0)
            {
                INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "maxLength", maxLength);
            }
            else
            {
                INTERNAL_HtmlDomManager.RemoveAttribute(OuterDiv, "maxlength");
            }
        }
    }

    internal void SetCaretBrush(Brush brush)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            this.SetCaretColor(brush);
        }
    }

    internal void SetPasswordNative(string text)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            Interop.ExecuteJavaScriptVoid(
                $"{sElement}.value = \"{INTERNAL_HtmlDomManager.EscapeStringForUseInJavaScript(text)}\"");

            InvalidateMeasure();
        }
    }

    private string GetPassword()
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            return Interop.ExecuteJavaScriptString($"{sElement}.value") ?? string.Empty;
        }

        return string.Empty;
    }

    private void SetProperties()
    {
        PasswordBox host = Host;

        this.SetCaretColor(host.CaretBrush);
        int maxLength = host.MaxLength;
        if (maxLength > 0)
        {
            INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "maxlength", maxLength);
        }
        SetPasswordNative(host.Password);
    }
}