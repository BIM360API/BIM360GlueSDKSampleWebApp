<%@ Page Title="BIM 360 Glue SDK Sample Web Application" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true"
    CodeBehind="default.aspx.cs" Inherits="BIM360GlueSDKSampleWebApp._default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
  <style type="text/css">
    .style1
    {
      width: 478px;
      height: 133px;
    }
    .style2
    {
      height: 22px;
      margin-left: 80px;
    }
    .style4
    {
      width: 256px;
      height: 186px;
    }
    .style5
    {
      font-size: x-large;
      color: #0076A3;
      height: 22px;
    }
    .style9
    {
      height: 22px;
      width: 360px;
      margin-left: 80px;
    }
    .style10
    {
      width: 360px;
    }
    .style15
    {
      height: 22px;
      }
    .style16
    {
      width: 324px;
    }
  </style>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

  <asp:Panel ID="panelAuth" runat="server" style="text-align: left">
    <h1>BIM 360 Glue Project List</h1>
    Click on the Project Name below to see more information about your BIM 360 Glue Project.<br />

    <div style="margin-top: 12px;" id="page_contents" runat="server"></div>
    <br />
  </asp:Panel>


  <asp:Panel ID="panelUnAuth" runat="server">
    <h1>The BIM 360 Glue SDK Sample Web Application</h1>
    <table style="width:100%;">
      <tr>
        <td>
          &nbsp;</td>
        <td>
          The BIM 360 Glue SDK Sample Web Application is an example to show how you can integrate the BIM 360 Glue SDK into your Web Application.<br />
          <br />




          <center>


          <div style="width: 450px;" class="message notice">
          <table style="width: 454px">
            <tr>
              <td align="left" class="style15" colspan="2">
                <strong>Login to the BIM 360 Glue Platform to Continue:</strong></td>
            </tr>
            <tr>
              <td class="style15" align="right">
                <strong>Login Name:</strong></td>
              <td class="style9" align="left" width="270">
                <asp:TextBox ID="txt_login_name" runat="server" Font-Size="Large" Width="270px"></asp:TextBox>
                &nbsp;</td>
            </tr>
            <tr>
              <td align="right" class="style16">
                <strong>Password:</strong></td>
              <td align="left" class="style10" width="270">
                <asp:TextBox ID="txt_password" runat="server" Font-Size="Large" 
                  TextMode="Password" Width="270px"></asp:TextBox>
              </td>
            </tr>
            <tr>
              <td align="right" class="style16">
              &nbsp;</td>
              <td align="right" class="style10" width="270">
                <asp:Button ID="butLogin" runat="server" Font-Size="Large" Text="Login" OnClientClick="return passwordValidate()" />
                <div ID="login_error" runat="server" 
                  style="margin-top: 3px; color: red; text-align: right; vertical-align: bottom;">
                </div>
              </td>
            </tr>
          </table>
          </div>
          </center>






          <br />
        </td>
        <td>
          &nbsp;</td>
      </tr>
      <tr>
        <td>
          &nbsp;</td>
        <td>
          <h3>The BIM 360 Glue SDK</h3>
          <p>
            The BIM 360 Glue Software Development Kit (BIM 360 Glue SDK<sup><small>TM</small></sup>) is a set 
            of tools that will allow third party application developers and integrators to 
            interface with the Autodesk BIM 360 Glue Platform. Using the BIM 360 Glue SDK, a developer 
            could build a variety of applications and integrations. For a fully interactive 
            web based application, a developer could write code finding particular models 
            within the BIM 360 Glue platform via Web Services API and then display it using the 
            customizable Display Component interface. For developers of project management systems, the 
            Web Services API gives them the ability to have full data access to models and 
            meta data in the BIM 360 Glue back end.
          </p>
          <p>
            With the BIM 360 Glue SDK, you can build applications that take full advantage of the 
            power of the Autodesk BIM 360 Glue Platform and bring state of the art BIM technology 
            into your products and services.</p>
        </td>
        <td>
          &nbsp;</td>
      </tr>
      <tr>
        <td class="style2">
        </td>
        <td class="style2">
        </td>
        <td class="style2">
        </td>
      </tr>
      <tr>
        <td>
          &nbsp;</td>
        <td align="center">


        </td>
        <td>
          &nbsp;</td>
      </tr>
    </table>
    <br />
    <br />
  </asp:Panel>

</asp:Content>
