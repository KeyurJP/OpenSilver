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


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

#if MIGRATION
using System.Windows;
using System.Windows.Controls;
#else
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    public sealed partial class OpenFileDialog
    {
        private readonly object _inputElement;

        public OpenFileDialog()
        {
            // Creates <input> element but does not add to DOM
            _inputElement = OpenSilver.Interop.ExecuteJavaScript(@"
                (function() {
                    var inputElement = document.createElement(""input"");
                    inputElement.type = ""file"";
                    inputElement.style.display ='none';
                    return inputElement;
                })()");
        }

        private void AddFileChangeCallback(TaskCompletionSource<bool> showDialogTaskCompletionSource)
        {
            // For each file selected in the dialog, this will be called back
            Action<string,string> onFileOpened = (filename, content) =>
            {
                OpenSilver.Interop.ExecuteJavaScript("console.log('OpenFileContent :" + content.ToString() + "')");
                //Dictionary<string, byte> indexedBytes = JsonSerializer.Deserialize<Dictionary<string, byte>>(content.ToString());
                byte[] values = Convert.FromBase64String(content);
                Files.Add(new MemoryFileInfo(filename, values));
            };

            Action onFileOpenFinished = () =>
            {
                System.Diagnostics.Debug.WriteLine("OpenFileDialog->AddFileChangeCallback->onFileOpenFinished.");
                showDialogTaskCompletionSource.SetResult(true);
            };

            Action onFileOpenCanceled = () =>
            {
                System.Diagnostics.Debug.WriteLine("OpenFileDialog->AddFileChangeCallback->onFileOpenCanceled.");
                showDialogTaskCompletionSource.SetResult(false);
            };

            // Listen to the "change" property of the "input" element, and call the callback:
            OpenSilver.Interop.ExecuteJavaScript(@"
                    $0.addEventListener(""click"", function(e) {
                        document.body.onfocus = function() {
                        document.body.onfocus = null;
                        setTimeout(() => { 
                            if ($0.value.length) {
                            }
                            else
                            {
                                var cancelCallback = $3;
                                cancelCallback();
                            }
                            $0.remove();
                        }, 1000);
                    }
                });
                $0.addEventListener(""change"", function(e) {                    
                    if(!e) {
                      e = window.event;
                    }
                    var input = e.target;
                    var callback = $1;
                    var reader = new FileReader();

                    // Reading each file sequentially, some results were null when running concurrently
                    function readNext(i) {
                        var file = input.files[i];
                        reader.onload = function(e) {    
                            var result = new Uint8Array(e.target.result);                                
                            var binaryString = btoa(String.fromCharCode.apply(null, result));
                            callback(file.name, binaryString);

                            if (input.files.length > i + 1) {
                                readNext(i + 1);
                            } else {
                                // Triggers finished callback
                                $2();
                            }
                        };

                        reader.readAsArrayBuffer(file);
                        var isRunningInTheSimulator = $4;
                        if (isRunningInTheSimulator) {
                          alert(""The file open dialog is not supported in the Simulator. Please test in the browser instead."");
                        }
                    }
                    readNext(0);
                });", _inputElement, onFileOpened, onFileOpenFinished, onFileOpenCanceled, OpenSilver.Interop.IsRunningInTheSimulator);
        }

        private void SetFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return;
            }

            // Process the filter list to convert the syntax from XAML to HTML5:
            // Example of syntax in Silverlight: Image Files (*.bmp, *.jpg)|*.bmp;*.jpg|All Files (*.*)|*.*
            // Example of syntax in HTML5: .gif, .jpg, .png, .doc
            string[] splitted = filter.Split('|');
            List<string> itemsKept = new List<string>();
            if (splitted.Length == 1)
            {
                itemsKept.Add(splitted[0]);
            }
            else
            {
                for (int i = 1; i < splitted.Length; i += 2)
                {
                    itemsKept.Add(splitted[i]);
                }
            }
            string filtersInHtml5 = string.Join(",", itemsKept).Replace("*", "").Replace(";", ",");

            // Apply the filter:
            if (!string.IsNullOrWhiteSpace(filtersInHtml5))
            {
                OpenSilver.Interop.ExecuteJavaScript(@"$0.accept = $1", _inputElement, filtersInHtml5);
            }
            else
            {
                OpenSilver.Interop.ExecuteJavaScript(@"$0.accept = """"", _inputElement);
            }
        }

        private bool _multiselect = false;
        public bool Multiselect
        {
            get { return _multiselect; }
            set
            {
                _multiselect = value;

                if (_multiselect)
                {
                    OpenSilver.Interop.ExecuteJavaScript(@"$0.multiple = 'multiple';", _inputElement);
                }
            }
        }

        private string _filter;
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                SetFilter(_filter);
            }
        }

        [OpenSilver.NotImplemented]
        // No option to separate html5 'accept' file types into groups, they all appear together as 'Custom',
        // with an additional option for 'All Files' (Chrome).
        // In Firefox they appear all together, and then one option for each file type.
        public int FilterIndex
        {
            get;
            set;
        }

        public MemoryFileInfo File
        {
            get
            {
                return Files.FirstOrDefault();
            }
        }

        public IEnumerable<MemoryFileInfo> Files
        {
            get;
            private set;
        } = new List<MemoryFileInfo>();

        /// <summary>
        /// Opens the default browser file dialog. This returns a Task, differently from Silverlight.
        /// This is because it is not possible to wait for the dialog to conclude, since the process is single-threaded.
        /// </summary>
        /// <returns>A Task that will have the result of the file dialog. True for files selected, false for cancel/exit.</returns>
        /// 
        public Task<bool> ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Opens the default browser file dialog. This returns a Task, differently from Silverlight.
        /// This is because it is not possible to wait for the dialog to conclude, since the process is single-threaded.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns>A Task that will have the result of the file dialog. True for files selected, false for cancel/exit.</returns>
        internal Task<bool> ShowDialog(Window owner)
        {
            ClearFiles();
            // This wraps a task to return to the caller and have a result asynchronously
            TaskCompletionSource<bool> showDialogTaskCompletionSource = new TaskCompletionSource<bool>();
            AddFileChangeCallback(showDialogTaskCompletionSource);
            // Triggers 'click' on <input>, even though it is not on the DOM
            OpenSilver.Interop.ExecuteJavaScript(@"$0.click();", _inputElement);

            return showDialogTaskCompletionSource.Task;
        }

        private void ClearFiles()
        {
            ((IList<MemoryFileInfo>)Files).Clear();
        }
    }
}