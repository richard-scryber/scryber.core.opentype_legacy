using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Scryber.Core.OpenType.Tests
{
    public class FontDownload : IDisposable
    {

        public FontDownload()
        {
        }

        

        public async Task<byte[]> DownloadFrom(string path)
        {
            if (string.IsNullOrEmpty(path) && !Uri.IsWellFormedUriString(path, UriKind.Absolute))
                throw new ArgumentException("Invalid URI", nameof(path));

            return await DoDownloadAsync(path);
        }


        System.Net.Http.HttpClient http = new System.Net.Http.HttpClient();
        
        protected async Task<byte[]> DoDownloadAsync(string path)
        {
            return await http.GetByteArrayAsync(path);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != this.http)
                    this.http.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }


        ~FontDownload()
        {
            this.Dispose(false);
        }
    }
}
