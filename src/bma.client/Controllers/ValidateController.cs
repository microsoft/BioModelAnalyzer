using BioCheckAnalyzerCommon;
using bmaclient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace bma.client.Controllers
{
    public class ValidateController : ApiController
    {
        // POST api/Validate
        public ValidationOutput Post([FromBody]ValidationInput input)
        {
            var output = new ValidationOutput();
            var result = UIExpr.check_syntax(input.Formula);
            if (result.IsParseOK)
            {
                output.IsValid = true;
            }
            else if (result.IsParseErr)
            {
                UIExpr.perr perr = (result as UIExpr.parse_result.ParseErr).Item;

                output.IsValid = false;
                output.Line = perr.line;
                output.Column = perr.col;
                output.Details = perr.msg;

                // TODO - Samin: sometimes the lines break mid-way through a line, e.g. the var is put on another line.

                var msgs = new List<string>(perr.msg.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

                string message = string.Empty;
                if (msgs.Count > 3)
                {
                    for (int i = 3; i < msgs.Count; i++)
                    {
                        var line = msgs[i];
                        if (!string.IsNullOrEmpty(line) && !line.StartsWith("Note"))
                        {
                            if (message != string.Empty)
                            {
                                message += " ";
                            }
                            message += line;
                        }
                    }
                    output.Message = message;
                }
                else if (msgs.Count > 0)
                {
                    output.Message = msgs[0];
                }
                else
                {
                    output.Message = "Unspecified validation error";
                }
            }

            return output;
        }
    }
}