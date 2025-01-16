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

using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Automation.Peers.Tests
{
    [TestClass]
    public class PasswordBoxAutomationPeerTest
    {
        [TestMethod]
        public void IsPassword_Should_Return_True()
        {
            Assert.IsTrue(new PasswordBoxAutomationPeer(new PasswordBox()).IsPassword());
        }

        [TestMethod]
        public void IValueProvider_SetValue_Should_Throw_ElementNotEnabledException()
        {
            var passwordBox = new PasswordBox
            {
                IsEnabled = false,
            };
            var peer = new PasswordBoxAutomationPeer(passwordBox);
            var provider = peer.GetPattern(PatternInterface.Value) as IValueProvider;

            Assert.IsNotNull(provider);
            Assert.ThrowsException<ElementNotEnabledException>(() => provider.SetValue("password"));
        }

        [TestMethod]
        public void IValueProvider_SetValue_Should_Throw_ArgumentNullException()
        {
            var passwordBox = new PasswordBox();
            var peer = new PasswordBoxAutomationPeer(passwordBox);
            var provider = peer.GetPattern(PatternInterface.Value) as IValueProvider;

            Assert.IsNotNull(provider);
            Assert.ThrowsException<ArgumentNullException>(() => provider.SetValue(null));
        }

        [TestMethod]
        public void IValueProvider_SetValue_Should_Set_Password()
        {
            var passwordBox = new PasswordBox();
            var peer = new PasswordBoxAutomationPeer(passwordBox);
            var provider = peer.GetPattern(PatternInterface.Value) as IValueProvider;

            Assert.IsNotNull(provider);

            provider.SetValue("password");

            Assert.AreEqual(passwordBox.Password, "password");
        }

        [TestMethod]
        public void IValueProvider_Value_Should_Throw_InvalidOperationException()
        {
            var peer = new PasswordBoxAutomationPeer(new PasswordBox());
            var provider = peer.GetPattern(PatternInterface.Value) as IValueProvider;

            Assert.IsNotNull(provider);
            Assert.ThrowsException<InvalidOperationException>(() => provider.Value);
        }

        [TestMethod]
        public void IValueProvider_IsReadOnly_Should_Return_False()
        {
            var peer = new PasswordBoxAutomationPeer(new PasswordBox());
            var provider = peer.GetPattern(PatternInterface.Value) as IValueProvider;

            Assert.IsNotNull(provider);
            Assert.IsFalse(provider.IsReadOnly);
        }
    }
}
