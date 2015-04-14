// Copyright 2012 Autodesk, Inc.  All rights reserved.
// Use of this software is subject to the terms of the Autodesk license agreement 
// provided at the time of installation or download, or which otherwise accompanies 
// this software in either electronic or hard copy form.   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;

namespace BIM360GlueSDKSampleWebApp
{
  public partial class _default : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(Request);

      // Is this a new login request?
      if ((txt_login_name.Text != "") && (txt_password.Text != ""))
      {
        apiObj.doLogin(Response, txt_login_name.Text, txt_password.Text);
      }

      // Now, setup the UI properly
      if (apiObj.userLoggedIn)
      {
        Label mpLabel = (Label) Master.FindControl("lab_login_name");
        if(mpLabel != null)
        {
          mpLabel.Text = apiObj.userInfo.login_name;
        }

        Page.Master.FindControl("logoutHeader").Visible = true;
        panelAuth.Visible = true;
        panelUnAuth.Visible = false;

        // Build project list...
        string buildHTML = "";
        buildHTML += apiObj.getProjectList();

        // More UI elements
        buildHTML += "<br/><h1>Various API Functions</h1>";
        buildHTML += "The links below exercise various functionality of the API and helps explain some of this Sample Web App.";
        buildHTML += "<div style=\"margin: 12px 0 0 40px;\">";
        string tID = "1";
        string tLink = "Show BIM 360 Glue SDK Developer Account Information";
        buildHTML += "<a id=\"id_link_" + tID + "\" class=\"id_link_" + tID + "\" href=\"javascript:void();\" onClick=\"getDeveloperInfo(" + tID + ");\">" + tLink + "</a>";

        buildHTML += "<br/><br/>";

        tID = "2";
        tLink = "Show User Account Information";
        buildHTML += "<a id=\"id_link_" + tID + "\" class=\"id_link_" + tID + "\" href=\"javascript:void();\" onClick=\"getUserInfo(" + tID + ");\">" + tLink + "</a>";

        buildHTML += "</div>";
        this.page_contents.InnerHtml = buildHTML;
      }
      else
      {
        if ((txt_login_name.Text != "") && (txt_password.Text != ""))
        {
          this.login_error.InnerHtml = "<b>Login Failed...Try Again</b>";
        }
        Page.Master.FindControl("logoutHeader").Visible = false;
        panelAuth.Visible = false;
        panelUnAuth.Visible = true;
      }
    }

    [WebMethod]
    public static string ajax_GetDeveloperInfo()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      if (apiObj.userLoggedIn)
      {
        buildHTML += apiObj.getDeveloperInformation();
      }
      else
      {
        buildHTML += "<b>Unauthorized: Please login to continue</b>";
      }
      return buildHTML;
    }

    [WebMethod]
    public static string ajax_GetUserInfo()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      if (apiObj.userLoggedIn)
      {
        buildHTML += apiObj.getUserInformation();
      }
      else
      {
        buildHTML += "<b>Unauthorized: Please login to continue</b>";
      }
      return buildHTML;
    }

  }
}
