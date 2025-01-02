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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace OpenSilver.Internal.Xaml.Context
{
    public class XamlContext
    {
        private readonly XamlContextStack _stack;
        private object _rootInstance;
        private Lazy<INameResolver> _nameResolver;

        internal XamlContext()
        {
            _stack = new XamlContextStack();
            SavedDepth = 0;
            ServiceProvider = new ServiceProviderContext(this);
        }

        internal XamlContext(XamlContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            _stack = ctx._stack.DeepCopy();
            SavedDepth = _stack.Depth;
            ServiceProvider = new ServiceProviderContext(this);
        }

        internal void PushScope() => _stack.PushScope();

        internal void PopScope() => _stack.PopScope();

        internal object CurrentInstance
        {
            get => _stack.CurrentFrame.Instance;
            set => _stack.CurrentFrame.Instance = value;
        }

        internal object ParentInstance => _stack.PreviousFrame.Instance;

        internal object RootInstance
        {
            get
            {
                //evaluate if _rootInstance should just always look at _rootFrame.Instance instead of caching an instance
                if (_rootInstance == null)
                {
                    XamlObjectFrame rootFrame = GetTopFrame();
                    _rootInstance = rootFrame.Instance;
                }
                return _rootInstance;
            }
        }

        internal INameResolver NameResolver
        {
            get
            {
                if (_nameResolver == null)
                {
                    _nameResolver = new Lazy<INameResolver>(() =>
                    {
                        if (SavedDepth > 0)
                        {
                            return new TemplateNameResolver((IInternalFrameworkElement)_stack.GetFrame(SavedDepth + 1).Instance);
                        }
                        else if (RootInstance is IInternalFrameworkElement rootObject)
                        {
                            return new XamlNameResolver(rootObject);
                        }

                        return null;
                    });
                }

                return _nameResolver.Value;
            }
        }

        internal ServiceProviderContext ServiceProvider { get; }

        internal INameScope ExternalNameScope { get; set; }

        internal WeakReference<DependencyObject> TemplateOwnerReference { get; set; }

        internal DependencyObject GetTemplateOwner()
        {
            if (TemplateOwnerReference is WeakReference<DependencyObject> wr)
            {
                wr.TryGetTarget(out DependencyObject owner);
                return owner;
            }
            return null;
        }

        /// <summary>
        /// Total depth of the stack SavedDepth+LiveDepth
        /// </summary>
        internal int Depth => _stack.Depth;

        internal int SavedDepth { get; }

        /// <summary>
        /// The Depth of the Stack above the Saved (template) part
        /// </summary>
        internal int LiveDepth => Depth - SavedDepth;

        private XamlObjectFrame GetTopFrame()
        {
            if (_stack.Depth == 0)
            {
                return null;
            }

            return _stack.GetFrame(1);
        }

        internal IEnumerable<object> ServiceProvider_GetAllAmbientValues()
        {
            XamlObjectFrame frame = _stack.CurrentFrame;
            while (frame.Depth >= 1)
            {
                object inst = frame.Instance;
                switch (inst)
                {
                    case FrameworkElement fe:
                        {
                            if (fe.HasResources)
                            {
                                yield return fe.Resources;
                            }
                            for (Style style = fe.Style; style is not null; style = style.BasedOn)
                            {
                                if (style.HasResources)
                                {
                                    yield return style.Resources;
                                }
                            }
                            if (fe.TemplateInternal is FrameworkTemplate template && template.HasResources)
                            {
                                yield return template.Resources;
                            }
                        }
                        break;
                    case ResourceDictionary:
                        yield return inst;
                        break;
                    case Style style:
                        do
                        {
                            if (style.HasResources)
                            {
                                yield return style.Resources;
                            }
                            style = style.BasedOn;
                        } while (style is not null);
                        break;
                    case FrameworkTemplate template:
                        if (template.HasResources)
                        {
                            yield return template.Resources;
                        }
                        break;
                    case Application app:
                        if (app.HasResources)
                        {
                            yield return app.Resources;
                        }
                        break;
                    case IInternalFrameworkElement ife:
                        if (ife.HasResources)
                        {
                            yield return ife.Resources;
                        }
                        break;
                }

                frame = frame.Previous;
            }
        }
    }
}
