<%@ Page Title="BIM 360 Glue SDK Sample Web Application" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true"
    CodeBehind="project.aspx.cs" Inherits="BIM360GlueSDKSampleWebApp.project" %>

<asp:Content ID="TreeCode" runat="server" ContentPlaceHolderID="PageCode">
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <div id="page_header" runat="server"></div>
    <table style="width: 100%; margin: 0px 0px 6px 0px;">
      <tr>
        <td align="left">
          <div id="page_sub_header" runat="server"></div>
        </td>
        <td align="right">

          <table>
            <tr>
              <td width=32 align="center">
                <div id="model_busy" class="model_busy"></div>
              </td>
              <td>
                <div id="model_busy_msg" style="display: none; font-weight: bold;">Model Loading...</div>
              </td>
            </tr>
          </table>

        </td>
      </tr>
    </table>

    <table>
	    <colgroup>
		    <col width="200" valign="top">
		    <col width="720">
	    </colgroup>
	    <tr>
		    <td valign="top" style="padding: 0; margin: 0;">
          <div style="color: #013668; margin: 6px 0px 0px 0px; width: 190px; 
                      text-align: center; border: 1px solid #CCCCCC; 
                      background: #eeeeee; font-size: 10px;">Right click on model for options</div>
			    <div id="tree"> </div>
          <div id="tree_busy" class="tree_busy"></div>
		    </td>
		    <td valign="top" style="background-color: #fff;">
			    <iframe onload="model_loaded();" src='<%= this.ResolveClientUrl("~/styles/blank.html") %>' name="contentFrame" id="contentFrame" width="720" height="800"
					        scrolling="no" marginheight="0" marginwidth="0" frameborder="0">
			      <p>Your browser does not support iframes</p>
			    </iframe>
          <!--
            <div style="border: 1px solid #CCCCCC; background: #FFD324; font-size: 9px;" id="link_info"></div>
          -->
		    </td>
	    </tr>
    </table>

    <div id="page_contents" runat="server"></div>
    <br />


	<!-- Definition of context menu -->
	<ul id="myMenu" class="contextMenu">
		<li class="view"><a href="#view">View Model</a></li>
		<li class="info"><a href="#info">Information</a></li>
		<li class="views"><a href="#views">Views</a></li>
		<li class="markups"><a href="#markups">Markups</a></li>
	</ul>

</asp:Content>
