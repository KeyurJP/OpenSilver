
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
    public class MatrixConverterTest : TypeConverterTestBase
    {
        protected override TypeConverter Converter { get; } =
            new MatrixConverter();

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
        public void ConvertFrom_String_Should_Return_Matrix_Identity()
        {
            Assert.AreEqual(Converter.ConvertFrom("Identity"), Matrix.Identity);
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_Matrix_1()
        {
            Assert.AreEqual(Converter.ConvertFrom("1,2,3,4,5,6"), new Matrix(1, 2, 3, 4, 5, 6));
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_Matrix_2()
        {
            Assert.AreEqual(Converter.ConvertFrom("1 2 3 4 5 6"), new Matrix(1, 2, 3, 4, 5, 6));
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Return_Matrix_3()
        {
            Assert.AreEqual(Converter.ConvertFrom("  1,2 3, 4 5  6 "), new Matrix(1, 2, 3, 4, 5, 6));
        }

        [TestMethod]
        public void ConvertFrom_String_Should_Throw_InvalidOperationException()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => Converter.ConvertFrom("1")
            );
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
            Assert.AreEqual(Converter.ConvertTo(new Matrix(1, 2, 3, 4, 5, 6), typeof(string)), "1,2,3,4,5,6");
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Converter.ConvertTo(new Matrix(), null)
            );
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_NotSupportedException_1()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertTo("Hi", typeof(string))
            );
        }

        [TestMethod]
        public void ConvertTo_Should_Throw_NotSupportedException_2()
        {
            Assert.ThrowsException<NotSupportedException>(
                () => Converter.ConvertTo(new Matrix(), typeof(bool))
            );
        }
    }
}
