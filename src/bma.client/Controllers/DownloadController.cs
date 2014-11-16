using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Http;

namespace bma.client.Controllers
{
    public class DownloadController : ApiController
    {
        public async Task<HttpResponseMessage> Get(string id, string token)
        {
            // GET https://apis.live.net/v5.0/file.a6b2a7e8f2515e5e.A6B2A7E8F2515E5E!126/content?access_token=ACCESS_TOKEN
            string url = String.Concat("https://apis.live.net/v5.0/", id, "/content?access_token=", token);
            var odRequest = WebRequest.CreateHttp(url);
            var odResponse = await odRequest.GetResponseAsync();
            var odContentType = new ContentType(odResponse.ContentType);
            var content = new StreamContent(odResponse.GetResponseStream());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(odContentType.MediaType) { CharSet = odContentType.CharSet };
            return new HttpResponseMessage() { Content = content };
        }
    }
}
