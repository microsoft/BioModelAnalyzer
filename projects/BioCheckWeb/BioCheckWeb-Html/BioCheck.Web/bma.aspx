﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bma.aspx.cs" Inherits="BioCheck.Web.bma" %>

<!-- --------------------------------------------------------------------------

  Copyright 2014 Microsoft Corporation.  All Rights Reserved.

  Core BMA web UI

-------------------------------------------------------------------------- -->

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

	<!-- NB: scale 1.0 != 100% - However, disable user zoom change, except that this doesn't seem to have an effect -->
	<meta name="viewport" content="user-scalable=no" />

	<script src="_scripts/bma.js" type="text/javascript"></script>
</head>
<body>
	<div id="tool-container">
		<div id="tools-top">
		<div id="tool">
			<div id="tools">
				<div id="general-tools">
					<button id="button-delete" title="Delete"><img src="_images/delete.png" /></button>
					<button id="button-undo" title="Undo"><img src="_images/undo.png" /></button>
					<button id="button-redo" title="Redo"><img src="_images/redo.png" /></button>
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
					<button id="button-run" title="Run proof">Run</button>
					<button id="button-simulate" title="Run simulation">Sim</button>
				</div>

				<div id="zoom-tools">
					<div id="zoom-slider"></div>
					<button id="button-zoomtofit" title="Zoom to fit"><img src="_images/zoomtofit.png" /></button>
				</div>
			</div>

			<div id="dialog-variable" title="Properties">
				<table class="dialog-grid" width="100%">
					<tr>
						<td>Name: </td>
						<td colspan="3"><input type="text" id="variable-name" name="name" class="text ui-widget-content fill-width" /></td>
					</tr>
					<tr>
						<td>Range: </td>
						<td><input type="text" id="variable-range0" name="range" class="text ui-widget-content" size="5" /></td>
						<td>&nbsp;</td>
						<td><input type="text" id="variable-range1" name="range" class="text ui-widget-content" size="5" /></td>
					</tr>
					<tr>
						<td colspan="4">Target function</td>
					</tr>
					<tr>
						<td>
							<select id="variable-function-list" size="6" class="fill-width">
								<option data-syntax="var(name)" data-description="A variable, where name is the name of the variable" value="var()" data-back="1">var</option>
								<option data-syntax="avg(x,y,z)" data-description="The average of a list of expressions. E.g., avg( var(X); var(Y); 22; var(Z)*2 )" value="avg(,)" data-back="2">avg</option>
								<option data-syntax="min(x, y)" data-description="The minimum of two expressions. E.g., min(var(X),var(Y)), or min(var(X), 0))" value="min(,)" data-back="2">min</option>
								<option data-syntax="max(x, y)" data-description="The maximum of two expressions. E.g., max(var(X),var(Y))" value="max(,)" data-back="2">max</option>
								<option data-syntax="22 or const(22)" data-description="An integer number. E.g., 1234, 42, -9" value="const()" data-back="1">const</option>
								<option data-syntax="x + y" data-description="Usual addition operator. E.g., 2+3, 44 + var(X)" value=" + " data-back="0">plus</option>
								<option data-syntax="x - y" data-description="Usual subtraction operator. E.g., 2-3, 44 - var(X)" value=" - " data-back="0">minus</option>
								<option data-syntax="x * y" data-description="Usual multiplication operator. E.g., 2*3, 44 * var(X)" value=" * " data-back="0">times</option>
								<option data-syntax="x / y" data-description="Usual division operator. E.g., 2/3, 44 / var(X)" value=" / " data-back="0">div</option>
								<option data-syntax="ceil(x)" data-description="The ceiling of an expression. E.g., ceil(var(X))" value="ceil()" data-back="1">ceil</option>
								<option data-syntax="floor(x)" data-description="The floor of an expression. E.g., floor(var(X))" value="floor()" data-back="1">floor</option>
							</select>
						</td>
						<td colspan="3">
							<button id="variable-function-insert">Insert</button>
							<p><b id="variable-function-syntax"></b></p>
							<p id="variable-function-description"></p>
						</td>
					</tr>
					<tr>
						<td colspan="4">
							<select id="variable-variable-list" size="1" class="fill-width" />
						</td>
					</tr>
					<tr>
						<td colspan="4">
							<textarea id="variable-function" rows="3" class="text ui-widget-content fill-width"></textarea>
						</td>
					</tr>
				</table>
			</div>

			<div id="dialog-container" title="Properties">
				<table class="dialog-grid" width="100%">
					<tr>
						<td>Name: </td>
						<td><input type="text" id="container-name" name="name" class="text ui-widget-content fill-width" /></td>
					</tr>
				</table>
			</div>

			<div id="design-surface">
				<svg id="svgroot" version="1.1" xmlns="http://www.w3.org/2000/svg"
					 viewBox="0 0 2000 1000" preserveAspectRatio="xMinyMin meet">
					<defs>
						<marker id="link-activate" refX="5" refY="5" markerUnits="strokeWidth" markerWidth="10" markerHeight="10" orient="auto" pointer-events="none">
							<path d="M0 0L5 5 0 10" fill="none" stroke="black" stroke-width="1px" />
						</marker>
						<marker id="link-inhibit" refX="0" refY="5" markerUnits="strokeWidth" markerWidth="10" markerHeight="10" orient="auto" pointer-events="none">
							<path d="M0 0L0 10" fill="none" stroke="black" stroke-width="1px" />
						</marker>
					</defs>
					<!-- <rect fill="none" stroke="green" x="0" y="0" width="100%" height="100%" />
					<rect fill="orange" stroke="black" width="150" height="150" x="50" y="25" />
					<circle fill="red" r="100" cx="1000" cy="500" /> -->
					<!-- <line style="stroke: blue; stroke-width: 2;" marker-end="url('#link-activate')" marker-start="url('#link-inhibit')" x1="20" y1="20" x2="300" y2="100" /> -->
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