using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClientZipExperiments
{
    public class HelloController : ApiController
    {
        // POST api/<controller>
        public async Task<bma.client.Model> Post()
        {
            var zippedStream = await Request.Content.ReadAsStreamAsync();

            // Skip 2 bytes to make header compatible with .NET DeflateStream
            var dummy = new byte[2];
            zippedStream.Read(dummy,0,2);

            // Decompressed stream
            var decompressedStream = new DeflateStream(zippedStream, CompressionMode.Decompress);

            var serializer = new JsonSerializer();
            var model = serializer.Deserialize<bma.client.Model>(new JsonTextReader(new StreamReader(decompressedStream)));

            return model;
        }
    }

    public class HelloInput {
        public string name { get; set; }
    }

    public class HelloOutput {
        public string greeting { get; set; }
    }
}