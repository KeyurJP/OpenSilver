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
    public class ScaleTransformTest
    {
        [TestMethod]
        public void Inverse()
        {
            var transform = new ScaleTransform { ScaleX = 10, ScaleY = -2 };
            var invertedTransform = transform.Inverse as MatrixTransform;

            Assert.IsNotNull(invertedTransform);

            var m = invertedTransform.Matrix;

            Assert.AreEqual(m.M11, 0.1);
            Assert.AreEqual(m.M12, 0);
            Assert.AreEqual(m.M21, 0);
            Assert.AreEqual(m.M22, -0.5);
            Assert.AreEqual(m.OffsetX, 0);
            Assert.AreEqual(m.OffsetY, 0);
        }

        [TestMethod]
        public void TransformBounds()
        {
            var rect = new Rect(1, 1, 100, 110);
            var transform = new ScaleTransform { ScaleX = -1.5, ScaleY = -2 };
            var result = transform.TransformBounds(rect);
            Assert.AreEqual(result.X, -151.5);
            Assert.AreEqual(result.Y, -222);
            Assert.AreEqual(result.Width, 150);
            Assert.AreEqual(result.Height, 220);
        }

        [TestMethod]
        public void TryTransform()
        {
            var point = new Point(-10, 1.5);
            var transform = new ScaleTransform { ScaleX = 100, ScaleY = 2 };
            var result = transform.TryTransform(point, out var outPoint);
            Assert.IsTrue(result);
            Assert.AreEqual(outPoint.X, -1000);
            Assert.AreEqual(outPoint.Y, 3);
        }
    }
}
