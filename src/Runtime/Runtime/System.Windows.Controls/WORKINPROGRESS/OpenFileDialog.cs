

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
        private object _windowFocusCallback;
        private readonly object _instance;

        public OpenFileDialog()
        {
            // Creates <input> element but does not add to DOM
            _instance = OpenSilver.Interop.ExecuteJavaScript(@"
                (function() {
                    var inputElement = document.createElement(""input"");
                    inputElement.type = ""file"";
                    return inputElement;
                })()");
        }

        private bool _multiselect = false;
        public bool Multiselect
        {
            get { return _multiselect; }
            set {
                _multiselect = value;
                if (_multiselect)
                {
                    OpenSilver.Interop.ExecuteJavaScript(@"$0.multiple = 'multiple';", _instance);
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
                OpenSilver.Interop.ExecuteJavaScript(@"$0.accept = $1", _instance, filtersInHtml5);
            }
            else
            {
                OpenSilver.Interop.ExecuteJavaScript(@"$0.accept = """"", _instance);
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
        public Task<bool?> ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Opens the default browser file dialog. This returns a Task, differently from Silverlight.
        /// This is because it is not possible to wait for the dialog to conclude, since the process is single-threaded.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns>A Task that will have the result of the file dialog. True for files selected, false for cancel/exit.</returns>
        public Task<bool?> ShowDialog(Window owner)
        {
            ClearFiles();

            // This wraps a task to return to the caller and have a result asynchronously
            TaskCompletionSource<bool?> showDialogTaskCompletionSource = new TaskCompletionSource<bool?>();

            AddCancelCallback(showDialogTaskCompletionSource);

            AddFileChangeCallback(showDialogTaskCompletionSource);

            // Triggers 'click' on <input>, even though it is not on the DOM
            OpenSilver.Interop.ExecuteJavaScript(@"
                $0.isOpenFileDialogOpen = true;
                $0.dispatchEvent(new MouseEvent(""click""));", _instance);
            return showDialogTaskCompletionSource.Task;
        }

        private void ClearFiles()
        {
            ((IList<MemoryFileInfo>)Files).Clear();
        }

        private void AddCancelCallback(TaskCompletionSource<bool?> showDialogTaskCompletionSource)
        {
            Action<object> onDialogCancel = (result) =>
            {
                try
                {
                    // Setting result of Task returned by ShowDialog()
                    showDialogTaskCompletionSource.SetResult(false);
                }
                catch (Exception ex)
                {
                    showDialogTaskCompletionSource.SetException(ex);
                }
            };

            _windowFocusCallback = OpenSilver.Interop.ExecuteJavaScript(@"
                (function() {
                    var windowFocusCallbackForFileDialogCancel = function(e) {
                        var isChrome = !!window.chrome;
                        if (isChrome) {
                            // If on Chrome, verifies flag after timeout because the window 'focus' is called before
                            // the 'change' event, timeout should be enough to make sure 'change' hasn't been triggered
                            setTimeout(function() {
                                if ($0.isOpenFileDialogOpen) {
                                    $0.isOpenFileDialogOpen = false;

                                    // Removing window focus handler to detect cancels, otherwise we could have multiple handlers
                                    // after calling multiple ShowDialog()
                                    window.removeEventListener('focus', windowFocusCallbackForFileDialogCancel);

                                    // Calls cancel callback
                                    $1();
                                }
                            }, 1000);
                        } else {
                            if ($0.isOpenFileDialogOpen) {
                                $0.isOpenFileDialogOpen = false;

                                // Removing window focus handler to detect cancels, otherwise we could have multiple handlers
                                // after calling multiple ShowDialog()
                                window.removeEventListener('focus', windowFocusCallbackForFileDialogCancel);

                                // Calls cancel callback
                                $1();
                            }
                        }
                    }

                    window.addEventListener('focus', windowFocusCallbackForFileDialogCancel);

                    return windowFocusCallbackForFileDialogCancel;
                })()
            ", _instance, onDialogCancel);
        }

        private void AddFileChangeCallback(TaskCompletionSource<bool?> showDialogTaskCompletionSource)
        {
            Action<object> FileChangeCallback = (object file) =>
            {
                try
                {
                    var jsonData = Convert.ToString(OpenSilver.Interop.ExecuteJavaScript("JSON.stringify($0)", file));                    
                    MemoryFileInfo info = JsonSerializer.Deserialize<MemoryFileInfo>(jsonData, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    info.Owner = this;
                    this.Files.Add(info);
                }
                catch (Exception ex)
                {
                    showDialogTaskCompletionSource.SetException(ex);
                }
            };

            Action onFinishedReading = () =>
            {
                try
                {
                    // Setting result of Task returned by ShowDialog()
                    showDialogTaskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    showDialogTaskCompletionSource.SetException(ex);
                }
            };

            // Listen to the "change" property of the "input" element, and call the callback:
            OpenSilver.Interop.ExecuteJavaScript(@"
                $0._FileNextFileId = 0;
                $0._FilesById = {};
                $0.addEventListener(""change"", function(e) {
                    var isRunningInTheSimulator = $2;
                    if (isRunningInTheSimulator) {
                        alert(""The file open dialog is not supported in the Simulator. Please test in the browser instead."");
                    }

                    $0.isOpenFileDialogOpen = false;

                    // Removing window focus handler to detect cancels, otherwise we could have multiple handlers
                    // after calling multiple ShowDialog()
                    window.removeEventListener('focus', $3);

                    if(!e) {
                      e = window.event;
                    }
                    var input = e.target;
                    var callback = $1;                   
                    var fileList = Array.prototype.map.call($0.files, function (file) {
				        var result = {
				        	id: ++$0._FileNextFileId,
				        	lastModified: new Date(file.lastModified).toISOString(),
				        	name: file.name,
				        	length: file.size,
				        	type: file.type,
				        	relativePath: file.webkitRelativePath
				        };
				        $0._FilesById[result.id] = result;

				        // Attach the blob data itself as a non-enumerable property so it doesn't appear in the JSON
				        Object.defineProperty(result, 'blob', { value: file });
                         $1(result);
				        return result;
			        });
                    $4();
                    $0.value = '';                    
                });", _instance, FileChangeCallback, OpenSilver.Interop.IsRunningInTheSimulator, _windowFocusCallback, onFinishedReading);
        }

        internal Stream OpenReadStream(MemoryFileInfo file) => new MemoryFileStream(_instance, file);
    }
}