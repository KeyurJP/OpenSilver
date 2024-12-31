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
using System.Diagnostics;
using System.Windows.Media;

namespace OpenSilver.Internal;

internal static partial class Parsers
{
    private const int s_zeroChar = (int)'0';
    private const int s_aLower = (int)'a';
    private const int s_aUpper = (int)'A';

    private static int ParseHexChar(char c)
    {
        int intChar = (int)c;

        if ((intChar >= s_zeroChar) && (intChar <= (s_zeroChar + 9)))
        {
            return (intChar - s_zeroChar);
        }

        if ((intChar >= s_aLower) && (intChar <= (s_aLower + 5)))
        {
            return (intChar - s_aLower + 10);
        }

        if ((intChar >= s_aUpper) && (intChar <= (s_aUpper + 5)))
        {
            return (intChar - s_aUpper + 10);
        }

        throw new FormatException(Strings.Parsers_IllegalToken);
    }

    private static Color ParseHexColor(string trimmedColor)
    {
        int a, r, g, b;
        a = 255;

        if (trimmedColor.Length > 7)
        {
            a = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
            r = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
            g = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
            b = ParseHexChar(trimmedColor[7]) * 16 + ParseHexChar(trimmedColor[8]);
        }
        else if (trimmedColor.Length > 5)
        {
            r = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
            g = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
            b = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
        }
        else if (trimmedColor.Length > 4)
        {
            a = ParseHexChar(trimmedColor[1]);
            a = a + a * 16;
            r = ParseHexChar(trimmedColor[2]);
            r = r + r * 16;
            g = ParseHexChar(trimmedColor[3]);
            g = g + g * 16;
            b = ParseHexChar(trimmedColor[4]);
            b = b + b * 16;
        }
        else
        {
            r = ParseHexChar(trimmedColor[1]);
            r = r + r * 16;
            g = ParseHexChar(trimmedColor[2]);
            g = g + g * 16;
            b = ParseHexChar(trimmedColor[3]);
            b = b + b * 16;
        }

        return (Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b));
    }

    private static Color ParseScRgbColor(string trimmedColor, IFormatProvider formatProvider)
    {
        if (!trimmedColor.StartsWith("sc#", StringComparison.Ordinal))
        {
            throw new FormatException(Strings.Parsers_IllegalToken);
        }

        string tokens = trimmedColor.Substring(3, trimmedColor.Length - 3);

        // The tokenizer helper will tokenize a list based on the IFormatProvider.
        var th = new TokenizerHelper(tokens, formatProvider);

        float f0 = Convert.ToSingle(th.NextTokenRequired(), formatProvider);
        float f1 = Convert.ToSingle(th.NextTokenRequired(), formatProvider);
        float f2 = Convert.ToSingle(th.NextTokenRequired(), formatProvider);

        if (th.NextToken())
        {
            float f3 = Convert.ToSingle(th.GetCurrentToken(), formatProvider);

            // We should be out of tokens at this point
            if (th.NextToken())
            {
                throw new FormatException(Strings.Parsers_IllegalToken);
            }

            return Color.FromScRgb(f0, f1, f2, f3);
        }
        else
        {
            return Color.FromScRgb(1.0f, f0, f1, f2);
        }
    }

    /// <summary>
    /// ParseColor
    /// <param name="color"> string with color description </param>
    /// <param name="formatProvider">IFormatProvider for processing string</param>
    /// </summary>
    internal static Color ParseColor(string color, IFormatProvider formatProvider)
    {
        string trimmedColor = Colors.MatchColor(
            color, out bool isPossibleKnowColor, out bool isNumericColor, out bool isScRgbColor
        );

        //Is it a number?
        if (isNumericColor)
        {
            return ParseHexColor(trimmedColor);
        }
        else if (isScRgbColor)
        {
            return ParseScRgbColor(trimmedColor, formatProvider);
        }
        else
        {
            Debug.Assert(isPossibleKnowColor);

            if (!Enum.TryParse(trimmedColor, true, out Colors.KnownColor kc))
            {
                throw new FormatException(Strings.Parsers_IllegalToken);
            }

            return Color.FromUInt32((uint)kc);
        }
    }

    /// <summary>
    /// ParseTransform - parse a Transform from a string
    /// </summary>
    internal static Transform ParseTransform(string transformString, IFormatProvider formatProvider)
    {
        Matrix matrix = Matrix.Parse(transformString);

        return new MatrixTransform(matrix);
    }

    /// <summary>
    /// Parse a PathFigureCollection string.
    /// </summary>
    internal static PathFigureCollection ParsePathFigureCollection(string pathString, IFormatProvider formatProvider)
    {
        var context = new PathStreamGeometryContext();
        var parser = new AbbreviatedGeometryParser();
        parser.ParseToGeometryContext(context, pathString, 0 /* curIndex */);
        PathGeometry pathGeometry = context.GetPathGeometry();
        return pathGeometry.Figures;
    }
}
