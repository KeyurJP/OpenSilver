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
using System.Diagnostics;
using OpenSilver.Internal;
using OpenSilver.Internal.Controls;

namespace System.Windows.Controls
{
    public sealed class ItemCollection : PresentationFrameworkCollection<object>, INotifyCollectionChanged
    {
        private readonly IInternalFrameworkElement _modelParent;
        private readonly CollectionChangedHelper _collectionChanged;

        private IEnumerable _itemsSource; // base collection
        private WeakEventListener<ItemCollection, INotifyCollectionChanged, NotifyCollectionChangedEventArgs> _collectionChangedListener;

        private bool _isUsingListWrapper;
        private ListWrapper _listWrapper;

        internal ItemCollection(IInternalFrameworkElement parent)
        {
            _modelParent = parent;
            _collectionChanged = new(this);
        }

        internal override bool IsFixedSizeImpl => IsUsingItemsSource;

        internal override bool IsReadOnlyImpl => IsUsingItemsSource;

        internal override void AddOverride(object value)
        {
            if (IsUsingItemsSource)
            {
                throw new InvalidOperationException(Strings.ItemsSourceInUse);
            }

            _collectionChanged.CheckReentrancy();

            SetModelParent(value);
            AddInternal(value);

            _collectionChanged.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, InternalCount - 1);
        }

        internal override void CopyToImpl(object[] array, int index)
        {
            if (IsUsingItemsSource)
            {
                SourceList.CopyTo(array, index);
            }
            else
            {
                base.CopyToImpl(array, index);
            }
        }

        internal override void ClearOverride()
        {
            if (IsUsingItemsSource)
            {
                throw new InvalidOperationException(Strings.ItemsSourceInUse);
            }

            _collectionChanged.CheckReentrancy();

            foreach (var item in InternalItems)
            {
                ClearModelParent(item);
            }

            ClearInternal();

            _collectionChanged.OnCollectionReset();
        }

        internal override void InsertOverride(int index, object value)
        {
            if (IsUsingItemsSource)
            {
                throw new InvalidOperationException(Strings.ItemsSourceInUse);
            }

            _collectionChanged.CheckReentrancy();

            SetModelParent(value);
            InsertInternal(index, value);

            _collectionChanged.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
        }

        internal override void RemoveAtOverride(int index)
        {
            if (IsUsingItemsSource)
            {
                throw new InvalidOperationException(Strings.ItemsSourceInUse);
            }

            _collectionChanged.CheckReentrancy();

            object removedItem = GetItemInternal(index);
            ClearModelParent(removedItem);
            RemoveAtInternal(index);

            _collectionChanged.OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        internal override object GetItemOverride(int index) => IsUsingItemsSource ? SourceList[index] : GetItemInternal(index);

        internal override void SetItemOverride(int index, object value)
        {
            if (IsUsingItemsSource)
            {
                throw new InvalidOperationException(Strings.ItemsSourceInUse);
            }

            _collectionChanged.CheckReentrancy();

            object originalItem = GetItemInternal(index);
            ClearModelParent(originalItem);
            SetModelParent(value);
            SetItemInternal(index, value);

            _collectionChanged.OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, value, index);
        }

        internal override int IndexOfImpl(object value) => IsUsingItemsSource ? SourceList.IndexOf(value) : base.IndexOfImpl(value);

        internal override IEnumerator<object> GetEnumeratorImpl() => IsUsingItemsSource ? new Enumerator(this) : base.GetEnumeratorImpl();

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _collectionChanged.CollectionChanged += value;
            remove => _collectionChanged.CollectionChanged -= value;
        }

        internal IEnumerator LogicalChildren => IsUsingItemsSource ? EmptyEnumerator.Instance : GetEnumerator();

        internal bool IsUsingItemsSource { get; private set; }

        internal IList SourceList => _isUsingListWrapper ? _listWrapper : (IList)_itemsSource;

        internal override int CountImpl => IsUsingItemsSource ? SourceList.Count : base.CountImpl;

        internal void SetItemsSource(IEnumerable value)
        {
            if (!IsUsingItemsSource && InternalCount != 0)
            {
                throw new InvalidOperationException(Strings.CannotUseItemsSource);
            }

            int previousCount = Count;

            TryUnsubscribeFromCollectionChangedEvent();

            _itemsSource = value;
            IsUsingItemsSource = true;

            TrySubscribeToCollectionChangedEvent(value);

            InitializeSourceList(value);

            UpdateCountProperty(previousCount, SourceList.Count);

            _collectionChanged.OnCollectionReset();
        }

        internal void ClearItemsSource()
        {
            if (IsUsingItemsSource)
            {
                int previousCount = Count;

                // return to normal mode
                TryUnsubscribeFromCollectionChangedEvent();

                _itemsSource = null;
                _listWrapper = null;
                IsUsingItemsSource = false;
                _isUsingListWrapper = false;

                UpdateCountProperty(previousCount, InternalCount);

                _collectionChanged.OnCollectionReset();
            }
        }

        private void InitializeSourceList(IEnumerable sourceCollection)
        {
            if (sourceCollection is not IList)
            {
                _listWrapper = new ListWrapper(sourceCollection);
                _isUsingListWrapper = true;
            }
            else
            {
                _listWrapper = null;
                _isUsingListWrapper = false;
            }
        }

        private void ValidateCollectionChangedEventArgs(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count != 1)
                    {
                        throw new NotSupportedException(Strings.RangeActionsNotSupported);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count != 1)
                    {
                        throw new NotSupportedException(Strings.RangeActionsNotSupported);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    if (e.NewItems.Count != 1 || e.OldItems.Count != 1)
                    {
                        throw new NotSupportedException(Strings.RangeActionsNotSupported);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new NotSupportedException(string.Format(Strings.UnexpectedCollectionChangeAction, e.Action));
            }
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateCollectionChangedEventArgs(e);

            int previousCount = Count;

            // Update list wrapper
            if (_isUsingListWrapper)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _listWrapper.Insert(e.NewStartingIndex, e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        _listWrapper.RemoveAt(e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        _listWrapper.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        _listWrapper[e.OldStartingIndex] = e.NewItems[0];
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _listWrapper.Refresh();
                        break;
                }
            }

            UpdateCountProperty(previousCount, Count);

            // Raise collection changed
            _collectionChanged.OnCollectionChanged(e);
        }

        private void SetModelParent(object item) => _modelParent?.AddLogicalChild(item);

        private void ClearModelParent(object item) => _modelParent?.RemoveLogicalChild(item);

        private void TrySubscribeToCollectionChangedEvent(IEnumerable collection)
        {
            if (collection is INotifyCollectionChanged incc)
            {
                _collectionChangedListener = new(this, incc)
                {
                    OnEventAction = static (instance, source, args) => instance.OnSourceCollectionChanged(source, args),
                    OnDetachAction = static (listener, source) => source.CollectionChanged -= listener.OnEvent,
                };
                incc.CollectionChanged += _collectionChangedListener.OnEvent;
            }
        }

        private void TryUnsubscribeFromCollectionChangedEvent()
        {
            if (_collectionChangedListener != null)
            {
                _collectionChangedListener.Detach();
                _collectionChangedListener = null;
            }
        }

        private sealed class ListWrapper : List<object>
        {
            private readonly IEnumerable _sourceCollection;

            public ListWrapper(IEnumerable source)
            {
                Debug.Assert(source != null);
                _sourceCollection = source;

                Refresh();
            }

            public void Refresh()
            {
                Clear();

                IEnumerator enumerator = _sourceCollection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Add(enumerator.Current);
                }
            }

            public void Move(int oldIndex, int newIndex)
            {
                if (oldIndex == newIndex)
                {
                    return;
                }

                var item = this[oldIndex];

                RemoveAt(oldIndex);
                Insert(newIndex, item);
            }
        }

        private readonly struct Enumerator : IEnumerator<object>
        {
            private readonly IEnumerator _enumerator;

            public Enumerator(ItemCollection itemCollection)
            {
                _enumerator = itemCollection.SourceList.GetEnumerator();
            }

            public object Current => _enumerator.Current;

            public void Dispose() { }

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }
    }
}