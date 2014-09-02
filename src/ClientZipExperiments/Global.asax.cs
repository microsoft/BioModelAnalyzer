using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using System.Web.SessionState;

namespace ClientZipExperiments
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configuration.Formatters.Add(new BinaryArrayMediaTypeFormatter());

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}");
        }
    }

    public class BinaryArrayMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly string atom = "application/binary";

        public BinaryArrayMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(atom));
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(byte[]);
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public async override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            const int chunkSize = 16384;
            byte[] chunk = new byte[chunkSize];
            List<byte> result = new List<byte>();
            while (true)
            {
                int len = await readStream.ReadAsync(chunk, 0, chunkSize);
                if (len > 0)
                    result.AddRange(chunk.Take(len));
                if (len < chunkSize)
                    break;
            }
            return result.ToArray();
        }
    }
}