using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSilver;
using System.Windows.Controls;

namespace System.Windows.Input.Tests
{
    [TestClass]
    public class FocusManagerTest
    {
        [TestMethod]
        public void GetFocusedElement()
        {
            var element = new Control();
            FocusManager.SetFocusedElement(Window.Current, element);

            Assert.AreSame(FocusManager.GetFocusedElement(), element);
        }

        [TestMethod]
        public void GetFocusedElement_WithScopeParameter()
        {
            var window = new Window();
            var otherWindow = new Window();

            var element = new Control();
            var otherElement = new Control();

            FocusManager.SetFocusedElement(window, element);

            var focusedElement = FocusManager.GetFocusedElement(window);
            Assert.AreSame(focusedElement, element);

            FocusManager.SetFocusedElement(window, otherElement);

            focusedElement = FocusManager.GetFocusedElement(window);
            Assert.AreSame(focusedElement, otherElement);
            Assert.IsNull(FocusManager.GetFocusedElement(otherWindow));
        }

        [TestMethod]
        public void GetFocusedElement_WithNonWindowScope_ShouldReturnNull()
        {
            Assert.IsNull(FocusManager.GetFocusedElement(new TextBlock()));
        }

        [TestMethod]
        public void GetFocusedElement_WithNoCurrentWindow_ShouldReturnNull()
        {
            var window = Window.Current;
            Window.Current = null;
            
            try
            {
                Assert.IsNull(FocusManager.GetFocusedElement());
            }
            finally
            {
                Window.Current = window;
            }
        }

        [TestMethod]
        public void GetFocusedElement_WithNoFocusedElement_ShouldReturnNull()
        {
            var window = Window.Current;
            Window.Current = new Window();

            try
            {
                Assert.IsNull(FocusManager.GetFocusedElement());
            }
            finally
            {
                Window.Current = window;
            }
        }

        [TestMethod]
        public void GetFocusedElement_ControlFocus()
        {
            using (var element = new FocusableControlWrapper<Control>(new Control()))
            {
                element.Control.Focus();
                Assert.AreSame(FocusManager.GetFocusedElement(), element.Control);
            }
        }

        [TestMethod]
        public void GetFocusedElement_TwoControls_ControlFocus()
        {
            var firstElement = new FocusableControlWrapper<Control>(new Control());
            var secondElement = new FocusableControlWrapper<Control>(new Control());

            try
            {
                firstElement.Control.Focus();
                Assert.AreSame(FocusManager.GetFocusedElement(), firstElement.Control);

                secondElement.Control.Focus();
                Assert.AreNotSame(FocusManager.GetFocusedElement(), firstElement.Control);
                Assert.AreSame(FocusManager.GetFocusedElement(), secondElement.Control);
            }
            finally
            {
                firstElement.Dispose();
                firstElement.Dispose();
            }
        }
    }
}
