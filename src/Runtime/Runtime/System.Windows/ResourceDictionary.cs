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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using OpenSilver.Internal;

namespace System.Windows
{
    /// <summary>
    /// Defines a dictionary that contains resources used by components of the app.
    /// This dictionary is oriented toward defining the resources in XAML, and then
    /// retrieving them through XAML references via the StaticResource markup extension.
    /// Alternatively you can access resources by traversing the dictionary at run
    /// time.
    /// </summary>
    public partial class ResourceDictionary : DependencyObject,
        IDictionary<object, object>,
        IDictionary,
        ISupportInitialize,
        INameScope
    {
        #region Data

        private PrivateFlags _flags = 0;
        private Dictionary<object, object> _baseDictionary;
        private ResourceDictionaryCollection _mergedDictionaries;

        //
        // Note: Not supported in WPF and Silverlight.
        // It is only here for UWP support.
        //
        private Dictionary<object, ResourceDictionary> _themeDictionaries;

        private WeakReferenceList<IResourceDictionaryOwner> _owners;

        // We store a weak reference so that the dictionary does not leak the owner.
        private WeakReference<DependencyObject> _inheritanceContext;

        // a dummy DO, used as the InheritanceContext when the dictionary's owner is
        // not itself a DO
        private static readonly DependencyObject _dummyInheritanceContext = new();
        private static readonly WeakReference<DependencyObject> DummyInheritanceContext = new(_dummyInheritanceContext);

        #endregion Data

        #region Constructor

        /// <summary>
        ///     Constructor for ResourceDictionary
        /// </summary>
        public ResourceDictionary()
        {
            _baseDictionary = new Dictionary<object, object>();
            IsThemeDictionary = XamlResources.IsSystemResourcesParsing;

            // Note: we want to handle the InheritanceContext here, so
            // we explicitly set these flags to make sur the Inheritance
            // Context is not changed/propagated anywhere else.
            CanBeInheritanceContext = false;
            IsInheritanceContextSealed = true;
        }

        #endregion Constructor

        #region Public Properties

        //
        // Note: Not supported in WPF and Silverlight.
        // It is only here for UWP support.
        //
        // Note: In UWP the ThemeDictionaries property return type is
        // IDictionary<object, object> and its real type is
        // ResourceDictionary.
        //
        /// <summary>
        /// Gets a collection of merged resource dictionaries that are 
        /// specifically keyed and composed to address theme scenarios
        /// </summary>
        public IDictionary<object, ResourceDictionary> ThemeDictionaries 
            => _themeDictionaries ??= new Dictionary<object, ResourceDictionary>();

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <remarks>
        ///     Fire Invalidations only for changes made after the Init Phase
        ///     If the key is not found on this ResourceDictionary, it will look on any MergedDictionaries for it
        /// </remarks>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public object this[object key]
        {
            get => GetItem(key);
            //
            // Note: In Silverlight setting a value through the indexer property
            // is not implemented and throw a NotImplementedException.
            // The behavior implemented below is taken from WPF.
            //
            set
            {
                // Seal styles and templates within App and Theme dictionary
                SealValue(value);

                SetItem(key, value);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <remarks>
        /// Elements stored in the <see cref="MergedDictionaries"/>
        /// property are not counted.
        /// </remarks>
        /// <returns>
        /// The number of elements contained in the collection.
        /// </returns>
        public int Count => _baseDictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ResourceDictionary"/> has a fixed size.
        /// </summary>
        /// <returns>Always returns false.</returns>
        public bool IsFixedSize => false;

        //
        // Note: see if we should enable readonly dictionaries 
        // for some special ResourceDictionaries like Theme Dictionaries
        //
        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        /// <returns>
        /// true if the hash table is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get => ReadPrivateFlag(PrivateFlags.IsReadOnly);
            internal set
            {
                WritePrivateFlag(PrivateFlags.IsReadOnly, value);

                if (value)
                {
                    // Seal all the styles and templates in this dictionary
                    SealValues();
                }

                // Set all the merged resource dictionaries as ReadOnly
                if (_mergedDictionaries is not null)
                {
                    foreach (ResourceDictionary mergedDictionary in _mergedDictionaries.InternalItems)
                    {
                        mergedDictionary.IsReadOnly = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection"/> object containing the keys of the <see cref="ResourceDictionary"/>.
        /// </summary>
        public ICollection Keys => _baseDictionary.Keys;

        ///<summary>
        /// List of ResourceDictionaries merged into this Resource Dictionary
        ///</summary>
        public PresentationFrameworkCollection<ResourceDictionary> MergedDictionaries
        {
            get
            {
                if (_mergedDictionaries == null)
                {
                    _mergedDictionaries = new ResourceDictionaryCollection(this);
                    _mergedDictionaries.CollectionChanged += new NotifyCollectionChangedEventHandler(OnMergedDictionariesChanged);
                }
                return _mergedDictionaries;
            }
        }

        /// <summary>
        /// Gets or sets a Uniform Resource Identifier (URI) that provides the source
        /// location of a merged resource dictionary.
        /// </summary>
        public Uri Source { get; set; }  // NOTE: This is used during COMPILE-TIME only.

        /// <summary>
        /// Gets an <see cref="ICollection"/> object containing the values of the <see cref="ResourceDictionary"/>.
        /// </summary>
        public ICollection Values => _baseDictionary.Values;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adds an item to the <see cref="ResourceDictionary"/>.
        /// </summary>
        /// <param name="key">The string key of the item to add.</param>
        /// <param name="value">The item value to add.</param>
        /// <exception cref="NotSupportedException">
        /// Attempted to add null as a value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Attempted to add an item with a key that already exists in this <see cref="ResourceDictionary"/>.-or-Attempted
        /// to use a key that is not a string.
        /// </exception>
        public void Add(object key, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.ResourceDictionaryIsReadOnly);
            }

            bool isImplicitStyle = false;
            bool isImplicitDataTemplate = false;

            switch (key)
            {
                case Type type:
                    if (value is not Style style)
                    {
                        throw new ArgumentException(Strings.ResourceDictionaryValueMustBeStyle);
                    }
                    if (style.TargetType != type)
                    {
                        throw new ArgumentException(Strings.ResourceDictionaryValueMustBeStyleWithCorrectTargetType);
                    }
                    isImplicitStyle = true;
                    break;

                case DataTemplateKey:
                    if (value is not DataTemplate)
                    {
                        throw new ArgumentException(Strings.ResourceDictionaryValueMustBeDataTemplate);
                    }
                    isImplicitDataTemplate = true;
                    break;

                case string:
                    break;

                default:
                    throw new ArgumentException(Strings.ResourceDictionaryKeyMustBeTypeOrString);
            }

            if (value is null)
            {
                throw new NotSupportedException(Strings.ResourceDictionaryNullValueNotSupported);
            }

            AddInternal(key, value, isImplicitStyle, isImplicitDataTemplate);
        }

        /// <summary>
        /// Adds an item to the <see cref="ResourceDictionary"/>.
        /// </summary>
        /// <param name="key">
        /// The string key of the item to add.
        /// </param>
        /// <param name="value">
        /// The item value to add.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Attempted to add null as a value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Attempted to add an item with a key that already exists in this <see cref="ResourceDictionary"/>.
        /// </exception>
        public void Add(string key, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.ResourceDictionaryIsReadOnly);
            }

            if (value is null)
            {
                throw new NotSupportedException(Strings.ResourceDictionaryNullValueNotSupported);
            }

            AddInternal(key, value, false, false);
        }

        /// <summary>
        /// Removes all items from this <see cref="ResourceDictionary"/>.
        /// </summary>
        public void Clear()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.ResourceDictionaryIsReadOnly);
            }

            if (Count > 0)
            {
                // remove inheritance context from all values that got it from
                // this dictionary
                RemoveInheritanceContextFromValues();

                Dictionary<object, object> oldDictionary = _baseDictionary;

                _baseDictionary = new Dictionary<object, object>();

                // Notify owners of the change and fire invalidate if already initialized
                NotifyOwners(ResourcesChangeInfo.CatastrophicDictionaryChangeInfo);

                if (IsLoaded())
                {
                    foreach (object resource in oldDictionary.Values)
                    {
                        UnloadResource(resource);
                    }
                }

                oldDictionary.Clear();
            }
        }

        /// <summary>
        /// Returns a value that indicates whether a specified key exists in the <see cref="ResourceDictionary"/>.
        /// </summary>
        /// <param name="key">
        /// The key to check for in the <see cref="ResourceDictionary"/>.
        /// </param>
        /// <returns>
        /// true if an item with that key exists in the <see cref="ResourceDictionary"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool Contains(object key)
        {
            bool result = _baseDictionary.ContainsKey(key);

            // Search for the value in the Merged Dictionaries
            if (_mergedDictionaries != null)
            {
                //
                // Note: we do the search in reversed order as it is the 
                // Silverlight and WPF behavior.
                //
                List<ResourceDictionary> mergedDictionaries = _mergedDictionaries.InternalItems;
                for (int i = mergedDictionaries.Count - 1; (i > -1) && !result; i--)
                {
                    result = mergedDictionaries[i].Contains(key);
                }
            }

            return result;
        }

        /// <summary>
        /// <see cref="Contains(object)"/>
        /// </summary>
        public bool ContainsKey(object key) => Contains(key);

        /// <summary>
        /// Copies the elements of the <see cref="ResourceDictionary"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from <see cref="ResourceDictionary"/>. The System.Array must have zero-based
        /// indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public void CopyTo(Array array, int index) => CopyTo(array as DictionaryEntry[], index);

        /// <summary>
        ///     Copies the dictionary's elements to a one-dimensional
        ///     Array instance at the specified index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional Array that is the destination of the
        ///     DictionaryEntry objects copied from Dictionary. The Array
        ///     must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        ///     The zero-based index in array at which copying begins.
        /// </param>
        public void CopyTo(DictionaryEntry[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            ((ICollection)_baseDictionary).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a non-generic
        /// collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IDictionaryEnumerator GetEnumerator() => _baseDictionary.GetEnumerator();

        /// <summary>
        /// Removes a specific item from the <see cref="ResourceDictionary"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the item to remove.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public void Remove(object key)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.ResourceDictionaryIsReadOnly);
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_baseDictionary.TryGetValue(key, out object resource))
            {
                // remove the inheritance context from the value, if it came from
                // this dictionary
                RemoveInheritanceContext(resource);

                _baseDictionary.Remove(key);

                // Notify owners of the change and fire invalidate if already initialized
                NotifyOwners(new ResourcesChangeInfo(key));

                if (IsLoaded())
                {
                    UnloadResource(resource);
                }
            }
        }

        /// <summary>
        /// Removes a specific item from the <see cref="ResourceDictionary"/>.
        /// </summary>
        /// <param name="key">
        /// The string key of the item to remove.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public void Remove(string key) => Remove((object)key);

        #region ISupportInitialize

        /// <summary>
        ///     Mark the begining of the Init phase
        /// </summary>
        /// <remarks>
        ///     BeginInit and EndInit follow a transaction model. BeginInit marks the
        ///     dictionary uninitialized and EndInit marks it initialized.
        /// </remarks>
        public void BeginInit()
        {
            // Nested BeginInits on the same instance aren't permitted
            if (IsInitializePending)
            {
                throw new InvalidOperationException(Strings.NestedBeginInitNotSupported);
            }

            IsInitializePending = true;
            IsInitialized = false;
        }

        /// <summary>
        ///     Fire Invalidation at the end of Init phase
        /// </summary>
        /// <remarks>
        ///     BeginInit and EndInit follow a transaction model. BeginInit marks the
        ///     dictionary uninitialized and EndInit marks it initialized.
        /// </remarks>
        public void EndInit()
        {
            if (!IsInitializePending)
            {
                throw new InvalidOperationException(Strings.EndInitWithoutBeginInitNotSupported);
            }
            Debug.Assert(IsInitialized == false, "Dictionary should not be initialized when EndInit is called");

            IsInitializePending = false;
            IsInitialized = true;

            // Fire Invalidations collectively for all changes made during the Init Phase
            NotifyOwners(new ResourcesChangeInfo(null, this));
        }

        #endregion ISupportInitialize

        #endregion Public Methods

        #region Explicit interface implementation

        object ICollection.SyncRoot => throw new NotImplementedException();

        bool ICollection.IsSynchronized => false;

        int ICollection<KeyValuePair<object, object>>.Count => throw new NotImplementedException();

        ICollection<object> IDictionary<object, object>.Keys => throw new NotImplementedException();

        ICollection<object> IDictionary<object, object>.Values => throw new NotImplementedException();

        bool IDictionary<object, object>.Remove(object key)
        {
            int count = Count;
            Remove(key);
            return Count < count;
        }

        bool IDictionary<object, object>.TryGetValue(object key, out object value) => TryGetResource(key, out value);

        void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> item) => Add(item.Key, item.Value);

        void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
            => throw new NotImplementedException();

        bool ICollection<KeyValuePair<object, object>>.Contains(KeyValuePair<object, object> item) => Contains(item.Key);

        bool ICollection<KeyValuePair<object, object>>.Remove(KeyValuePair<object, object> item)
        {
            int count = Count;
            Remove(item.Key);
            return Count < count;
        }

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
            => ((IEnumerable<KeyValuePair<object, object>>)_baseDictionary).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion Explicit interface implementation

        #region Internal API

        internal static readonly DependencyProperty ResourceKeyProperty =
            DependencyProperty.Register(
                "ResourceKeyProperty",
                typeof(object),
                typeof(ResourceDictionary),
                null);

        #region Helper Methods

        // Add an owner for this dictionary
        internal void AddOwner(IResourceDictionaryOwner owner)
        {
            Debug.Assert(owner is not null);

            if (_inheritanceContext is null)
            {
                // the first owner gets to be the InheritanceContext for
                // all the values in the dictionary that want one.
                if (owner is DependencyObject inheritanceContext)
                {
                    _inheritanceContext = new WeakReference<DependencyObject>(inheritanceContext);

                    // set InheritanceContext for the existing values
                    AddInheritanceContextToValues();
                }
                else
                {
                    // if the first owner is ineligible, use a dummy
                    _inheritanceContext = DummyInheritanceContext;

                    // set InheritanceContext for the existing values
                    AddInheritanceContextToValues();

                    //
                    // Note: In WPF InheritedContext is handled later, as
                    // explained in the comment below.
                    // However, in Silverlight, values are not sealed when
                    // added to the Application resources, so we do it here.
                    //
                    // do not call AddInheritanceContextToValues -
                    // the owner is an Application, and we'll be
                    // calling SealValues soon, which takes care
                    // of InheritanceContext as well
                }
            }

            if (_owners is null)
            {
                _owners = new WeakReferenceList<IResourceDictionaryOwner>(1);
            }
            else if (_owners.Contains(owner) && ContainsCycle(this))
            {
                throw new InvalidOperationException(Strings.ResourceDictionaryInvalidMergedDictionary);
            }

            owner.SetResources(this);

            _owners.Add(owner);

            AddOwnerToAllMergedDictionaries(owner);

            // This dictionary will be marked initialized if no one has called BeginInit on it.
            // This is done now because having an owner is like a parenting operation for the dictionary.
            TryInitialize();
        }

        // Remove an owner for this dictionary
        internal void RemoveOwner(IResourceDictionaryOwner owner)
        {
            Debug.Assert(owner is not null);

            if (_owners != null)
            {
                _owners.Remove(owner);

                if (_owners.Count == 0)
                {
                    _owners = null;
                }
            }

            if (owner == InheritanceContext)
            {
                RemoveInheritanceContextFromValues();
                _inheritanceContext = null;
            }

            RemoveOwnerFromAllMergedDictionaries(owner);
        }

        // Check if the given is an owner to this dictionary
        internal bool ContainsOwner(IResourceDictionaryOwner owner) => _owners is not null && _owners.Contains(owner);

        // Helper method that tries to set IsInitialized to true if BeginInit hasn't been called before this.
        // This method is called on AddOwner
        private void TryInitialize()
        {
            if (!IsInitializePending && !IsInitialized)
            {
                IsInitialized = true;
            }
        }

        // Call FrameworkElement.InvalidateTree with the right data
        private void NotifyOwners(ResourcesChangeInfo info)
        {
            bool shouldInvalidate = IsInitialized;
            bool hasImplicitStyles = info.IsResourceAddOperation && HasImplicitStyles;

            if (shouldInvalidate && InvalidatesImplicitDataTemplateResources)
            {
                info.SetIsImplicitDataTemplateChange();
            }

            if (shouldInvalidate || hasImplicitStyles)
            {
                // Invalidate all owners
                if (_owners != null)
                {
                    foreach (IResourceDictionaryOwner owner in _owners)
                    {
                        owner.OnResourcesChange(info, shouldInvalidate, hasImplicitStyles);
                    }
                }
            }
        }

        private void OnMergedDictionariesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResourcesChangeInfo info;

            if (e.Action != NotifyCollectionChangedAction.Reset)
            {
                Debug.Assert(
                    (e.NewItems != null && e.NewItems.Count == 1) ||
                    (e.OldItems != null && e.OldItems.Count == 1),
                    "The NotifyCollectionChanged event fired when no dictionaries were added or removed");

                ResourceDictionary oldDictionary = null;
                ResourceDictionary newDictionary = null;

                // If one or more resource dictionaries were removed we
                // need to remove the owners they were given by their
                // parent ResourceDictionary.

                if (e.Action == NotifyCollectionChangedAction.Remove ||
                    e.Action == NotifyCollectionChangedAction.Replace)
                {
                    oldDictionary = (ResourceDictionary)e.OldItems[0];
                    RemoveParentOwners(oldDictionary);
                }

                // If one or more resource dictionaries were added to the merged
                // dictionaries collection we need to send down the parent
                // ResourceDictionary's owners.
                if (e.Action == NotifyCollectionChangedAction.Add ||
                    e.Action == NotifyCollectionChangedAction.Replace)
                {
                    newDictionary = (ResourceDictionary)e.NewItems[0];

                    // If the merged dictionary HasImplicitStyle mark the outer dictionary the same.
                    if (!HasImplicitStyles && newDictionary.HasImplicitStyles)
                    {
                        HasImplicitStyles = true;
                    }

                    // If the merged dictionary HasImplicitDataTemplates mark the outer dictionary the same.
                    if (!HasImplicitDataTemplates && newDictionary.HasImplicitDataTemplates)
                    {
                        HasImplicitDataTemplates = true;
                    }

                    // If the parent dictionary is a theme dictionary mark the merged dictionary the same.
                    if (IsThemeDictionary)
                    {
                        newDictionary.IsThemeDictionary = true;
                    }

                    PropagateParentOwners(newDictionary);
                }

                info = new ResourcesChangeInfo(oldDictionary, newDictionary);
            }
            else
            {
                // Case when MergedDictionary collection is cleared
                info = ResourcesChangeInfo.CatastrophicDictionaryChangeInfo;
            }

            // Notify the owners of the change and fire
            // invalidation if already initialized
            NotifyOwners(info);
        }

        /// <summary>
        /// Adds the given owner to all merged dictionaries of this ResourceDictionary
        /// </summary>
        /// <param name="owner"></param>
        private void AddOwnerToAllMergedDictionaries(IResourceDictionaryOwner owner)
        {
            if (_mergedDictionaries != null)
            {
                List<ResourceDictionary> mergedDictionaries = _mergedDictionaries.InternalItems;
                for (int i = 0; i < mergedDictionaries.Count; i++)
                {
                    mergedDictionaries[i].AddOwner(owner);
                }
            }
        }

        /// <summary>
        /// Removes the given owner to all merged dictionaries of this ResourceDictionary
        /// </summary>
        /// <param name="owner"></param>
        private void RemoveOwnerFromAllMergedDictionaries(IResourceDictionaryOwner owner)
        {
            if (_mergedDictionaries != null)
            {
                List<ResourceDictionary> mergedDictionaries = _mergedDictionaries.InternalItems;
                for (int i = 0; i < mergedDictionaries.Count; i++)
                {
                    mergedDictionaries[i].RemoveOwner(owner);
                }
            }
        }

        /// <summary>
        /// This sends down the owners of this ResourceDictionary into the given
        /// merged dictionary.  We do this because whenever a merged dictionary
        /// changes it should invalidate all owners of its parent ResourceDictionary.
        ///
        /// Note that AddOwners throw if the merged dictionary already has one of the
        /// parent's owners.  This implies that either we're putting a dictionary
        /// into its own MergedDictionaries collection or we're putting the same
        /// dictionary into the collection twice, neither of which are legal.
        /// </summary>
        /// <param name="mergedDictionary"></param>
        private void PropagateParentOwners(ResourceDictionary mergedDictionary)
        {
            if (_owners != null)
            {
                Debug.Assert(_owners.Count > 0);

                mergedDictionary._owners ??= new WeakReferenceList<IResourceDictionaryOwner>(_owners.Count);

                foreach (IResourceDictionaryOwner owner in _owners)
                {
                    mergedDictionary.AddOwner(owner);
                }
            }
        }

        /// <summary>
        /// Removes the owners of this ResourceDictionary from the given
        /// merged dictionary.  The merged dictionary will be left with
        /// whatever owners it had before being merged.
        /// </summary>
        /// <param name="mergedDictionary"></param>
        internal void RemoveParentOwners(ResourceDictionary mergedDictionary)
        {
            if (_owners != null)
            {
                foreach (IResourceDictionaryOwner owner in _owners)
                {
                    mergedDictionary.RemoveOwner(owner);
                }
            }
        }

        private bool ContainsCycle(ResourceDictionary origin)
        {
            if (_mergedDictionaries != null)
            {
                foreach (ResourceDictionary mergedDictionary in _mergedDictionaries.InternalItems)
                {
                    if (mergedDictionary == origin || mergedDictionary.ContainsCycle(origin))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Helper Methods

        #region Inheritance Context

        private new DependencyObject InheritanceContext
        {
            get
            {
                if (_inheritanceContext is not null && _inheritanceContext.TryGetTarget(out DependencyObject context))
                {
                    return context;
                }
                return null;
            }
        }

        //  This method
        //  1. Seals all the styles/templates that belong to this App/Theme/Style/Template ResourceDictionary
        private void SealValues()
        {
            Debug.Assert(IsReadOnly, "This must be an Style/Template ResourceDictionary");

            if (_baseDictionary.Count > 0)
            {
                foreach (object value in _baseDictionary.Values)
                {
                    SealValue(value);
                }
            }
        }

        //  This method
        //  1. Sets the InheritanceContext of the value to the dictionary's principal owner
        //  (Not yet) 2. Seals the style/template that is to be placed in an App/Theme/Style/Template ResourceDictionary
        private void SealValue(object value)
        {
            DependencyObject inheritanceContext = InheritanceContext;
            if (inheritanceContext != null)
            {
                AddInheritanceContext(inheritanceContext, value);
            }

            if (IsReadOnly)
            {
                // If the value is a ISealable then seal it
                StyleHelper.SealIfSealable(value);
            }
        }

        // add inheritance context to a value
        private void AddInheritanceContext(DependencyObject inheritanceContext, object value)
        {
            if (inheritanceContext.ProvideSelfAsInheritanceContext(value, ResourceKeyProperty))
            {
                // if the assignment was successful, seal the value's InheritanceContext.
                // This makes sure the resource always gets inheritance-related information
                // from its point of definition, not from its point of use.
                if (value is DependencyObject doValue)
                {
                    doValue.IsInheritanceContextSealed = true;
                }
            }
        }

        // add inheritance context to all values that came from this dictionary
        private void AddInheritanceContextToValues()
        {
            DependencyObject inheritanceContext = InheritanceContext;

            // setting InheritanceContext can cause values to be replaced (Dev11 380869).
            // This changes the Values collection, so we can't iterate it directly.
            // Instead, iterate over a copy.
            int count = _baseDictionary.Count;
            if (count > 0)
            {
                object[] values = new object[count];
                _baseDictionary.Values.CopyTo(values, 0);

                foreach (object value in values)
                {
                    AddInheritanceContext(inheritanceContext, value);
                }
            }
        }

        // remove inheritance context from a value, if it came from this dictionary
        private void RemoveInheritanceContext(object value)
        {
            DependencyObject inheritanceContext = InheritanceContext;

            if (value is DependencyObject doValue &&
                inheritanceContext != null &&
                doValue.IsInheritanceContextSealed &&
                doValue.InheritanceContext == inheritanceContext)
            {
                doValue.IsInheritanceContextSealed = false;
                inheritanceContext.RemoveSelfAsInheritanceContext(doValue, ResourceKeyProperty);
            }
        }

        // remove inheritance context from all values that came from this dictionary
        private void RemoveInheritanceContextFromValues()
        {
            foreach (object value in _baseDictionary.Values)
            {
                RemoveInheritanceContext(value);
            }
        }

        #endregion Inheritance Context

        internal bool IsEmpty => Count == 0 && (_mergedDictionaries is null || _mergedDictionaries.InternalCount == 0);

        internal bool TryGetResource(object key, out object value) => (value = GetItem(key)) != null;

        internal object GetItem(object key)
        {
            if (_baseDictionary.TryGetValue(key, out object value))
            {
                return value;
            }
            else
            {
                //Search for the value in the Merged Dictionaries
                if (_mergedDictionaries != null)
                {
                    //
                    // Note: we do the search in reversed order as it is the 
                    // Silverlight and WPF behavior.
                    //
                    List<ResourceDictionary> mergedDictionaries = _mergedDictionaries.InternalItems;
                    for (int i = mergedDictionaries.Count - 1; (i > -1); i--)
                    {
                        value = mergedDictionaries[i].GetItem(key);
                        if (value != null)
                        {
                            break;
                        }
                    }
                }
            }
            return value;
        }

        private void SetItem(object key, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.ResourceDictionaryIsReadOnly);
            }

            if (value is null)
            {
                //
                // Note: Silverlight does not support null values in a 
                // ResourceDictionary but WPF does.
                //
                throw new NotSupportedException(Strings.ResourceDictionaryNullValueNotSupported);
            }

            if (!_baseDictionary.TryGetValue(key, out object oldItem) || oldItem != value)
            {
                _baseDictionary[key] = value;

                // Update the HasImplicitStyles flag
                UpdateHasImplicitStyles(key);

                // Update the HasImplicitDataTemplates flag
                UpdateHasImplicitDataTemplates(key);

                // Notify owners of the change and fire invalidate if already initialized
                NotifyOwners(new ResourcesChangeInfo(key));

                if (IsLoaded())
                {
                    UnloadResource(oldItem);
                }
            }
        }

        private void AddInternal(object key, object value, bool isImplicitStyle, bool isImplicitDataTemplate)
        {
            // Seal styles and templates within App and Theme dictionary
            SealValue(value);

            _baseDictionary.Add(key, value);

            if (isImplicitStyle)
            {
                HasImplicitStyles = true;
            }

            if (isImplicitDataTemplate)
            {
                HasImplicitDataTemplates = true;
            }

            // Notify owners of the change and fire invalidate if already initialized
            NotifyOwners(new ResourcesChangeInfo(key));
        }

        internal void LoadResources()
        {
            if (_mergedDictionaries != null)
            {
                foreach (ResourceDictionary rd in _mergedDictionaries.InternalItems)
                {
                    rd.LoadResources();
                }
            }

            foreach (object resource in _baseDictionary.Values)
            {
                LoadResource(resource);
            }
        }

        internal void UnloadResources()
        {
            if (_mergedDictionaries != null)
            {
                foreach (ResourceDictionary rd in _mergedDictionaries.InternalItems)
                {
                    rd.UnloadResources();
                }
            }

            foreach (object resource in _baseDictionary.Values)
            {
                UnloadResource(resource);
            }
        }

        private bool IsLoaded() => InheritanceContext is FrameworkElement feOwner && feOwner.IsLoaded;

        private void LoadResource(object resource)
        {
            if (resource is FrameworkElement feResource
                && !feResource.IsLoaded && !feResource.IsLoadedInResourceDictionary)
            {
                feResource.IsLoadedInResourceDictionary = true;
                feResource.LoadResources();
                feResource.RaiseLoadedEvent();
            }
        }

        private static void UnloadResource(object resource)
        {
            if (resource is FrameworkElement feResource && feResource.IsLoadedInResourceDictionary)
            {
                feResource.IsLoadedInResourceDictionary = false;
                feResource.RaiseUnloadedEvent();
                feResource.UnloadResources();
            }
        }

        // Sets the HasImplicitStyles flag if the given key is of type Type.
        private void UpdateHasImplicitStyles(object key)
        {
            // Update the HasImplicitStyles flag
            if (!HasImplicitStyles)
            {
                HasImplicitStyles = key is Type;
            }
        }

        // Sets the HasImplicitDataTemplates flag if the given key is of type DataTemplateKey.
        private void UpdateHasImplicitDataTemplates(object key)
        {
            // Update the HasImplicitDataTemplates flag
            if (!HasImplicitDataTemplates)
            {
                HasImplicitDataTemplates = key is DataTemplateKey;
            }
        }

        private bool IsInitialized
        {
            get => ReadPrivateFlag(PrivateFlags.IsInitialized);
            set => WritePrivateFlag(PrivateFlags.IsInitialized, value);
        }

        private bool IsInitializePending
        {
            get => ReadPrivateFlag(PrivateFlags.IsInitializePending);
            set => WritePrivateFlag(PrivateFlags.IsInitializePending, value);
        }

        private bool IsThemeDictionary
        {
            get => ReadPrivateFlag(PrivateFlags.IsThemeDictionary);
            set
            {
                if (IsThemeDictionary != value)
                {
                    WritePrivateFlag(PrivateFlags.IsThemeDictionary, value);
                    //if (value)
                    //{
                    //    SealValues();
                    //}
                    if (_mergedDictionaries != null)
                    {
                        List<ResourceDictionary> mergedDictionaries = _mergedDictionaries.InternalItems;
                        for (int i = 0; i < mergedDictionaries.Count; i++)
                        {
                            mergedDictionaries[i].IsThemeDictionary = value;
                        }
                    }
                }
            }
        }

        internal bool HasImplicitStyles
        {
            get => ReadPrivateFlag(PrivateFlags.HasImplicitStyles);
            set => WritePrivateFlag(PrivateFlags.HasImplicitStyles, value);
        }

        internal bool HasImplicitDataTemplates
        {
            get => ReadPrivateFlag(PrivateFlags.HasImplicitDataTemplates);
            set => WritePrivateFlag(PrivateFlags.HasImplicitDataTemplates, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the invalidations fired by the
        /// <see cref="ResourceDictionary"/> object cause <see cref="ContentPresenter"/> 
        /// objects to reevaluate their choice of template. The invalidations happen when
        /// an implicit data template resource changes.
        /// </summary>
        public bool InvalidatesImplicitDataTemplateResources
        {
            get => ReadPrivateFlag(PrivateFlags.InvalidatesImplicitDataTemplateResources);
            set => WritePrivateFlag(PrivateFlags.InvalidatesImplicitDataTemplateResources, value);
        }

        private void WritePrivateFlag(PrivateFlags bit, bool value)
        {
            if (value)
            {
                _flags |= bit;
            }
            else
            {
                _flags &= ~bit;
            }
        }

        private bool ReadPrivateFlag(PrivateFlags bit) => (_flags & bit) != 0;

        #endregion Internal API

        #region INameScope implementation

        private Dictionary<string, object> _nameScopeDictionary = new Dictionary<string, object>();

        //
        // Note: WPF always returns null
        //
        /// <summary>
        /// Finds the UIElement with the specified name.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <returns>The object with the specified name if any; otherwise null.</returns>
        [Obsolete("INameScope is not supported on ResourceDictionaries.")]
        public object FindName(string name)
        {
            if (_nameScopeDictionary.ContainsKey(name))
                return _nameScopeDictionary[name];
            else
                return null;
        }

        //
        // Note: WPF does not support RegisterName,
        // A NotSupportedException is thrown
        //
        [Obsolete("INameScope is not supported on ResourceDictionaries.")]
        public void RegisterName(string name, object scopedElement)
        {
            if (_nameScopeDictionary.ContainsKey(name) && _nameScopeDictionary[name] != scopedElement)
                throw new ArgumentException(string.Format(Strings.NameScopeDuplicateNamesNotAllowed, name));

            _nameScopeDictionary[name] = scopedElement;
        }

        //
        // Note: does nothing in WPF as name can't be registered...
        //
        [Obsolete("INameScope is not supported on ResourceDictionaries.")]
        public void UnregisterName(string name)
        {
            if (!_nameScopeDictionary.ContainsKey(name))
                throw new ArgumentException(string.Format(Strings.NameScopeNameNotFound, name));

            _nameScopeDictionary.Remove(name);
        }

        #endregion

        #region Private Types

        private enum PrivateFlags : byte
        {
            IsInitialized = 0x01,
            IsInitializePending = 0x02,
            IsReadOnly = 0x04,
            IsThemeDictionary = 0x08,
            HasImplicitStyles = 0x10,
            CanBeAccessedAcrossThreads = 0x20, // unused
            InvalidatesImplicitDataTemplateResources = 0x40,
            HasImplicitDataTemplates = 0x80,
        }

        #endregion

        internal static class Helpers
        {
            public static Dictionary<object, object> BuildImplicitResourcesCache(ResourceDictionary rd)
            {
                Debug.Assert(rd is not null);

                var cache = new Dictionary<object, object>();
                AddResourcesToCache(rd, cache);
                return cache;

                static void AddResourcesToCache(ResourceDictionary rd, Dictionary<object, object> cache)
                {
                    if (rd._mergedDictionaries is not null)
                    {
                        foreach (ResourceDictionary mergedDictionary in rd._mergedDictionaries.InternalItems)
                        {
                            AddResourcesToCache(mergedDictionary, cache);
                        }
                    }

                    foreach (var kvp in rd._baseDictionary)
                    {
                        switch (kvp.Key)
                        {
                            case Type type:
                                Debug.Assert(kvp.Value is Style);
                                cache[type] = kvp.Value;
                                break;

                            case DataTemplateKey templateKey:
                                Debug.Assert(kvp.Value is DataTemplate);
                                cache[templateKey] = kvp.Value;
                                break;
                        }
                    }
                }
            }

            public static bool HasImplicitResources(ResourceDictionary rd)
            {
                Debug.Assert(rd is not null);

                if (rd.HasImplicitStyles || rd.HasImplicitDataTemplates)
                {
                    return true;
                }

                if (rd._mergedDictionaries is not null)
                {
                    foreach (ResourceDictionary mergedDictionary in rd._mergedDictionaries.InternalItems)
                    {
                        if (HasImplicitResources(mergedDictionary))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
