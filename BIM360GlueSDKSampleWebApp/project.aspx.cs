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
using System.IO;
using System.Text;

namespace BIM360GlueSDKSampleWebApp
{
  public partial class project : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(Request);

      // If no logged on, just redirect to the home page
      if (!apiObj.userLoggedIn)
      {
        string redirURL = BIM360WebServiceAPI.GetBaseURL() + "/default.aspx";
        Response.Redirect(redirURL);
      }

      // Get the Project ID from the URL.. if not valid, just display message
      string projectID = Request.Params["id"];
      if (projectID == "")
      {
        this.page_header.InnerHtml = "<h1>Invalid Project ID</b>: [" + projectID + "]</h1>";
        return;
      }

      project_info_response_v1 tProj = apiObj.getProjectInfo(projectID);
      if (tProj != null)
      {
        string tHead = "<h1>Project: " + HttpUtility.UrlDecode(tProj.project_name) + " [ID=" + projectID + "]</h1>";
        this.page_header.InnerHtml = tHead;

        string tHead2 = "<b>Project Created: </b>" + tProj.created_date + " <b>Roster Count: </b>" + tProj.project_roster.Count();
        tHead2 += " <a class=\"roster_link\" id=\"roster_link\" href=\"javascript:void();\" onClick=\"viewProjectRoster('" + projectID + "');\">(View Roster)</a>";
        this.page_sub_header.InnerHtml = tHead2;
      }
      else
      {
        this.page_header.InnerHtml = "<h1>Project: [ID=" + projectID + "]</h1>";
      }

      string tJS = "";
      tJS += "<script>";
      tJS += "loadProjectTree(\"" + projectID + "\");";
      tJS += "</script>";
      this.page_contents.InnerHtml = tJS;
    }

    [WebMethod]
    static public string ajax_GetProjectTree()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      // If no logged on, just redirect to the home page
      if (!apiObj.userLoggedIn)
      {
        return "[]";
      }

      // Get the ID
      string projectID = aRequest.Params["id"];
      HttpContext.Current.Response.ContentType = "application/json; charset=UTF-8;";

      // Build project list...
      buildHTML += apiObj.getProjectTreeView(projectID);
      return buildHTML;
    }

    [WebMethod]
    static public string ajax_GetModelInfo()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      // If no logged on, just redirect to the home page
      if (!apiObj.userLoggedIn)
      {
        return "<b>Unauthorized: Please login to continue</b>";
      }

      // Get the ID
      string modelID = aRequest.Params["id"];

      // Get the model info...
      model_info_response_v1 tModel = apiObj.getModelInfo(modelID);
      if (tModel == null)
      {
        return "<b>Model Not Found</b>";
      }

      buildHTML += "<center>";
      buildHTML += "<table width=500 style=\"border: 1px solid #CCCCCC;\">";
      buildHTML += "<tr bgcolor=\"#CCCCCC\">";
      buildHTML += "<td><b>Attribute</b></td>";
      buildHTML += "<td><b>Value</b></td>";
      buildHTML += "</tr>";

      buildHTML += addRow("company_id", tModel.company_id);
      buildHTML += addRow("project_id", tModel.project_id);
      buildHTML += addRow("model_name", tModel.model_name);
      buildHTML += addRow("model_id", tModel.model_id);
      buildHTML += addRow("model_version", tModel.model_version.ToString());
      buildHTML += addRow("model_version_id", tModel.model_version_id);
      buildHTML += addRow("is_merged_model", tModel.is_merged_model.ToString());
      buildHTML += addRow("action_id", tModel.action_id);
      buildHTML += addRow("created_by", tModel.created_by);
      buildHTML += addRow("created_date", tModel.created_date);
      buildHTML += addRow("modified_by", tModel.modified_by);
      buildHTML += addRow("modified_date", tModel.modified_date);
      buildHTML += addRow("parent_folder_id", tModel.parent_folder_id);
      buildHTML += addRow("file_parsed_status", tModel.file_parsed_status.ToString());

      // Build the URL to view the model
      string timestamp = BIM360WebServiceAPI.getUNIXEpochTimestamp().ToString();
      string tURL = "";
      tURL += BIM360SDKDeveloperConfig.GLUE_VIEWER_BASE_URL;

      // Add question mark if needed 
      if (tURL.Substring(tURL.Length - 1) != "?")
      {
        tURL += "?";
      }

      tURL += "<br/>company_id=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID;
      tURL += "<br/>&amp;api_key=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY;
      tURL += "<br/>&amp;auth_token=" + apiObj.auth_token;
      tURL += "<br/>&amp;timestamp=" + timestamp;
      tURL += "<br/>&amp;sig=" + BIM360WebServiceAPI.generateAPISignature(timestamp);
      tURL += "<br/>&amp;action_id=" + tModel.action_id;
      tURL += "<br/>&amp;gui=";
      buildHTML += addRow("View URL", tURL);

      buildHTML += "</table>";
      return buildHTML;
    }

    static string addRow(string aField, string aVal)
    {
      string rHTML = "";
      rHTML += "<tr style=\"border-bottom: 1px solid #CCCCCC\">";
      rHTML += "<td style=\"border-right: 1px solid #CCCCCC\">";
        rHTML += "<b>" + aField + "</b>";
        rHTML += "</td>";

        rHTML += "<td>";
        rHTML += aVal;
        rHTML += "</td>";
      rHTML += "</tr>";
      return rHTML;
    }

    [WebMethod]
    static public string ajax_GetProjectRoster()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      // If no logged on, just redirect to the home page
      if (!apiObj.userLoggedIn)
      {
        return "<b>Unauthorized: Please login to continue</b>";
      }

      // Get the ID
      string projectID = aRequest.Params["id"];

      // Get the model info...
      project_info_response_v1 tProj = apiObj.getProjectInfo(projectID);
      if ((tProj == null) || (tProj.project_roster == null))
      {
        return "<b>Roster Not Found</b>";
      }

      buildHTML += "<center>";
      buildHTML += "<table width=500 style=\"border: 1px solid #CCCCCC;\">";
      buildHTML += "<tr bgcolor=\"#CCCCCC\">";
      buildHTML += "<td><b>Login Name</b></td>";
      buildHTML += "<td><b>Date Added</b></td>";
      buildHTML += "</tr>";

      foreach (user_info_response_v1 tUser in tProj.project_roster)
      {
        buildHTML += addRow(tUser.login_name, tUser.created_date);
      }

      buildHTML += "</table>"; 
      
      return buildHTML;
    }

    [WebMethod]
    static public string ajax_GetModelViews()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      // If no logged on, just redirect to the home page
      if (!apiObj.userLoggedIn)
      {
        return "<b>Unauthorized: Please login to continue</b>";
      }

      // Get the ID
      string modelID = aRequest.Params["id"];

      // Get the model info...
      model_info_response_v1 tModel = apiObj.getModelInfo(modelID);
      if (tModel == null)
      {
        return "<b>Model Not Found</b>";
      }

      if (tModel.view_tree == null)
      {
        return "<div class=\"message notice\" style=\"margin: 0px 16px 6px 16px;\"><b>This model does not contain any Views.</b></div>";
      }

      buildHTML += "<center>";
      buildHTML += "<table width=500 style=\"border: 1px solid #CCCCCC;\">";
      buildHTML += "<tr bgcolor=\"#CCCCCC\">";
      buildHTML += "<td><b>Name</b></td>";
      buildHTML += "<td><b>Create Date</b></td>";
      buildHTML += "<td><b>Creator</b></td>";
      buildHTML += "</tr>";

      foreach (model_view_node tView in tModel.view_tree)
      {
        if (tView.type != "VIEW")
        {
          continue;
        }

        buildHTML += "<tr style=\"border-bottom: 1px solid #CCCCCC\">";

        buildHTML += "<td style=\"border-right: 1px solid #CCCCCC\">";

          // Build the URL to view the model
          string timestamp = BIM360WebServiceAPI.getUNIXEpochTimestamp().ToString();
          string tURL = "";
          tURL += BIM360SDKDeveloperConfig.GLUE_VIEWER_BASE_URL;

          // Add question mark if needed 
          if (tURL.Substring(tURL.Length - 1) != "?")
          {
            tURL += "?";
          }

          tURL += "company_id=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID;
          tURL += "&api_key=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY;
          tURL += "&auth_token=" + apiObj.auth_token;
          tURL += "&timestamp=" + timestamp;
          tURL += "&sig=" + BIM360WebServiceAPI.generateAPISignature(timestamp);
          tURL += "&action_id=" + tView.action_id;
          tURL += "&gui=";

          buildHTML += "<a href=\"javascript:void(0);\" onClick=\"loadModel('" + tURL + "');\">";
          buildHTML += HttpUtility.UrlDecode(tView.name);
          buildHTML += "</a>";

          buildHTML += "</td>";

          buildHTML += "<td style=\"border-right: 1px solid #CCCCCC\">";
          buildHTML += tView.created_date;
          buildHTML += "</td>";

          buildHTML += "<td>";
          buildHTML += tView.created_by;
          buildHTML += "</td>";
        buildHTML += "</tr>";
      }

      buildHTML += "</table>";
      return buildHTML;
    }

    [WebMethod]
    static public string ajax_GetModelMarkups()
    {
      string buildHTML = "";
      HttpRequest aRequest = HttpContext.Current.Request;
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(aRequest);

      // If no logged on, just redirect to the home page
      if (!apiObj.userLoggedIn)
      {
        return "<b>Unauthorized: Please login to continue</b>";
      }

      // Get the ID
      string modelID = aRequest.Params["id"];

      // Get the model info...
      model_markup[] tMarkups = apiObj.getAllModelMarkups(modelID);
      if (tMarkups == null)
      {
        return "<div class=\"message notice\" style=\"margin: 0px 16px 6px 16px;\"><b>This model does not contain any Markups.</b></div>";
      }

      buildHTML += "<center>";
      buildHTML += "<table width=500 style=\"border: 1px solid #CCCCCC;\">";
      buildHTML += "<tr bgcolor=\"#CCCCCC\">";
      buildHTML += "<td><b>Name</b></td>";
      buildHTML += "<td><b>Create Date</b></td>";
      buildHTML += "<td><b>Creator</b></td>";
      buildHTML += "</tr>";

      // Show the markups
      foreach (model_markup aMarkup in tMarkups)
      {
        buildHTML += "<tr style=\"border-bottom: 1px solid #CCCCCC\">";

        buildHTML += "<td style=\"border-right: 1px solid #CCCCCC\">";

        // Build the URL to view the model
        string timestamp = BIM360WebServiceAPI.getUNIXEpochTimestamp().ToString();
        string tURL = "";
        tURL += BIM360SDKDeveloperConfig.GLUE_VIEWER_BASE_URL;

        // Add question mark if needed 
        if (tURL.Substring(tURL.Length - 1) != "?")
        {
          tURL += "?";
        }

        tURL += "company_id=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID;
        tURL += "&api_key=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY;
        tURL += "&auth_token=" + apiObj.auth_token;
        tURL += "&timestamp=" + timestamp;
        tURL += "&sig=" + BIM360WebServiceAPI.generateAPISignature(timestamp);
        tURL += "&action_id=" + aMarkup.action_id;
        tURL += "&gui=";

        buildHTML += "<a href=\"javascript:void(0);\" onClick=\"loadModel('" + tURL + "');\">";
        buildHTML += HttpUtility.UrlDecode(aMarkup.name);
        buildHTML += "</a>";

        buildHTML += "</td>";

        buildHTML += "<td style=\"border-right: 1px solid #CCCCCC\">";
        buildHTML += aMarkup.created_date;
        buildHTML += "</td>";

        buildHTML += "<td>";
        buildHTML += aMarkup.created_by;
        buildHTML += "</td>";
        buildHTML += "</tr>";
      }

      buildHTML += "</table>";
      return buildHTML;
    }

  }
}
