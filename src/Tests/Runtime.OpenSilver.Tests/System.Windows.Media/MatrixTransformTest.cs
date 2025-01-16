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
    public class MatrixTransformTest
    {
        [TestMethod]
        public void Inverse_When_Not_Invertible()
        {
            var transform = new MatrixTransform(MatrixTest.GetSingularMatrix(2, 4));
            var invertedTransform = transform.Inverse;

            Assert.IsNull(invertedTransform);
        }

        [TestMethod]
        public void Inverse_When_Invertible()
        {
            var transform = new MatrixTransform(MatrixTest.GetIncrementalMatrix(0, 1));
            var invertedTransform = transform.Inverse as MatrixTransform;

            Assert.IsNotNull(invertedTransform);

            var m = invertedTransform.Matrix;

            Assert.AreEqual(m.M11, -1.5);
            Assert.AreEqual(m.M12, 0.5);
            Assert.AreEqual(m.M21, 1);
            Assert.AreEqual(m.M22, 0);
            Assert.AreEqual(m.OffsetX, 1);
            Assert.AreEqual(m.OffsetY, -2);
        }

        [TestMethod]
        public void TransformBounds()
        {
            var rect = new Rect(-1, 1, 5, 2);
            var transform = new MatrixTransform(MatrixTest.GetIncrementalMatrix(10, 3));
            var result = transform.TransformBounds(rect);
            Assert.AreEqual(result.X, 28);
            Assert.AreEqual(result.Y, 31);
            Assert.AreEqual(result.Width, 82);
            Assert.AreEqual(result.Height, 103);
        }

        [TestMethod]
        public void TryTransform()
        {
            var point = new Point(1, 2);
            var transform = new MatrixTransform(MatrixTest.GetIncrementalMatrix(-5, 2));
            var result = transform.TryTransform(point, out var outPoint);
            Assert.IsTrue(result);
            Assert.AreEqual(outPoint.X, -4);
            Assert.AreEqual(outPoint.Y, 4);
        }
    }
}
