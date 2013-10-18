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
            var modelUri = req.QueryString["Model"];
            if (!string.IsNullOrWhiteSpace(modelUri))
            {
                var uri = new Uri(req.Url, modelUri);
                Model = uri.AbsoluteUri;
                // HACK for local emulator - something to do with port mapping,
                // but don't understand.
                //Model = Model.Replace(":82/", ":81/");
            }
        }
    }
}