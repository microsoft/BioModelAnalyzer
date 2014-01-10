using System;
using System.Web;

namespace BioCheck.Web
{
    public partial class tool : System.Web.UI.Page
    {
        public string IPAddress, Model;

        protected void Page_Load(object sender, EventArgs e)
        {
            var req = HttpContext.Current.Request;
            IPAddress = req.ServerVariables["REMOTE_ADDR"];
            Model = req.QueryString["Model"];
        }
    }
}