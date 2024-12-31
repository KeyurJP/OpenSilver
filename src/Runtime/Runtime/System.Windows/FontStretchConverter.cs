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
/// Converts instances of <see cref="FontStretch"/> to and from other type representations.
/// </summary>
public sealed class FontStretchConverter : TypeConverter
{
    /// <summary>
    /// Determines if conversion from a specified type to a <see cref="FontStretch"/> value is possible.
    /// </summary>
    /// <param name="context">
    /// Context information of a type.
    /// </param>
    /// <param name="sourceType">
    /// The type of the source that is being evaluated for conversion.
    /// </param>
    /// <returns>
    /// true if t can create a <see cref="FontStretch"/>; otherwise, false.
    /// </returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

    /// <summary>
    /// Determines whether an instance of <see cref="FontStretch"/> can be converted to a different type.
    /// </summary>
    /// <param name="context">
    /// Context information of a type.
    /// </param>
    /// <param name="destinationType">
    /// The desired type that this instance of <see cref="FontStretch"/> is being evaluated for conversion to.
    /// </param>
    /// <returns>
    /// true if the converter can convert <see cref="FontStretch"/> to destinationType; otherwise, false.
    /// </returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string);

    /// <summary>
    /// Attempts to convert a specified object to an instance of <see cref="FontStretch"/>.
    /// </summary>
    /// <param name="context">
    /// Context information of a type.
    /// </param>
    /// <param name="culture">
    /// <see cref="CultureInfo"/> of the type being converted.
    /// </param>
    /// <param name="value">
    /// The object being converted.
    /// </param>
    /// <returns>
    /// The instance of <see cref="FontStretch"/> created from the converted value.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// value is not a valid type for conversion.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is null)
        {
            throw GetConvertFromException(value);
        }

        if (value is not string s)
        {
            throw new ArgumentException(string.Format(Strings.General_BadType, nameof(ConvertFrom)), nameof(value));
        }

        var fontStretch = new FontStretch();
        if (!FontStretches.FontStretchStringToKnownStretch(s, culture, ref fontStretch))
        {
            throw new FormatException(Strings.Parsers_IllegalToken);
        }

        return fontStretch;
    }

    /// <summary>
    /// Attempts to convert an instance of <see cref="FontStretch"/> to a specified type.
    /// </summary>
    /// <param name="context">
    /// Context information of a type.
    /// </param>
    /// <param name="culture">
    /// <see cref="CultureInfo"/> of the type being converted.
    /// </param>
    /// <param name="value">
    /// The instance of <see cref="FontStretch"/> to convert.
    /// </param>
    /// <param name="destinationType">
    /// The type this instance of <see cref="FontStretch"/> is converted to.
    /// </param>
    /// <returns>
    /// The object created from the converted instance of <see cref="FontStretch"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// destinationType is null.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// value is not an instance of <see cref="FontStretch"/> -or- destinationType is not a valid destination type.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType is null)
        {
            throw new ArgumentNullException(nameof(destinationType));
        }

        if (destinationType == typeof(string))
        {
            if (value is FontStretch instance)
            {
                return ((IFormattable)instance).ToString(null, culture);
            }
        }

        throw GetConvertToException(value, destinationType);
    }
}
