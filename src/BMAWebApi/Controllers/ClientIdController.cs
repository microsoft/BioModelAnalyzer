// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace bma.client.Controllers
{
    public class ClientIdController : ApiController
    {
        // GET api/<controller>
        public string Get()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
