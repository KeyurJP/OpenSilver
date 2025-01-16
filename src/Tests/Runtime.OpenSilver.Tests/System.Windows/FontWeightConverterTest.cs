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
using OpenSilver.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Tests
{
    [TestClass]
    public class FontWeightConverterTest : TypeConverterTestBase
    {
        protected override TypeConverter Converter { get; } =
            new FontWeightConverter();

        [TestMethod]
        public void CanConvertFrom_String_Should_Return_True()
        {
            Assert.IsTrue(Converter.CanConvertFrom(typeof(string)));
        }

        [TestMethod]
        public void CanConvertFrom_Bool_Should_Return_False()
        {
            Assert.IsFalse(Converter.CanConvertFrom(typeof(bool)));
        }

        [TestMethod]
        public void CanConvertTo_String_Should_Return_True()
        {
            Assert.IsTrue(Converter.CanConvertTo(typeof(string)));
        }

        [TestMethod]
        public void CanConvertTo_Bool_Should_Return_False()
        {
            Assert.IsFalse(Converter.CanConvertTo(typeof(bool)));
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_FontWeight_1()
        {
            Assert.AreEqual(Converter.ConvertFrom("Thin"), FontWeights.Thin);
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_FontWeight_2()
        {
            Assert.AreEqual(Converter.ConvertFrom("thin"), FontWeights.Thin);
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_FontWeight_3()
        {
            Assert.AreEqual(Converter.ConvertFrom("100"), FontWeights.Thin);
        }

        [TestMethod]
        public void ConvertFrom_Null_Should_Throw_NotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertFrom(null)
            );
        }

        [TestMethod]
        public void ConvertFrom_Bool_Should_Throw_ArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(
                () => Converter.ConvertFrom(true)
            );
        }

        [TestMethod]
        public void ConvertFrom_Should_Throw_FormatException()
        {
            Assert.ThrowsException<FormatException>(
                () => Converter.ConvertFrom("whatever")
            );
        }

        [TestMethod]
        public void ConvertTo_String()
        {
            Assert.AreEqual(Converter.ConvertTo(FontWeights.Light, typeof(string)), "Light");
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Converter.ConvertTo(new FontWeight(), null)
            );
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_NotSupportedException_1()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertTo(new FontWeight(), typeof(bool))
            );
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_NotSupportedException_2()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertTo("Hi", typeof(string))
            );
        }
    }
}
