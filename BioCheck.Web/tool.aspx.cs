using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BioCheck.Web
{
      public partial class tool : System.Web.UI.Page
    {
        public string InitParam;

        protected void Page_Load(object sender, EventArgs e)
        {
            InitParam = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}