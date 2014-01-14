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
				<div id="drawingTools">
					<input type="radio" id="toolPointer" name="drawingTool" onclick="drawingToolClick(this)" />
					<label for="toolPointer"><img src="_images/activate.png" title="Pointer" /></label>
					<input type="radio" id="toolContainer" name="drawingTool" onclick="drawingToolClick(this)" data-type="Container" />
					<label for="toolContainer"><img src="_images/container.png" title="Container" data-type="Container" /></label>
					<input type="radio" id="toolVariable" name="drawingTool" onclick="drawingToolClick(this)" data-type="Variable" />
					<label for="toolVariable"><img src="_images/variable.png" title="Container" data-type="Variable" /></label>
					<input type="radio" id="toolConstant" name="drawingTool" onclick="drawingToolClick(this)" data-type="Constant" />
					<label for="toolConstant"><img src="_images/constant.png" title="Container" data-type="Constant" /></label>
					<input type="radio" id="toolReceptor" name="drawingTool" onclick="drawingToolClick(this)" data-type="Receptor" />
					<label for="toolReceptor"><img src="_images/receptor.png" title="Container" data-type="Receptor" /></label>
					<input type="radio" id="toolActivate" name="drawingTool" onclick="drawingToolClick(this)" data-type="Activate" />
					<label for="toolActivate"><img src="_images/activate.png" title="Container" data-type="Activate" /></label>
					<input type="radio" id="toolInhibit" name="drawingTool" onclick="drawingToolClick(this)" data-type="Inhibit" />
					<label for="toolInhibit"><img src="_images/inhibit.png" title="Container" data-type="Inhibit" /></label>
				</div>
			</div>

			<div id="designSurface">
				<svg id="svgroot" version="1.1"
					 xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"
					 viewBox="0 0 1000 1000" preserveAspectRatio="xMidyMid meet">
					 <!-- onmousedown="startDrag()" onmousemove="doDrag()" onmouseup="drawItemOrStopDrag()"> -->
					<!-- <rect id="svgRect" fill="none" stroke="green" x="0" y="0" width="100%" height="100%" stroke-width="10px" />
					<rect fill="orange" stroke="black" width="150" height="150" x="50" y="25" /> -->
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
