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
using System.Globalization;
using System.Windows.Markup;
using OpenSilver.Internal.Media.Animation;

namespace System.Windows.Media.Animation;

/// <summary>
/// Animates the value of an <see cref="object"/> property along a set of <see cref="KeyFrames"/>
/// over a specified <see cref="Duration"/>.
/// </summary>
[ContentProperty(nameof(KeyFrames))]
public sealed class ObjectAnimationUsingKeyFrames : AnimationTimeline, IKeyFrameAnimation<object>
{
    private ObjectKeyFrameCollection _frames;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectAnimationUsingKeyFrames"/> class.
    /// </summary>
    public ObjectAnimationUsingKeyFrames() { }

    /// <summary>
    /// Gets the collection of <see cref="ObjectKeyFrame"/> objects
    /// that define the animation.
    /// </summary>
    /// <returns>
    /// The collection of <see cref="ObjectKeyFrame"/> objects that
    /// define the animation. The default is an empty collection.
    /// </returns>
    public ObjectKeyFrameCollection KeyFrames
    {
        get
        {
            if (_frames is null)
            {
                SetKeyFrames(new());
            }
            return _frames;
        }
        set { SetKeyFrames(value); }
    }

    IKeyFrameCollection<object> IKeyFrameAnimation<object>.KeyFrames => _frames;

    protected sealed override Duration GetNaturalDurationCore() =>
        KeyFrameAnimationHelpers.GetLargestTimeSpanKeyTime(this);

    internal sealed override TimelineClock CreateClock(bool isRoot) =>
       new AnimationClock<object>(this, isRoot, new ObjectKeyFramesAnimator(this));

    private void SetKeyFrames(ObjectKeyFrameCollection keyFrames)
    {
        if (_frames is not null)
        {
            RemoveSelfAsInheritanceContext(_frames, null);
        }

        _frames = keyFrames;

        if (_frames is not null)
        {
            ProvideSelfAsInheritanceContext(_frames, null);
        }
    }

    private sealed class ObjectKeyFramesAnimator : IValueAnimator<object>
    {
        private readonly KeyFramesAnimator<object> _baseAnimator;

        public ObjectKeyFramesAnimator(ObjectAnimationUsingKeyFrames animation)
        {
            Debug.Assert(animation is not null);
            _baseAnimator = new KeyFramesAnimator<object>(animation);
        }

        public object GetCurrentValue(object initialValue, DependencyProperty dp, TimelineClock clock)
        {
            object value = _baseAnimator.GetCurrentValue(initialValue, dp, clock);

            if (value is not null && !dp.PropertyType.IsInstanceOfType(value))
            {
                if (value is Color color && dp.PropertyType == typeof(Brush))
                {
                    value = new SolidColorBrush(color);
                }
                else if (TypeConverterHelper.GetConverter(dp.PropertyType) is TypeConverter converter &&
                         converter.CanConvertFrom(value.GetType()))
                {
                    value = converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                }
            }

            return value;
        }
    }
}

/// <summary>
/// Represents a collection of <see cref="ObjectKeyFrame"/> objects that can be individually accessed by index.
/// </summary>
public sealed class ObjectKeyFrameCollection : PresentationFrameworkCollection<ObjectKeyFrame>, IKeyFrameCollection<object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectKeyFrameCollection"/> class.
    /// </summary>
    public ObjectKeyFrameCollection() { }

    internal override void AddOverride(ObjectKeyFrame keyFrame) => AddDependencyObjectInternal(keyFrame);

    internal override void ClearOverride() => ClearDependencyObjectInternal();

    internal override void InsertOverride(int index, ObjectKeyFrame keyFrame) => InsertDependencyObjectInternal(index, keyFrame);

    internal override void RemoveAtOverride(int index) => RemoveAtDependencyObjectInternal(index);

    internal override ObjectKeyFrame GetItemOverride(int index) => GetItemInternal(index);

    internal override void SetItemOverride(int index, ObjectKeyFrame keyFrame) => SetItemDependencyObjectInternal(index, keyFrame);

    IKeyFrame<object> IKeyFrameCollection<object>.this[int index] => GetItemInternal(index);
}