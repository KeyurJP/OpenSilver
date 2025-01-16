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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Media.Tests
{
    [TestClass]
    public class TranslateTransformTest
    {
        [TestMethod]
        public void Inverse()
        {
            var transform = new TranslateTransform { X = 123, Y = 321 };
            var invertedTransform = transform.Inverse as MatrixTransform;

            Assert.IsNotNull(invertedTransform);

            var m = invertedTransform.Matrix;

            Assert.AreEqual(m.M11, 1);
            Assert.AreEqual(m.M12, 0);
            Assert.AreEqual(m.M21, 0);
            Assert.AreEqual(m.M22, 1);
            Assert.AreEqual(m.OffsetX, -123);
            Assert.AreEqual(m.OffsetY, -321);
        }

        [TestMethod]
        public void TransformBounds()
        {
            var rect = new Rect(0, 0, 100, 100);
            var transform = new TranslateTransform { X = 100, Y = 200 };
            var result = transform.TransformBounds(rect);

            Assert.AreEqual(result.X, 100);
            Assert.AreEqual(result.Y, 200);
            Assert.AreEqual(result.Width, 100);
            Assert.AreEqual(result.Height, 100);
        }

        [TestMethod]
        public void TryTransform()
        {
            var point = new Point(0, 0);
            var transform = new TranslateTransform { X = 100, Y = 200 };
            var result = transform.TryTransform(point, out var outPoint);

            Assert.IsTrue(result);
            Assert.AreEqual(outPoint.X, 100);
            Assert.AreEqual(outPoint.Y, 200);
        }
    }
}
