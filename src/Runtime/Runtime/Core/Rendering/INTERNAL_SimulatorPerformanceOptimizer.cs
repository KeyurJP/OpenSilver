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


//#define DISABLE_SIMULATOR_PERFORMANCE_OPTIMIZATION


#if OLD_CODE_TO_OPTIMIZE_SIMULATOR_PERFORMANCE // Obsolete since Beta 13.4 on 2018.01.31 because we now use the Dispatcher instead (cf. the class "INTERNAL_SimulatorExecuteJavaScript")

#if BRIDGE
using Bridge;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSHTML5.Internal
{
#if !BRIDGE
    [JSIgnore]
#else
    [External]
#endif
    internal static class INTERNAL_SimulatorPerformanceOptimizer
    {
        //--------------
        // The goal of this class is to increase the performance of
        // the simulator by introducing the concept of "transaction",
        // which allow to execute all the JavaScript code of a transaction
        // at once at the end of the transaction instead of progressively.
        // This increases performance because it reduces the number of
        // "interops" between C# and the WebBrowser control.
        //--------------

        static int _nestedTransactionsCounter = 0;
        static List<string> _pendingJavaScript = new List<string>();

        public static void StartTransaction()
        {
            // Check if a transaction is already running, in which case we nest a new transaction by increasing the counter:
            if (IsRunningInsideATransaction)
            {
                _nestedTransactionsCounter++;
            }
            else
            {
                _nestedTransactionsCounter++;

                _pendingJavaScript.Add(@"//------ Transaction Start ------");

                // As a precautionary measure, we also add an additional "Flush" to the Dispacther queue so that if the app code crashes or the "EndTransaction()" for some reason is not called, we still flush the transaction.
                INTERNAL_Simulator.WebControl.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Flush();
                }));
            }
        }

        public static void EndTransaction()
        {
            _nestedTransactionsCounter--;

            // If there are no more nested transactions, flush:
            if (_nestedTransactionsCounter <= 0)
            {
                _pendingJavaScript.Add(@"//------ Transaction End ------");
                Flush();
            }
        }

        public static void QueueJavaScriptCode(string javaScriptCode)
        {
#if !DISABLE_SIMULATOR_PERFORMANCE_OPTIMIZATION
            // If we are in a transaction, we queue the code, otherwise we execute it straight away.
            if (IsRunningInsideATransaction)
            {
                _pendingJavaScript.Add(javaScriptCode);
            }
            else
            {
                INTERNAL_HtmlDomManager.ExecuteJavaScript(javaScriptCode);
            }
#else
            INTERNAL_HtmlDomManager.ExecuteJavaScript(javaScriptCode);
#endif
        }

        // This will execute all the pending JavaScript, and reset all nested transactions:
        public static void Flush()
        {
            // Reset the counter of nested transactions:
            _nestedTransactionsCounter = 0;

            // Execute all pending JavaScript:
            if (_pendingJavaScript.Count > 0)
            {
                _pendingJavaScript.Add(@"//------ Transaction Flush ------");
                string javaScriptCodeToExecute = string.Join("\r\n", _pendingJavaScript);
                INTERNAL_HtmlDomManager.ExecuteJavaScript(javaScriptCodeToExecute);
            }

            // Clear the list of pending JavaScript:
            _pendingJavaScript.Clear();
        }

        static bool IsRunningInsideATransaction
        {
            get
            {
                return _nestedTransactionsCounter > 0;
            }
        }
    }
}
#endif