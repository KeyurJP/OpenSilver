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


using System.Threading;
#if MIGRATION
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace System.IO
{
    public class MemoryFileInfo
    {
        public string Name { get; set; } = string.Empty;

        string _extension = null;
        public string Extension 
        {
            get 
            { 
                if (_extension == null) 
                    _extension = Path.GetExtension(Name);                
                return _extension;
            } 
        }

        private long _length;

        public long Length
        {
            get => _length;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(Length), $"Size must be a non-negative value. Value provided: {value}.");
                }

                _length = value;
            }
        }

        #region Internal methods
        internal OpenFileDialog Owner { get; set; }

        public int Id { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public string ContentType { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        #endregion

        public Stream OpenReadStream()
        {            
            return Owner.OpenReadStream(this);
        }
    }
}