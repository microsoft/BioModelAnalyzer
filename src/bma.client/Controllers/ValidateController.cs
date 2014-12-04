using System;
using System.Collections.Generic;
using System.Web.Http;

namespace bma.client.Controllers
{
    public class ValidationInput
    {
        public string Formula { get; set; }
    }

    public class ValidationOutput
    {
        public bool IsValid { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public string Message { get; set; }

        public string Details { get; set; }

        public override string ToString()
        {
            return string.Format("IsValid={0} : {1}", IsValid, Message);
        }
    }

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