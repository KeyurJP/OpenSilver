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

using System.ComponentModel;
using System.Diagnostics;
using System.Xaml;
using OpenSilver.Internal;
using OpenSilver.Internal.Xaml;
using OpenSilver.Theming;

namespace System.Windows.Markup;

/// <summary>
/// Implements a markup extension that supports static (XAML load time) resource references made from XAML, 
/// with additional system logic that retrieves different resources depending on the currently active theme.
/// </summary>
[OpenSilver.NotImplemented]
[EditorBrowsable(EditorBrowsableState.Never)]
[ContentProperty(nameof(ResourceKey))]
public class ThemeResourceExtension : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeResourceExtension"/> class.
    /// </summary>
    public ThemeResourceExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeResourceExtension"/> class, with the provided
    /// initial key.
    /// </summary>
    /// <param name="resourceKey">
    /// The key of the resource that this markup extension references.
    /// </param>
    public ThemeResourceExtension(string resourceKey)
    {
        ResourceKey = resourceKey;
    }

    /// <summary>
    /// Gets or sets the key value passed by this theme resource reference. They key is used to return 
    /// the object matching that key in resource dictionaries.
    /// </summary>
    public string ResourceKey { get; set; }

    /// <summary>
    /// Returns the object found in a resource dictionary, where the object to find is identified by the 
    /// <see cref="ResourceKey"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// Object that can provide services for the markup extension.
    /// </param>
    /// <returns>
    /// The object value to set on the property where the markup extension provided value is evaluated.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (TryFindTheResource(serviceProvider, out object resource))
        {
            return resource;
        }

        return FindResourceInAppOrSystem() ??
            throw new XamlParseException(string.Format(Strings.ParserNoResource, ResourceKey));
    }

    private bool TryFindTheResource(IServiceProvider serviceProvider, out object resource)
    {
        if (serviceProvider.GetService(typeof(IAmbientResourcesProvider)) is IAmbientResourcesProvider ambientResourcesProvider)
        {
            return TryFindResourceFromCompiler(ambientResourcesProvider, out resource);
        }
        else if (serviceProvider.GetService(typeof(IAmbientProvider)) is IAmbientProvider ambientProvider)
        {
            if (serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) is not IXamlSchemaContextProvider schemaContextProvider)
            {
                throw new InvalidOperationException(
                    string.Format(Strings.MarkupExtensionNoContext1, GetType().Name, nameof(IXamlSchemaContextProvider)));
            }

            return TryFindResourceFromParser(ambientProvider, schemaContextProvider, out resource);
        }
        else
        {
            throw new InvalidOperationException(
                string.Format(Strings.MarkupExtensionNoContext2, GetType().Name, nameof(IAmbientResourcesProvider), nameof(IAmbientProvider)));
        }
    }

    private bool TryFindResourceFromCompiler(IAmbientResourcesProvider ambientProvider, out object resource)
    {
        Debug.Assert(ambientProvider is not null);

        foreach (object ambientValue in ambientProvider.GetAllAmbientValues())
        {
            if (ambientValue is ResourceDictionary rd && TryGetResourceFromDictionary(rd, out resource))
            {
                return true;
            }
        }

        resource = null;
        return false;
    }

    private bool TryFindResourceFromParser(IAmbientProvider ambientProvider, IXamlSchemaContextProvider schemaContextProvider, out object resource)
    {
        Debug.Assert(ambientProvider is not null);
        Debug.Assert(schemaContextProvider is not null);

        XamlSchemaContext schemaContext = schemaContextProvider.SchemaContext;

        XamlType feXType = schemaContext.GetXamlType(typeof(FrameworkElement));
        XamlType styleXType = schemaContext.GetXamlType(typeof(Style));
        XamlType templateXType = schemaContext.GetXamlType(typeof(FrameworkTemplate));
        XamlType appXType = schemaContext.GetXamlType(typeof(Application));

        XamlMember feResourcesProperty = feXType.GetMember("Resources");
        XamlMember styleResourcesProperty = styleXType.GetMember("Resources");
        XamlMember styleBasedOnProperty = styleXType.GetMember("BasedOn");
        XamlMember templateResourcesProperty = templateXType.GetMember("Resources");
        XamlMember appResourcesProperty = appXType.GetMember("Resources");

        XamlType[] types = new XamlType[1] { schemaContext.GetXamlType(typeof(ResourceDictionary)) };

        var ambientValues = ambientProvider.GetAllAmbientValues(null,
                                                                false,
                                                                types,
                                                                feResourcesProperty,
                                                                styleResourcesProperty,
                                                                styleBasedOnProperty,
                                                                templateResourcesProperty,
                                                                appResourcesProperty);

        foreach (var ambientValue in ambientValues)
        {
            if (ambientValue.Value is ResourceDictionary rd)
            {
                if (rd.TryGetResource(ResourceKey, out resource))
                {
                    return true;
                }
            }
            else if (ambientValue.Value is Style style)
            {
                resource = style.FindResource(ResourceKey);
                if (resource != DependencyProperty.UnsetValue)
                {
                    return true;
                }
            }
        }

        resource = null;
        return false;
    }

    private bool TryGetResourceFromDictionary(ResourceDictionary rd, out object resource)
    {
        Debug.Assert(rd is not null);

        if (rd.ThemeDictionaries.TryGetValue("Default", out ResourceDictionary themeDictionary))
        {
            return themeDictionary.TryGetResource(ResourceKey, out resource);
        }
        else
        {
            return rd.TryGetResource(ResourceKey, out resource);
        }
    }

    private object FindResourceInAppOrSystem()
    {
        if (Application.Current is Application app)
        {
            if (app.HasResources && app.Resources.TryGetResource(ResourceKey, out object resource))
            {
                return resource;
            }

            if (app.Theme is Theme theme && theme.TryGetResource(ResourceKey, out resource))
            {
                return resource;
            }
        }

        // Look in the built-in resources (eg. "SystemAccentColor")
        return XamlResources.FindBuiltInResource(ResourceKey);
    }
}