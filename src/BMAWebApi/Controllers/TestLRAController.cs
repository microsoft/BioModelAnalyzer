using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace BMAWebApi.Controllers
{
    public class TestLRAController : ApiController
    {
        // GET api/<controller>
        public string Post(object data)
        {
            return Guid.NewGuid().ToString();
        }

        [HttpGet]
        [ActionName("status")]
        public string GetStatus(string id)
        {
            Random r = new Random();
            if (r.Next(0, 10) > 2)
            {
                return "Completed";
            }
            else
            {
                return "Incompleted";
            }
        }

        [HttpGet]
        [ActionName("result")]
        public object GetData(string id)
        {
            return new int[] { 1, 2, 3 };
        }

    }
}
