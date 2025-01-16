
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

namespace System.Windows.Media.Tests
{
    [TestClass]
    public class DoubleCollectionConverterTest : TypeConverterTestBase
    {
        protected override TypeConverter Converter { get; } =
            new DoubleCollectionConverter();

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
        public void ConvertFrom_String_Should_Return_DoubleCollection_1()
        {
            object value = Converter.ConvertFrom("1.0, 2.0");

            Assert.IsInstanceOfType<DoubleCollection>(value);

            DoubleCollection doubleCollection = value as DoubleCollection;

            Assert.AreEqual(doubleCollection.Count, 2);
            Assert.AreEqual(doubleCollection[0], 1.0);
            Assert.AreEqual(doubleCollection[1], 2.0);
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_DoubleCollection_2()
        {
            object value = Converter.ConvertFrom(" 1.0  2.0 4.0  ");

            Assert.IsInstanceOfType<DoubleCollection>(value);

            DoubleCollection doubleCollection = value as DoubleCollection;

            Assert.AreEqual(doubleCollection.Count, 3);
            Assert.AreEqual(doubleCollection[0], 1.0);
            Assert.AreEqual(doubleCollection[1], 2.0);
            Assert.AreEqual(doubleCollection[2], 4.0);
        }

        [TestMethod]
        public void ConvertFrom_Null_Should_Throw_NotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertFrom(null)
            );
        }

        [TestMethod]
        public void ConvertFrom_Bool_Should_Throw_NotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertFrom(true)
            );
        }

        [TestMethod]
        public void ConvertTo_String()
        {
            var collection = new DoubleCollection() { 1.0, 2.0, 3.0 };
            
            Assert.AreEqual(Converter.ConvertTo(collection, typeof(string)), collection.ToString());
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Converter.ConvertTo(new DoubleCollection(), null)
            );
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_NotSupportedException_1()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertTo(new DoubleCollection() { 1.0, 2.0 }, typeof(int))
            );
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_NotSupportedException_2()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertTo(new Color(), typeof(string))
            );
        }
    }
}
