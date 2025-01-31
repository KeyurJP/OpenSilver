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

using OpenSilver.Internal.Media.Animation;

namespace System.Windows.Media.Animation;

/// <summary>
/// Abstract class that, when implemented, defines an animation segment with its own target value and interpolation method
/// for a <see cref="CharAnimationUsingKeyFrames"/>.
/// </summary>
public abstract class CharKeyFrame : DependencyObject, IKeyFrame<char>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharKeyFrame"/> class.
    /// </summary>
    protected CharKeyFrame() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharKeyFrame"/> class that has the specified target <see cref="Value"/>.
    /// </summary>
    /// <param name="value">
    /// The <see cref="Value"/> of the new <see cref="CharKeyFrame"/> instance.
    /// </param>
    protected CharKeyFrame(char value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharKeyFrame"/> class that has the specified target <see cref="Value"/>
    /// and <see cref="KeyTime"/>.
    /// </summary>
    /// <param name="value">
    /// The <see cref="Value"/> of the new <see cref="CharKeyFrame"/> instance.
    /// </param>
    /// <param name="keyTime">
    /// The <see cref="KeyTime"/> of the new <see cref="CharKeyFrame"/> instance.
    /// </param>
    protected CharKeyFrame(char value, KeyTime keyTime)
    {
        Value = value;
        KeyTime = keyTime;
    }

    /// <summary>
    /// Identifies the <see cref="KeyTime"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty KeyTimeProperty =
        DependencyProperty.Register(
            nameof(KeyTime),
            typeof(KeyTime),
            typeof(CharKeyFrame),
            new PropertyMetadata(KeyTime.Uniform));

    /// <summary>
    /// Gets or sets the time at which the key frame's target <see cref="Value"/> should be reached.
    /// </summary>
    /// <returns>
    /// The time at which the key frame's current value should be equal to its <see cref="Value"/> property.
    /// The default value is <see cref="KeyTime.Uniform"/>.
    /// </returns>
    public KeyTime KeyTime
    {
        get => (KeyTime)GetValue(KeyTimeProperty);
        set => SetValueInternal(KeyTimeProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Value"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(char),
            typeof(CharKeyFrame),
            new PropertyMetadata((char)0));

    /// <summary>
    /// Gets or sets the key frame's target value.
    /// </summary>
    /// <returns>
    /// The key frame's target value, which is the value of this key frame at its specified <see cref="KeyTime"/>.
    /// The default value is 0.
    /// </returns>
    public char Value
    {
        get => (char)GetValue(ValueProperty);
        set => SetValueInternal(ValueProperty, value);
    }

    object IKeyFrame.Value
    {
        get => Value;
        set => Value = (char)value;
    }

    /// <summary>
    /// Returns the interpolated value of a specific key frame at the progress increment provided.
    /// </summary>
    /// <param name="baseValue">
    /// The value to animate from.
    /// </param>
    /// <param name="keyFrameProgress">
    /// A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.
    /// </param>
    /// <returns>
    /// The output value of this key frame given the specified base value and progress.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Occurs if keyFrameProgress is not between 0.0 and 1.0, inclusive.
    /// </exception>
    public char InterpolateValue(char baseValue, double keyFrameProgress)
    {
        if (keyFrameProgress < 0.0 || keyFrameProgress > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(keyFrameProgress));
        }

        return InterpolateValueCore(baseValue, keyFrameProgress);
    }

    /// <summary>
    /// Calculates the value of a key frame at the progress increment provided.
    /// </summary>
    /// <param name="baseValue">
    /// The value to animate from; typically the value of the previous key frame.
    /// </param>
    /// <param name="keyFrameProgress">
    /// A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.
    /// </param>
    /// <returns>
    /// The output value of this key frame given the specified base value and progress.
    /// </returns>
    protected virtual char InterpolateValueCore(char baseValue, double keyFrameProgress) => baseValue;
}

/// <summary>
/// Animates from the <see cref="char"/> value of the previous key frame to its own <see cref="CharKeyFrame.Value"/>
/// using discrete interpolation.
/// </summary>
public sealed class DiscreteCharKeyFrame : CharKeyFrame
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteCharKeyFrame"/> class.
    /// </summary>
    public DiscreteCharKeyFrame() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteCharKeyFrame"/> class with the specified ending value.
    /// </summary>
    /// <param name="value">
    /// Ending value (also known as "target value") for the key frame.
    /// </param>
    public DiscreteCharKeyFrame(char value)
        : base(value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteCharKeyFrame"/> class with the specified ending value and key time.
    /// </summary>
    /// <param name="value">
    /// Ending value (also known as "target value") for the key frame.
    /// </param>
    /// <param name="keyTime">
    /// Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.
    /// </param>
    public DiscreteCharKeyFrame(char value, KeyTime keyTime)
        : base(value, keyTime)
    {
    }

    /// <inheritdoc />
    protected override char InterpolateValueCore(char baseValue, double keyFrameProgress) =>
        keyFrameProgress switch
        {
            < 1.0 => baseValue,
            _ => Value
        };
}
