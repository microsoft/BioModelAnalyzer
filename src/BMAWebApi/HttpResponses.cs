using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BMAWebApi
{
    static class HttpResponses
    {
        public static HttpResponseMessage Json(HttpRequestMessage request, string content)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage PlainText(HttpRequestMessage request, string content, HttpStatusCode status = HttpStatusCode.OK)
        {
            var response = request.CreateResponse(status);
            response.Content = new StringContent(content, System.Text.Encoding.UTF8, "text/plain");
            return response;
        }
    }
}
