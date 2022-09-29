using System.Windows.Media;
using System.Windows;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSHTML5.Internal;
using System.Windows.Media.Imaging;

#if MIGRATION
using System.Windows.Controls;
namespace System.Windows.Media.Imaging
#else
using Windows.UI.Xaml.Controls;
namespace Windows.UI.Xaml.Media.Imaging
#endif
{
    public sealed class WriteableBitmap : BitmapSource
    {
        private int[] _pixels;
        private int _pixelWidth;
        private int _pixelHeight;
        private int _pixelArrayLength;

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> class using the provided dimensions.</summary>
        /// <param name="pixelWidth">The width of the bitmap.</param>
        /// <param name="pixelHeight">The height of the bitmap.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="pixelWidth" /> or <paramref name="pixelHeight" /> is zero or less.</exception>
        public WriteableBitmap(int pixelWidth, int pixelHeight)
        {
            if (pixelWidth < 0)
                throw new ArgumentOutOfRangeException(nameof(pixelWidth));

            if (pixelHeight < 0)
                throw new ArgumentOutOfRangeException(nameof(pixelHeight));

            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;
            _pixelArrayLength = _pixelWidth * _pixelHeight;
            _pixels = new int[_pixelArrayLength];
            _tcsInit.SetResult(true);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> class using the provided <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
        /// <param name="source">The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> to use for initialization. </param>
        public WriteableBitmap(BitmapSource source)
        {
            if (source == null)
                throw new ArgumentOutOfRangeException(nameof(source));

            var imageSrc = Image.GetImageTagSrc(source);
            var javascript = @"
                    var imageView = new Image();
                    imageView.src = $0;
                    imageView.onload = function() {
                        let canvas = document.createElement('canvas'); 
                        canvas.height = imageView.height;
                        canvas.width = imageView.width;
                        let ctx = canvas.getContext('2d');
                        ctx.drawImage(imageView, 0, 0);
                        let dataUrl = canvas.toDataURL();
                        let imgData = ctx.getImageData(0, 0, ctx.canvas.width, ctx.canvas.height);
                        var bytes = '';
                        for (var i = 0; i < imgData.data.length;i++) {
                            bytes += ','; bytes += imgData.data[i].toString();
                        }    
                        canvas = null; imageView = null;
                        $1(imgData.width, imgData.height, dataUrl, bytes.substring(1));
                    }";
            Action<int, int, string, string> callback = OnImageDataLoadedCallback;
            OpenSilver.Interop.ExecuteJavaScript(javascript, imageSrc, callback);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> class using the provided element and transform.</summary>
        /// <param name="element">The desired element to be rendered within the bitmap. </param>
        /// <param name="transform">The transform the user wants to apply to the element as the last step before drawing into the bitmap. This is particularly interesting for you if you want the bitmap to respect its transform. This value can be null.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The element size is invalid. This happens when the pixel width or pixel height is not greater than zero.</exception>
        public WriteableBitmap(UIElement element, Transform transform)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            Action<int, int, string, string> callback = OnImageDataLoadedCallback;

            if (element.INTENRAL_CanvasDom != null)
            {
                FetchImageData(element.INTENRAL_CanvasDom, callback);
            }
            else
            {
                var outerDiv = (INTERNAL_HtmlDomElementReference)OpenSilver.Interop.GetDiv(element);
                var javascript = @"
                domtoimage.toCanvas(document.querySelector('#' + $0)).then(function (canvas) {
                    try {
                        let dataUrl = canvas.toDataURL();
                        let ctx = canvas.getContext('2d');
                        let imgData = ctx.getImageData(0, 0, ctx.canvas.width, ctx.canvas.height);
                        var bytes = '';
                        for (var i = 0; i < imgData.data.length;i++) {
                            bytes += ','; bytes += imgData.data[i].toString();
                        }                        
                        $1(imgData.width, imgData.height, dataUrl, bytes.substring(1));
                    }
                    catch (err) {
                        console.error(err);
                    }
                });";
                OpenSilver.Interop.ExecuteJavaScript(javascript, outerDiv.UniqueIdentifier, callback);
            }
        }

        TaskCompletionSource<bool> _tcsInit = new TaskCompletionSource<bool>();

        /// <summary>
        /// User must call Init after instantiation in order to load the buffer
        /// </summary>
        /// <returns></returns>
        public Task<bool> WaitToInitialize()
        {
            return _tcsInit.Task;
        }

        private void OnImageDataLoadedCallback(int width, int height, string dataUrl, string bytes)
        {
            int startIdx = dataUrl.IndexOf(',');
            if (startIdx >= 0)
            {
                //dataUrl = dataUrl.Remove(0, startIdx + 1);
                SetSource(dataUrl);
            }

            var parts = bytes.Split(new char[] { ',' });

            _pixelWidth = width;
            _pixelHeight = height;
            _pixels = new int[parts.Length / 4];
            _pixelArrayLength = _pixels.Length;

            byte[] rgba = { 0, 0, 0, 0 };
            int count = 0;
            for (int i = 0; i < parts.Length; i += 4)
            {
                for (int j = 0; j < rgba.Length; j++)
                {
                    rgba[j] = Convert.ToByte(parts[i + j]);
                }

                _pixels[count] = BitConverter.ToInt32(rgba, 0);
                count++;
            }

            _tcsInit.SetResult(true);
        }

        private void FetchImageData(object canvasDom, Action<int, int, string, string> callback)
        {
            if (canvasDom == null)
            {
                throw new InvalidOperationException("Canvas is not set");
            }

            var javascript = @"
                        let canvas = $0;
                        let dataUrl = canvas.toDataURL();
                        let ctx = canvas.getContext('2d');
                        let imgData = ctx.getImageData(0, 0, ctx.canvas.width, ctx.canvas.height);
                        var bytes = '';
                        for (var i = 0; i < imgData.data.length;i++) {
                            bytes += ','; bytes += imgData.data[i].toString();
                        }
                        $1(imgData.width, imgData.height, dataUrl, bytes.substring(1));";

            OpenSilver.Interop.ExecuteJavaScript(javascript, canvasDom, callback);
        }
        /// <summary>Gets an array representing the 2-D texture of the bitmap.</summary>
        /// <returns>An array of integers representing the 2-D texture of the bitmap.</returns>
        /// <exception cref="T:System.Security.SecurityException">The <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> is created from protected content. The <see cref="P:System.Windows.Media.Imaging.WriteableBitmap.Pixels" /> array is inaccessible in this case.</exception>
        public int[] Pixels
        {
            get
            {
                return _pixels;
            }
        }

        internal override int PixelHeightInternal
        {
            get
            {
                return _pixelHeight;
            }
        }

        internal override int PixelWidthInternal
        {
            get
            {
                return _pixelWidth;
            }
        }

        private TaskCompletionSource<bool> _tcsRender;
        public Task<bool> WaitTobeRendered()
        {
            if (_tcsRender == null) return Task.FromResult(true);
            else return _tcsRender.Task;
        }

        private void OnRenderDataLoadedCallback(int width, int height, string dataUrl, string bytes)
        {
            int startIdx = dataUrl.IndexOf(',');
            if (startIdx >= 0)
            {
                //dataUrl = dataUrl.Remove(0, startIdx + 1);
                SetSource(dataUrl);
            }
            var parts = bytes.Split(new char[] { ',' });
            var pixels = new int[parts.Length / 4];
            byte[] rgba = { 0, 0, 0, 0 };

            int count = 0;
            for (int i = 0; i < parts.Length; i += 4)
            {
                for (int j = 0; j < rgba.Length; j++)
                {
                    rgba[j] = Convert.ToByte(parts[i + j]);
                }

                pixels[count] = BitConverter.ToInt32(rgba, 0);
                count++;
            }


            int rowLenth = width * 4 + 1;
            // assing the element image into destination pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIdx = width * y + x;
                    int destIdx = PixelWidth * y + x;
                    if (destIdx < Pixels.Length)
                    {
                        Pixels[destIdx] = pixels[srcIdx];
                    }
                }
            }

            _tcsRender.SetResult(true);
        }

        /// <summary>Renders an element within the bitmap.</summary>
        /// <param name="element">The element to be rendered within the bitmap.</param>
        /// <param name="transform">The transform to apply to the element before drawing into the bitmap. If an empty transform is supplied, the bits representing the element show up at the same offset as if they were placed within their parent.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element" /> is null.</exception>
        public void Render(UIElement element, Transform transform)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _tcsRender = new TaskCompletionSource<bool>();
            Action<int, int, string, string> callback = OnRenderDataLoadedCallback;

            if (element.INTENRAL_CanvasDom != null)
            {
                FetchImageData(element.INTENRAL_CanvasDom, callback);
            }
            else
            {
                var outerDiv = (INTERNAL_HtmlDomElementReference)OpenSilver.Interop.GetDiv(element);
                var javascript = @"
                domtoimage.toCanvas(document.querySelector('#' + $0)).then(function (canvas) {
                    try {
                        let dataUrl = canvas.toDataURL();
                        let ctx = canvas.getContext('2d');
                        let imgData = ctx.getImageData(0, 0, ctx.canvas.width, ctx.canvas.height);
                        var bytes = '';
                        for (var i = 0; i < imgData.data.length;i++) {
                            bytes += ','; bytes += imgData.data[i].toString();
                        }                        
                        $1(imgData.width, imgData.height, dataUrl, bytes.substring(1));
                    }
                    catch (err) {
                        console.error(err);
                    }
                });";
                OpenSilver.Interop.ExecuteJavaScript(javascript, outerDiv.UniqueIdentifier, callback);
            }
        }

        /// <summary>Requests a draw or redraw of the entire bitmap.</summary>
        public void Invalidate()
        {
            if (_pixels != null)
            {
                int rowLenth = _pixelWidth * 4 + 1;

                var bytes = new byte[rowLenth * _pixelHeight];

                for (int y = 0; y < PixelHeight; y++)
                {
                    for (int x = 0; x < PixelWidth; x++)
                    {
                        var rgba = BitConverter.GetBytes(Pixels[PixelWidth * y + x]);
                        int startIdx = rowLenth * y + x * 4 + 1;
                        for (int j = 0; j < rgba.Length; j++)
                        {
                            bytes[startIdx + j] = rgba[j];
                        }
                    }
                }

                SetSource(PngEncoder.Encode(bytes, PixelWidth, PixelHeight));

                //PngEncoder.Encode()
                //StringBuilder builder = new StringBuilder();

                //foreach (var pixel in _pixels)
                //{
                //    var rgba = BitConverter.GetBytes(pixel);
                //    for (int j = 0; j < rgba.Length; j++)
                //    {
                //        builder.Append(',');
                //        builder.Append(rgba[j].ToString());
                //    }
                //}

                //builder.Remove(0, 1);

                //var javascript = @"(async() => {
                //        let parts = $0.split(',');
                //        var  data = new Uint8Array($1 * $2 * 4);  
                //        for(let i = 0; i < parts.length; i++) {
                //            data[i] = Number(parts[i]);
                //        }
                //        let png = UPNG.encode([data.buffer], $1, $2, 0);
                //        let array = new Uint8Array(png);
                //        console.log(array);
                //        var base64 = await OSBase64.encode(array);
                //        console.log('Base64', base64);
                //        return base64;})();";

                //string csv = builder.ToString();
                //var base64 = Convert.ToString(OpenSilver.Interop.ExecuteJavaScript(javascript, csv, PixelWidth, PixelHeight));
                //Console.WriteLine("Base64: " + base64);
                //SetSource(base64);  
            }
        }
    }
}
