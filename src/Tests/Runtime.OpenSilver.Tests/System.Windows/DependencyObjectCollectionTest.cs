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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Tests
{
    [TestClass]
    public class DependencyObjectCollectionTest
    {
        [TestMethod]
        public void DependencyObjectCollection_IndexOf_ItemNotFound_ReturnsMinusOne()
        {
            DependencyObjectCollection<DependencyObject> dependencyObjectCollection =
                new DependencyObjectCollection<DependencyObject>();

            Assert.AreEqual(dependencyObjectCollection.IndexOf(new DependencyObject()), -1);
        }

        [TestMethod]
        public void DependencyObjectCollection_IndexOf_NotDependencyObject_Throws()
        {
            DependencyObjectCollection<object> dependencyObjectCollection =
                new DependencyObjectCollection<object>();

            Assert.ThrowsException<ArgumentException>(
                () => dependencyObjectCollection.IndexOf("Some Item"),
                "item is not a DependencyObject.");
        }
    }
}
