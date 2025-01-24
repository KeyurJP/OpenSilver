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

using CSHTML5.Internal;
using DotNetForHtml5;
using DotNetForHtml5.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenSilver;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Runtime.OpenSilver.Tests
{
    [TestClass]
    public class TestSetup
    {
        public static event EventHandler<ExecuteJavascriptEventArgs> ExecuteJavascript;

        private static void OnExecuteJavascript(ExecuteJavascriptEventArgs e)
        {
            ExecuteJavascript?.Invoke(null, e);
        }

        private static object ExecuteJsMock(string param)
        {
            var e = new ExecuteJavascriptEventArgs
            {
                Javascript = param
            };

            OnExecuteJavascript(e);

            if (e.Handled)
            {
                return e.Result;
            }

            // Mocks Simulator portion of UIElement.TransformToVisual
            if (Regex.IsMatch(param, @".+?\.getBoundingClientRect\(\)\.left - .+?\.getBoundingClientRect\(\)\.left") ||
                Regex.IsMatch(param, @".+?\.getBoundingClientRect\(\)\.top - .+?\.getBoundingClientRect\(\)\.top"))
            {
                return 0;
            }

            if (Regex.IsMatch(param, @"document\.inputManager\.focus\(document\.getElementByIdSafe\(""([^""]*)""\)\)"))
            {
                return true;
            }

            if (Regex.IsMatch(param, @"document\.getElementByIdSafe\(""([^""]*)""\)\.offsetWidth"))
            {
                return 0;
            }

            if (Regex.IsMatch(param, @"document\.getElementByIdSafe\(""([^""]*)""\)\.offsetHeight"))
            {
                return 0;
            }

            if (Regex.IsMatch(param, @"document\.getBBox\(document\.getElementByIdSafe\(""([^""]*)""\)\)"))
            {
                return JsonDocument.Parse("{\"x\":0,\"y\":0,\"width\":0,\"height\":0}").RootElement;
            }

            return new JsonElement();
        }

        /// <summary>
        /// This method will be executed whenever the assembly is loaded,
        /// so before any number of tests being run.
        /// </summary>
        /// <param name="testContext"></param>
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            Features.Interop.UseNewLineSeparator = true;

            var javaScriptExecutionHandlerMock = new Mock<INativeMethods>();
            javaScriptExecutionHandlerMock
                .Setup(x => x.ExecuteJavaScriptWithResult(It.IsAny<string>()))
                .Returns<string>(ExecuteJsMock);
            javaScriptExecutionHandlerMock
                .Setup(x => x.InvokeJS(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns<string, int, bool>((js, refId, wantsResult) => ExecuteJsMock(js));

            var javaScriptExecutionHandler2 = javaScriptExecutionHandlerMock.Object;
            INTERNAL_Simulator.JavaScriptExecutionHandler = javaScriptExecutionHandler2;

            // Instantiating Application because it sets itself as Application.Current
            _ = new App
            {
                RootVisual = new Grid(),
            };
        }

        public static void AttachVisualChild(UIElement element)
        {
            INTERNAL_VisualTreeManager.AttachVisualChildIfNotAlreadyAttached(element,
                Window.Current);
        }

        public static void SleepWhile(Func<bool> condition, string description = null, int timeoutInMs = 2000)
        {
            const int interval = 100;
            int total = 0;

            while (condition())
            {
                Thread.Sleep(interval);
                total += interval;
                if (total >= timeoutInMs)
                {
                    throw new Exception($"Timed out on waiting while {$"'{description}'" ?? "condition"}." +
                        " Consider increasing timeout or evaluating other conditions" +
                        " that do not take this long to flip.");
                }
            }
        }

        private sealed class App : Application
        {
        }
    }
}
