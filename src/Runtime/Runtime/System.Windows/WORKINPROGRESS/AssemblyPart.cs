

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


using System.IO;
using System.Reflection;
using System.Security;

#if MIGRATION
namespace System.Windows
#else
namespace Windows.UI.Xaml
#endif
{
    public sealed partial class AssemblyPart : DependencyObject
    {
        internal Assembly Assembly { get; set; } = null;
        //
        // Summary:
        //     Identifies the System.Windows.AssemblyPart.Source dependency property.
        //
        // Returns:
        //     The identifier for the System.Windows.AssemblyPart.Source dependency property.
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(AssemblyPart), new PropertyMetadata());

        //
        // Summary:
        //     Initializes a new instance of the System.Windows.AssemblyPart class.
        public AssemblyPart()
        {

        }

        //
        // Summary:
        //     Gets the System.Uri that identifies an assembly as an assembly part.
        //
        // Returns:
        //     A System.String that is the assembly, which is identified as an assembly part.
        public string Source
        {
            get { return (string)this.GetValue(SourceProperty); }
            set { this.SetValue(SourceProperty, value); }
        }

        //
        // Summary:
        //     Converts a System.IO.Stream to an System.Reflection.Assembly that is subsequently
        //     loaded into the current application domain.
        //
        // Parameters:
        //   assemblyStream:
        //     The System.IO.Stream to load into the current application domain.
        //
        // Returns:
        //     The System.Reflection.Assembly that is subsequently loaded into the current application
        //     domain.
        [SecuritySafeCritical]
        public Assembly Load(Stream assemblyStream)
        {
            this.Assembly = Assembly.Load(StreamToBuffer(assemblyStream));
            return this.Assembly;
        }

        internal static byte[] StreamToBuffer(Stream assemblyStream)
        {
            byte[] buffer;

            using (assemblyStream)
            {
                // avoid extra step for MemoryStream (but not any stream that inherits from it)
                if (assemblyStream.GetType() == typeof(MemoryStream))
                    return (assemblyStream as MemoryStream).ToArray();

                // it's normally bad to depend on Stream.Length since some stream (e.g. NetworkStream)
                // don't implement them. However it is safe in this case (i.e. SL2 depends on Length too)
                buffer = new byte[assemblyStream.Length];

                int length = buffer.Length;
                int offset = 0;
                while (length > 0)
                {
                    int read = assemblyStream.Read(buffer, offset, length);
                    if (read == 0)
                        break;

                    length -= read;
                    offset += read;
                }
            }

            return buffer;
        }
    }
}
