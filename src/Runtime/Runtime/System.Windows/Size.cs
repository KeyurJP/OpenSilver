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
using System.Globalization;
using OpenSilver.Internal;

namespace System.Windows;

/// <summary>
/// Describes the width and height of an object.
/// </summary>
[TypeConverter(typeof(SizeConverter))]
public struct Size : IFormattable
{
    internal double _width;
    internal double _height;

    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> structure and assigns it
    /// an initial width and height.
    /// </summary>
    /// <param name="width">
    /// The initial width of the instance of <see cref="Size"/>.
    /// </param>
    /// <param name="height">
    /// The initial height of the instance of <see cref="Size"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// width or height are less than 0.
    /// </exception>
    public Size(double width, double height)
    {
        if (width < 0 || height < 0)
        {
            throw new ArgumentException(Strings.Size_WidthAndHeightCannotBeNegative);
        }

        _width = width;
        _height = height;
    }

    /// <summary>
    /// Returns an instance of <see cref="Size"/> from a converted <see cref="string"/>.
    /// </summary>
    /// <param name="source">
    /// A <see cref="string"/> value to parse to a <see cref="Size"/> value.
    /// </param>
    /// <returns>
    /// An instance of <see cref="Size"/>.
    /// </returns>
    public static Size Parse(string source)
    {
        IFormatProvider formatProvider = CultureInfo.InvariantCulture;

        var th = new TokenizerHelper(source, formatProvider);

        Size size;

        string firstToken = th.NextTokenRequired();

        // The token will already have had whitespace trimmed so we can do a
        // simple string compare.
        if (firstToken == "Empty")
        {
            size = Empty;
        }
        else
        {
            size = new Size(
                Convert.ToDouble(firstToken, formatProvider),
                Convert.ToDouble(th.NextTokenRequired(), formatProvider));
        }

        // There should be no more tokens in this string.
        th.LastTokenRequired();

        return size;
    }

    /// <summary>
    /// Gets a value that represents a static empty <see cref="Size"/>.
    /// </summary>
    /// <returns>
    /// An empty instance of <see cref="Size"/>.
    /// </returns>
    public static Size Empty { get; } =
        new Size
        {
            _width = double.NegativeInfinity,
            _height = double.NegativeInfinity
        };

    /// <summary>
    /// Gets a value that indicates whether this instance of <see cref="Size"/> is <see cref="Empty"/>.
    /// </summary>
    public bool IsEmpty => _width < 0;

    /// <summary>
    /// Gets or sets the height of this instance of <see cref="Size"/>.
    /// </summary>
    /// <returns>
    /// The <see cref="Height"/> of this instance of <see cref="Size"/>, in pixels.
    /// The default is 0. The value cannot be negative.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Specified a value less than 0.
    /// </exception>
    public double Height
    {
        get => _height;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException(Strings.Size_HeightCannotBeNegative);
            }

            _height = value;
        }
    }

    /// <summary>
    /// Gets or sets the width of this instance of <see cref="Size"/>.
    /// </summary>
    /// <returns>
    /// The <see cref="Width"/> of this instance of <see cref="Size"/>, in pixels.
    /// The default value is 0. The value cannot be negative.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Specified a value less than 0.
    /// </exception>
    public double Width
    {
        get => _width;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException(Strings.Size_WidthCannotBeNegative);
            }

            _width = value;
        }
    }

    /// <summary>
    /// Compares an object to an instance of <see cref="Size"/> for equality.
    /// </summary>
    /// <param name="o">
    /// The object to compare.
    /// </param>
    /// <returns>
    /// true if the sizes are equal; otherwise, false.
    /// </returns>
    public override bool Equals(object o) => o is Size size && size == this;

    /// <summary>
    /// Compares a value to an instance of <see cref="Size"/> for equality.
    /// </summary>
    /// <param name="value">
    /// The size to compare to this current instance of <see cref="Size"/>.
    /// </param>
    /// <returns>
    /// true if the instances of <see cref="Size"/> are equal; otherwise, false.
    /// </returns>
    public bool Equals(Size value) => this == value;

    /// <summary>
    /// Compares two instances of <see cref="Size"/> for equality.
    /// </summary>
    /// <param name="size1">
    /// The first instance of <see cref="Size"/> to compare.
    /// </param>
    /// <param name="size2">
    /// The second instance of <see cref="Size"/> to compare.
    /// </param>
    /// <returns>
    /// true if the instances of <see cref="Size"/> are equal; otherwise, false.
    /// </returns>
    public static bool Equals(Size size1, Size size2) => size1 == size2;

    /// <summary>
    /// Gets the hash code for this instance of <see cref="Size"/>.
    /// </summary>
    /// <returns>
    /// The hash code for this instance of <see cref="Size"/>.
    /// </returns>
    public override int GetHashCode()
    {
        if (IsEmpty)
        {
            return 0;
        }
        else
        {
            // Perform field-by-field XOR of HashCodes
            return Width.GetHashCode() ^ Height.GetHashCode();
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Size"/>.
    /// </summary>
    /// <returns>
    /// A string representation of this <see cref="Size"/>.
    /// </returns>
    public override string ToString() => ConvertToString(null, null);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance of <see cref="Size"/>.
    /// </summary>
    /// <param name="provider">
    /// An object that provides a way to control formatting.
    /// </param>
    /// <returns>
    /// A <see cref="string"/> that represents this <see cref="Size"/> object.
    /// </returns>
    public string ToString(IFormatProvider provider) => ConvertToString(null, provider);

    /// <summary>
    /// Creates a string representation of this object based on the format string
    /// and IFormatProvider passed in.
    /// If the provider is null, the CurrentCulture is used.
    /// See the documentation for IFormattable for more information.
    /// </summary>
    /// <returns>
    /// A string representation of this object.
    /// </returns>
    string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);

    /// <summary>
    /// Compares two instances of <see cref="Size"/> for equality.
    /// </summary>
    /// <param name="size1">
    /// The first instance of <see cref="Size"/> to compare.
    /// </param>
    /// <param name="size2">
    /// The second instance of <see cref="Size"/> to compare.
    /// </param>
    /// <returns>
    /// true if the two instances of <see cref="Size"/> are equal; otherwise false.
    /// </returns>
    public static bool operator ==(Size size1, Size size2)
        => size1.Width == size2.Width && size1.Height == size2.Height;

    /// <summary>
    /// Compares two instances of <see cref="Size"/> for inequality.
    /// </summary>
    /// <param name="size1">
    /// The first instance of <see cref="Size"/> to compare.
    /// </param>
    /// <param name="size2">
    /// The second instance of <see cref="Size"/> to compare.
    /// </param>
    /// <returns>
    /// true if the instances of <see cref="Size"/> are not equal; otherwise false.
    /// </returns>
    public static bool operator !=(Size size1, Size size2) => !(size1 == size2);

    /// <summary>
    /// Explicitly converts an instance of <see cref="Size"/> to an instance of <see cref="Point"/>.
    /// </summary>
    /// <param name="size">
    /// The <see cref="Size"/> value to be converted.
    /// </param>
    /// <returns>
    /// A <see cref="Point"/> equal in value to this instance of <see cref="Size"/>.
    /// </returns>
    public static explicit operator Point(Size size) => new(size._width, size._height);

    /// <summary>
    /// Explicitly converts an instance of <see cref="Size"/> to an instance of <see cref="Vector"/>.
    /// </summary>
    /// <param name="size">
    /// The <see cref="Size"/> value to be converted.
    /// </param>
    /// <returns>
    /// A <see cref="Vector"/> equal in value to this instance of <see cref="Size"/>.
    /// </returns>
    public static explicit operator Vector(Size size) => new(size._width, size._height);

    /// <summary>
    /// Creates a string representation of this object based on the format string
    /// and IFormatProvider passed in.
    /// If the provider is null, the CurrentCulture is used.
    /// See the documentation for IFormattable for more information.
    /// </summary>
    /// <returns>
    /// A string representation of this object.
    /// </returns>
    internal string ConvertToString(string format, IFormatProvider provider)
    {
        if (IsEmpty)
        {
            return "Empty";
        }

        // Helper to get the numeric list separator for a given culture.
        char separator = TokenizerHelper.GetNumericListSeparator(provider);
        return string.Format(provider,
                             "{1:" + format + "}{0}{2:" + format + "}",
                             separator,
                             _width,
                             _height);
    }
}