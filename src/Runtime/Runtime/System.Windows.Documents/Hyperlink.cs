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

using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CSHTML5.Internal;
using OpenSilver.Internal.Media;

namespace System.Windows.Documents;

/// <summary>
/// Provides an inline-level content element that provides facilities for hosting hyperlinks.
/// </summary>
public sealed class Hyperlink : Span
{
    private JavaScriptCallback _clickCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="Hyperlink"/> class.
    /// </summary>
    public Hyperlink()
    {
        Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 124, 187));
        TextDecorations = Windows.TextDecorations.Underline;
    }

    /// <summary>
    /// Occurs when the left mouse button is clicked on a <see cref="Hyperlink"/>.
    /// </summary>
    public event RoutedEventHandler Click;

    /// <summary>
    /// Identifies the <see cref="Command"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(Hyperlink),
            new PropertyMetadata((object)null));

    /// <summary>
    /// Gets or sets a command to associate with the <see cref="Hyperlink"/>.
    /// </summary>
    /// <returns>
    /// A command to associate with the <see cref="Hyperlink"/>. The default 
    /// is null.
    /// </returns>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValueInternal(CommandProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(Hyperlink),
            new PropertyMetadata((object)null));

    /// <summary>
    /// Gets or sets command parameters associated with the command specified by the
    /// <see cref="Command"/> property.
    /// </summary>
    /// <returns>
    /// An object specifying parameters for the command specified by the <see cref="Command"/>
    /// property. The default is null.
    /// </returns>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValueInternal(CommandParameterProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="MouseOverForeground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MouseOverForegroundProperty =
        DependencyProperty.Register(
            nameof(MouseOverForeground),
            typeof(Brush),
            typeof(Hyperlink),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 237, 110, 0)))
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) =>
                {
                    const string MouseOverForegroundVariable = "--mouse-over-color";

                    Hyperlink hyperlink = (Hyperlink)d;
                    string color = (Brush)newValue switch
                    {
                        SolidColorBrush solidColorBrush => solidColorBrush.ToHtmlString(),
                        _ => string.Empty,
                    };

                    hyperlink.OuterDiv.Style.setProperty(MouseOverForegroundVariable, color);
                },
            });

    /// <summary>
    /// Gets or sets the brush that paints the foreground color when the mouse pointer
    /// moves over the <see cref="Hyperlink"/>.
    /// </summary>
    /// <returns>
    /// The brush that paints the foreground color when the mouse pointer moves over
    /// the <see cref="Hyperlink"/>.
    /// </returns>
    public Brush MouseOverForeground
    {
        get => (Brush)GetValue(MouseOverForegroundProperty);
        set => SetValueInternal(MouseOverForegroundProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="MouseOverTextDecorations"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MouseOverTextDecorationsProperty =
        DependencyProperty.Register(
            nameof(MouseOverTextDecorations),
            typeof(TextDecorationCollection),
            typeof(Hyperlink),
            new PropertyMetadata(Windows.TextDecorations.Underline)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) =>
                {
                    const string MouseOverTextDecorationsVariable = "--mouse-over-decoration";

                    Hyperlink hyperlink = (Hyperlink)d;
                    string value = FontProperties.ToCssTextDecoration((TextDecorationCollection)newValue);
                    hyperlink.OuterDiv.Style.setProperty(MouseOverTextDecorationsVariable, value);
                },
            });

    /// <summary>
    /// Gets or sets the <see cref="TextDecorationCollection"/> that decorates the <see cref="Hyperlink"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="TextDecorationCollection"/> that decorates the <see cref="Hyperlink"/>.
    /// </returns>
    public TextDecorationCollection MouseOverTextDecorations
    {
        get => (TextDecorationCollection)GetValue(MouseOverTextDecorationsProperty);
        set => SetValueInternal(MouseOverTextDecorationsProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="NavigateUri"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NavigateUriProperty =
        DependencyProperty.Register(
            nameof(NavigateUri),
            typeof(Uri),
            typeof(Hyperlink),
            new PropertyMetadata((object)null));

    /// <summary>
    /// Gets or sets a URI to navigate to when the <see cref="Hyperlink"/>
    /// is activated.
    /// </summary>
    /// <returns>
    /// The URI to navigate to when the <see cref="Hyperlink"/> is activated.
    /// The default is null.
    /// </returns>
    public Uri NavigateUri
    {
        get => (Uri)GetValue(NavigateUriProperty);
        set => SetValueInternal(NavigateUriProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="TargetName"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TargetNameProperty =
        DependencyProperty.Register(
            nameof(TargetName),
            typeof(string),
            typeof(Hyperlink),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the name of a target window or frame for the <see cref="Hyperlink"/>.
    /// </summary>
    /// <returns>
    /// A string that specifies the name of a target window or frame for the <see cref="Hyperlink"/>.
    /// </returns>
    public string TargetName
    {
        get => (string)GetValue(TargetNameProperty);
        set => SetValueInternal(TargetNameProperty, value);
    }

    protected internal override void INTERNAL_OnAttachedToVisualTree()
    {
        base.INTERNAL_OnAttachedToVisualTree();
        INTERNAL_HtmlDomManager.AddCSSClass(OuterDiv, "opensilver-hyperlink");
    }

    public override void INTERNAL_AttachToDomEvents()
    {
        base.INTERNAL_AttachToDomEvents();

        _clickCallback = JavaScriptCallback.Create(OnClickNative);

        string sDiv = OpenSilver.Interop.GetVariableStringForJS(OuterDiv);
        string sClickCallback = OpenSilver.Interop.GetVariableStringForJS(_clickCallback);
        OpenSilver.Interop.ExecuteJavaScriptVoidAsync(
            $"{sDiv}.addEventListener('click', function (e) {{ {sClickCallback}(); }})");
    }

    public override void INTERNAL_DetachFromDomEvents()
    {
        base.INTERNAL_DetachFromDomEvents();

        _clickCallback?.Dispose();
        _clickCallback = null;
    }

    private void OnClickNative() => OnClick();

    private void OnClick()
    {
        Click?.Invoke(this, new RoutedEventArgs { OriginalSource = this });

        ExecuteCommand();

        if (NavigateUri is Uri navigateUri)
        {
            Navigate(this, navigateUri, TargetName);
        }
    }

    private void ExecuteCommand()
    {
        if (Command is ICommand command)
        {
            object parameter = CommandParameter;
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }

    internal static void Navigate(DependencyObject d, Uri navigateUri, string target)
    {
        Debug.Assert(navigateUri is not null);

        if (!IsExternalTarget(target))
        {
            if (TryInternalNavigate(d, navigateUri, target))
            {
                return;
            }
        }

        if (target == "_search")
        {
            throw new NotImplementedException("The search target is not implemented.");
        }

        if (string.IsNullOrEmpty(target))
        {
            target = "_self";
        }

        NavigateNative(navigateUri, target);
    }

    private static bool IsExternalTarget(string target) =>
        target == "_blank" ||
        target == "_media" ||
        target == "_parent" ||
        target == "_search" ||
        target == "_self" ||
        target == "_top";

    private static void NavigateNative(Uri navigateUri, string target)
    {
        string sUri = OpenSilver.Interop.GetVariableStringForJS(navigateUri.ToString());
        string sTarget = OpenSilver.Interop.GetVariableStringForJS(target);
        OpenSilver.Interop.ExecuteJavaScriptVoidAsync($"window.open({sUri}, {sTarget})");
    }

    private static bool TryInternalNavigate(DependencyObject d, Uri navigateUri, string target)
    {
        DependencyObject subtree = d;
        do
        {
            d = VisualTreeHelper.GetParent(d) ?? (d as FrameworkElement)?.Parent;

            if (d is not null && (d is INavigate || VisualTreeHelper.GetParent(d) is null))
            {
                if (FindNavigator(d as FrameworkElement, subtree, target) is INavigate navigator)
                {
                    return navigator.Navigate(navigateUri);
                }
                subtree = d;
            }
        }
        while (d is not null);

        return false;
    }

    private static INavigate FindNavigator(FrameworkElement fe, DependencyObject subtree, string target)
    {
        if (fe is null)
        {
            return null;
        }

        if (fe is INavigate && (fe.Name == target || string.IsNullOrEmpty(target)))
        {
            return (INavigate)fe;
        }

        bool isPopup = fe is Popup;
        int count = isPopup ? 1 : VisualTreeHelper.GetChildrenCount(fe);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = isPopup ? ((Popup)fe).Child : VisualTreeHelper.GetChild(fe, i);
            if (child == subtree)
            {
                continue;
            }

            if (FindNavigator(child as FrameworkElement, subtree, target) is INavigate navigate)
            {
                return navigate;
            }
        }

        return null;
    }
}
