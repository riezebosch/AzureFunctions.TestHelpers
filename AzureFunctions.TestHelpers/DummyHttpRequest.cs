using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace AzureFunctions.TestHelpers
{
    public class DummyHttpRequest : HttpRequest
    {
        public DummyHttpRequest()
        {
            HttpContext = new DummyHttpContext(this);
        }
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken()) => 
            Task.FromResult((IFormCollection)new FormCollection(new Dictionary<string, StringValues>()));

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; } = "Get";
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; } = new QueryCollection();
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override IRequestCookieCollection Cookies { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override Stream Body { get; set; } = new MemoryStream();
        public override bool HasFormContentType { get; } = false;
        public override IFormCollection Form { get; set; }
    }
}