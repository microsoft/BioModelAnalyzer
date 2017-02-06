// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi
{
    public class HelloController : ApiController
    {
        public string Post()
        {
            return "Hello, World!";
        }
    }
}
