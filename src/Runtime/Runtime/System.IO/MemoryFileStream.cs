using OpenSilver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CSHTML5.INTERNAL_InteropImplementation;

namespace System.IO
{
    internal sealed class MemoryFileStream : Stream
    {
        private long _position;
        private readonly object _inputFileElement;
        private readonly MemoryFileInfo _file;

        private bool _isDisposed;

        public MemoryFileStream(object inputFileElement, MemoryFileInfo file)
        {
            _inputFileElement = inputFileElement;
            _file = file;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _file.Length;

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
            => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("Synchronous reads are not supported.");

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();


        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytesAvailableToRead = Length - Position;
            var maxBytesToRead = (int)Math.Min(bytesAvailableToRead, buffer.Length);
            if (maxBytesToRead <= 0)
            {
                return 0;
            }

            var bytesRead = await CopyFileDataIntoBuffer(buffer, offset, maxBytesToRead);

            _position += bytesRead;

            return bytesRead;
        }

        int counter = 0;
        private async Task<int> CopyFileDataIntoBuffer(byte[] buffer, int offset, int count)
        {
            var dataThreshold = 512000;
            var totalBytesCopied = 0;
            while (count > 0)
            {
                counter++;
                if (counter == 3)
                {
                    counter = 0;
                    await Task.Delay(500);
                }
                var numBytesToTransfer = Math.Min(count, dataThreshold);
                if (numBytesToTransfer == 0)
                {
                    break;
                }
                var result = await ReadBlock(buffer, offset, numBytesToTransfer);
                Array.Copy(Convert.FromBase64String(result), 0, buffer, offset, numBytesToTransfer);
                count = count - numBytesToTransfer;
                offset = offset + numBytesToTransfer;
                totalBytesCopied = totalBytesCopied + numBytesToTransfer;
            }
            return totalBytesCopied;
        }

        private Task<string> ReadBlock(byte[] buffer, int offset, int count)
        {
            TaskCompletionSource<String> readBlockTaskCompletionSource = new TaskCompletionSource<String>();
            Action<string> ReadBlockCallback = (content) =>
            {
                try
                {
                    readBlockTaskCompletionSource.SetResult(content);
                }
                catch (Exception ex)
                {
                    readBlockTaskCompletionSource.SetException(ex);
                }
            };

            // Add the callback to the document:
            int callbackId = CSHTML5.INTERNAL_InteropImplementation.RegisterCallback(ReadBlockCallback);            
            Interop.ExecuteJavaScript("window.readFileData($0,$1,$2,$3,$4)", _inputFileElement, _file.Id, offset, count, callbackId);
            return readBlockTaskCompletionSource.Task;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            // If the browser connection is still live, notify the JS side that it's free to release the Blob
            // and reclaim the memory. If the browser connection is already gone, there's no way for the
            // notification to get through, but we don't want to fail the .NET-side disposal process for this.
            try
            {
                //(_jsStreamReference as IDisposable)?.Dispose();
            }
            catch
            {
            }

            _isDisposed = true;

            base.Dispose(disposing);
        }
    }
}
