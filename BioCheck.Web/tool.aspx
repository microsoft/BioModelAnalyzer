<%@ page language="C#" autoeventwireup="true" codebehind="tool.aspx.cs" inherits="BioCheck.Web.tool" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Bio Model Analyzer</title>
    <link rel="stylesheet" type="text/css" href="style.css" />
    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript">
        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
                appSource = sender.getHost().Source;
            }

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
                return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

            errMsg += "Code: " + iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " + args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
    </script>
</head>
<body style="background-image: none">
    <form id="form1" runat="server" style="height: 100%">
    <div id="tool-container">
        <div id="tool">
            <object data="data:application/x-silverlight-2," type="application/x-silverlight-2"
                width="100%" height="100%">
                <param name="source" value="ClientBin/BioCheck.xap" />
                <param name="onError" value="onSilverlightError" />
                <param name="background" value="white" />
                <param name="minRuntimeVersion" value="5.0.61118.0" />
                <param name="autoUpgrade" value="true" />
                <param name="enableRedrawRegions" value="false" />
                <param name="enableFrameRateCounter" value="false" />
                <param name="enableGpuAcceleration" value="false" />
                <param name="EnableCacheVisualization" value="false" />
               
                 <param name="initParams" value="IPAddress=<%=InitParam%>"/>
               
                <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.60310.0" style="text-decoration: none">
                    <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight"
                        style="border-style: none" />
                </a>
            </object>
            <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px;
                border: 0px"></iframe>
        </div>
        <div id="tool-footer">
            <div id="tool-MSR-logo">
                <a href="http://research.microsoft.com/en-us/" target="_blank">
                    <img src="_images/MSR-logo.png"></a>
            </div>
            <div id="tool-site-wide-nav">
                <a href="about.html" target="_blank">About</a> - <a href="help.html" target="_blank">
                    Help</a> - <a href="tou.html" target="_blank">Terms of Use</a>
            </div>
            <div id="tool-logo">
                <img src="_images/logo-crop.png">
            </div>
        </div>
    </div>
    </form>
</body>
</html>
