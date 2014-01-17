<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bma.aspx.cs" Inherits="BioCheck.Web.bma" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Bio Model Analyzer</title>
	<!-- Third party scripts or code, linked to, called or referenced from this
		 web site are licensed to you by the third parties that own such code,
		 not by Microsoft, see ASP.NET Ajax CDN Terms of Use –
		 http://www.asp.net/ajaxlibrary/CDN.ashx. -->
	<link rel="stylesheet" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.10.3/themes/base/jquery-ui.css" type="text/css" />
	<script src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-2.0.3.min.js" type="text/javascript"></script>
	<script src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.10.3/jquery-ui.min.js" type="text/javascript"></script>

	<link rel="stylesheet" href="style.css" type="text/css" />
	<link rel="stylesheet" href="_scripts/bma.css" type="text/css" />

	<!-- <link rel="stylesheet" href="_scripts/vuePlot.css" type="text/css" />
	<script src="_scripts/vueplot.js" type="text/javascript"></script> -->

	<script src="_scripts/bma.js" type="text/javascript"></script>
</head>
<body>
	<div id="tool-container">
		<div id="tools-top">
		<div id="tool">
			<div id="tools">
				<div id="general-tools">
					<button id="button-undo"><img src="_images/undo.png" title="Undo" /></button>
					<button id="button-redo"><img src="_images/redo.png" title="Redo" /></button>
				</div>

				<div id="drawing-tools">
					<input type="radio" id="button-pointer" name="drawing-button" checked="checked" />
					<label for="button-pointer"><img src="_images/pointer.png" title="Pointer" /></label>
					<input type="radio" id="button-container" name="drawing-button" data-type="Container" />
					<label for="button-container"><img src="_images/container.png" title="Container" data-type="Container" class="draggable-button" /></label>
					<input type="radio" id="button-variable" name="drawing-button" data-type="Variable" />
					<label for="button-variable"><img src="_images/variable.png" title="Variable" data-type="Variable" class="draggable-button" /></label>
					<input type="radio" id="button-constant" name="drawing-button" data-type="Constant" />
					<label for="button-constant"><img src="_images/constant.png" title="Constant" data-type="Constant" class="draggable-button" /></label>
					<input type="radio" id="button-receptor" name="drawing-button" data-type="Receptor" />
					<label for="button-receptor"><img src="_images/receptor.png" title="Receptor" data-type="Receptor" class="draggable-button" /></label>
					<input type="radio" id="button-activate" name="drawing-button" data-type="Activate" />
					<label for="button-activate"><img src="_images/activate.png" title="Activate" data-type="Activate" /></label>
					<input type="radio" id="button-inhibit" name="drawing-button" data-type="Inhibit" />
					<label for="button-inhibit"><img src="_images/inhibit.png" title="Inhibit" data-type="Inhibit" /></label>
				</div>

				<div id="prover-tools">
					<button id="button-run">Run</button>
					<button id="button-simulate">Sim</button>
				</div>

				<div id="zoom-tools">
					<div id="zoom-slider"></div>
					<button id="button-zoomtofit">X</button>
				</div>
			</div>

			<div id="design-surface">
				<svg id="svgroot" version="1.1" xmlns="http://www.w3.org/2000/svg"
					 viewBox="0 0 2000 1000" preserveAspectRatio="xMinyMin meet">
					<!-- <rect fill="none" stroke="green" x="0" y="0" width="100%" height="100%" />
					<rect fill="orange" stroke="black" width="150" height="150" x="50" y="25" />
					<circle fill="red" r="100" cx="1000" cy="500" /> -->
				</svg>
			</div>
		</div>
		</div>
		<div id="tool-footer">
			<div id="tool-MSR-logo">
				<a href="http://research.microsoft.com/en-us/" target="_blank">
					<img src="_images/MSR-logo.png" alt="Logo" /></a>
			</div>
			<div id="tool-site-wide-nav">
				<a href="about.html" target="_blank">About</a> - <a href="help.html" target="_blank">Help</a> - <a href="tou.html" target="_blank">Terms of Use</a>
			</div>
			<div id="tool-logo">
				<img src="_images/logo-crop.png" alt="Logo" />
			</div>
		</div>
	</div>
</body>
</html>
