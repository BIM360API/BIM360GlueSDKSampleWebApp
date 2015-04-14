// Copyright 2012 Autodesk, Inc.  All rights reserved.
// Use of this software is subject to the terms of the Autodesk license agreement 
// provided at the time of installation or download, or which otherwise accompanies 
// this software in either electronic or hard copy form.   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace BIM360GlueSDKSampleWebApp
{
  public class tree_node
  {
    public tree_node()
    {
      url = "";
      title = "";
      key = "";
      isFolder = false;
      expand = false;
      /**
        tooltip = "";
        isLazy = false;
        tooltip = "";
        icon = "";
        addClass = "";
        noLink = false;
        activate = false;
        focus = false;
        select = false;
        hideCheckbox = false;
        unselectable = false;
       **/
      children = null;
    }

    public string title;          // (required) Displayed name of the node (html is allowed here)
    public string key;            // May be used with activate(); select(); find(); ...
    public Boolean isFolder;      // Use a folder icon. Also the node is expandable but not selectable.
    public Boolean expand;        // Initial expanded status.
    public string url;            // Custom attribute for URL

    /**
    public string tooltip;        // Show this popup text.
        public Boolean isLazy;        // Call onLazyRead(); when the node is expanded for the first time to allow for delayed creation of children.
        public string icon;           // Use a custom image (filename relative to tree.options.imagePath). 'null' for default icon; 'false' for no icon.
        public string addClass;       // Class name added to the node's span tag.
        public Boolean noLink;        // Use <span> instead of <a> tag for this node
        public Boolean activate;      // Initial active status.
        public Boolean focus;         // Initial focused status.
        public Boolean select;        // Initial selected status.
        public Boolean hideCheckbox;  // Suppress checkbox display for this node.
        public Boolean unselectable;  // Prevent selection.
                                      // The following attributes are only valid if passed to some functions:
    **/
    public tree_node[] children;  // Array of child nodes.
                                  // NOTE: we can also add custom attributes here.
                                  // This may then also be used in the onActivate(); onSelect() or onLazyTree() callbacks.
  }

  public class BIM360WebServiceAPI
  {
    // Public Member args...
    public Boolean userLoggedIn { get; set; }
    public string user_id { get; set; }
    public HttpStatusCode statusCode { get; set; }
    public user_info_response_v1 userInfo { get; set; }
    public string last_api_error { get; set; }
    public string auth_token;

    // Private Member args...
    private HttpRequest clientRequest;
    private string verboseResponse { get; set; }
    private string responseBody { get; set; }

    // Constructor
    public BIM360WebServiceAPI(HttpRequest aRequest)
    {
      // Initialize Variables...
      userLoggedIn = false;
      clientRequest = aRequest;
      last_api_error = "";

      // Get the auth token cookie
      auth_token = "";
      if (aRequest.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION] != null)
      {
        if (aRequest.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION]["auth_token"] != null)
        {
          auth_token = aRequest.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION]["auth_token"]; 
        }
      }

      // When we get here, need to see if this is a session with an auth token and if so, 
      // just get the user info and check validity
      //
      if (aRequest.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION] != null)
      {
        auth_token = aRequest.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION]["auth_token"];
      }
      if (auth_token != "")
      {
        getAuthenticatedUserInfo();
      }

    }

    //===============================================================
    // Routines to generate the signature components
    //===============================================================
    #region Signature Routines
    public static string generateAPISignature(string aTimestamp)
    {
      // To build a signature, create an MD5 has of the following concatenated information (no delimiters):
      // API Key
      // API Secret
      // Unix Epoch Timestamp
      //
      string baseString = BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY + BIM360SDKDeveloperConfig.BIM360GLUESDK_API_SECRET + aTimestamp;
      string newSig = computeMD5Hash(baseString);
      return newSig;
    }

    public static int getUNIXEpochTimestamp()
    {
      TimeSpan tSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1));
      int timestamp = (int)tSpan.TotalSeconds;
      return timestamp;
    }

    public static string computeMD5Hash(string aString)
    {
      // step 1, calculate MD5 hash from aString
      MD5 md5 = System.Security.Cryptography.MD5.Create();
      byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(aString);
      byte[] hash = md5.ComputeHash(inputBytes);

      // step 2, convert byte array to hex string
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hash.Length; i++)
      {
        sb.Append(hash[i].ToString("x2"));
      }
      return sb.ToString();
    }
    #endregion Signature Routines

    //===================================================================
    // Method: GetBaseURL
    // Purpose: Get the base URL where this service is running
    //===================================================================
    public static string GetBaseURL()
    {
      string appPath = "";

      //Getting the current context of HTTP request
      HttpContext context = HttpContext.Current;

      //Checking the current context content
      if (context != null)
      {
        // Formatting the fully qualified website url/name
        appPath = string.Format("{0}://{1}{2}{3}",
        context.Request.Url.Scheme,
        context.Request.Url.Host,
        context.Request.Url.Port == 80 ? string.Empty : ":" + context.Request.Url.Port,
        context.Request.ApplicationPath);
      }
      return appPath;
    }

    //=================================================================
    // Networking Utility Calls
    //=================================================================
    #region HTTP Request Routines
    private string handleError(WebException ex)
    {
      string reponseAsString = "";
      HttpStatusCode statusCode = HttpStatusCode.Ambiguous;
      if (((HttpWebResponse)ex.Response) != null)
      {
        statusCode = ((HttpWebResponse)ex.Response).StatusCode;
      }

      try
      {
        ex.Response.GetResponseStream();
      }
      catch (Exception localEx)
      {
        string localMsg = localEx.Message;
      }
      reponseAsString += "ERROR: " + ex.Message;

      if (ex.Response != null)
      {
        if (ex.Response.ContentLength > 0)
        {
          using (Stream stream = ex.Response.GetResponseStream())
          {
            using (StreamReader reader = new StreamReader(stream))
            {
              reponseAsString += "\r\n";
              reponseAsString += "<::--------- Server Response Below ---------::>";
              reponseAsString += "\r\n";
              reponseAsString += reader.ReadToEnd().Trim();
            }
          }
        }
      }

      last_api_error = reponseAsString;
      return reponseAsString;
    }

    private void makeHTTPCall(string urlEndpoint, string urlArgs, string reqMethod, string postBody = "")
    {
      string reponseAsString = "";
      var url = BIM360SDKDeveloperConfig.BIM360GLUESDK_BASE_URL;
      if (url.Substring(url.Length - 1) != "/")
      {
        url += "/";
      }
      url += urlEndpoint + ".json";
      if (urlArgs != "")
      {
        url += "?" + urlArgs;
      }
      try
      {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = reqMethod;


        // Setup the user agent
        request.UserAgent = "User-Agent: " + BIM360SDKDeveloperConfig.BIM360GLUESDK_APP_NAME + "/" + BIM360SDKDeveloperConfig.BIM360GLUESDK_APP_VERSION + 
                            "(platform=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_APP_PLATFORM + ", page=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_APP_PAGE +
                            ", info=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_APP_INFO  + ")";
        if (reqMethod != "GET")
        {
          setBody(request, postBody);
        }
        HttpWebResponse response = null;
        // Bump up timeout for debugging purposes (5 mins)
        request.Timeout = 300000;

        response = (HttpWebResponse)request.GetResponse();

        string tHeader = getHeaderFromResponse(response);
        string tBody = getBodyFromResponse(response);

        reponseAsString = tHeader + tBody;
        this.responseBody = tBody;
        this.statusCode = response.StatusCode;
      }
      catch (WebException ex)
      {
        if (((HttpWebResponse)ex.Response) == null)
        {
          this.statusCode = HttpStatusCode.Ambiguous;
        }
        else
        {
          this.statusCode = ((HttpWebResponse)ex.Response).StatusCode;
        }
        reponseAsString += handleError(ex);
      }

      this.verboseResponse = reponseAsString;
    }

    private void setBody(HttpWebRequest request, string requestBody)
    {
      if (requestBody.Length > 0)
      {
        request.ContentType = "application/x-www-form-urlencoded";
        using (Stream requestStream = request.GetRequestStream())
        using (StreamWriter writer = new StreamWriter(requestStream))
        {
          writer.Write(requestBody);
        }
      }
    }

    private string getHeaderFromResponse(HttpWebResponse response)
    {
      string result = "Status code: " + (int)response.StatusCode + " " + response.StatusCode + "\r\n";
      foreach (string key in response.Headers.Keys)
      {
        result += string.Format("{0}: {1} \r\n", key, response.Headers[key]);
      }

      result += "\r\n";
      return result;
    }

    private string getBodyFromResponse(HttpWebResponse response)
    {
      string result = "";
      string tBody = new StreamReader(response.GetResponseStream()).ReadToEnd();
      result += tBody;
      return result;
    }
    #endregion HTTP Request Routines

    //=================================================================
    // Login to BIM 360 Glue Service
    //=================================================================
    public void doLogin(HttpResponse aResponse, string aLoginName, string aPassword)
    {
      // Call the BIM 360 Glue API to Login to the service and get an Auth Key...
      string timestamp = getUNIXEpochTimestamp().ToString();

      string postBody = "";
      postBody += "login_name=" + HttpUtility.UrlEncode(aLoginName);
      postBody += "&password=" + HttpUtility.UrlEncode(aPassword);
      postBody += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      postBody += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      postBody += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      postBody += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      postBody += "&pretty=1";

      this.makeHTTPCall("security/v1/login", "", "POST", postBody);
      if (this.statusCode == HttpStatusCode.OK)
      {
        if (this.responseBody != "")
        {
          JavaScriptSerializer jSer = new JavaScriptSerializer();
          security_login_response_v1 tJSON = jSer.Deserialize<security_login_response_v1>(this.responseBody);
          this.auth_token = tJSON.auth_token;
          this.user_id = tJSON.user_id;
          this.userLoggedIn = true;
        }
      }

      // Now send the response to the browser
      aResponse.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION]["auth_token"] = this.auth_token;
      aResponse.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION].Expires = DateTime.Now.AddDays(BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_LIFESPAN);
      getAuthenticatedUserInfo();
    }

    //=================================================================
    // Logout to BIM 360 Glue Service
    //=================================================================
    public void doLogout(HttpResponse aResponse)
    {
      // Call the BIM 360 Glue API to Logout of the service...
      string timestamp = getUNIXEpochTimestamp().ToString();

      string postBody = "";
      postBody += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      postBody += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      postBody += "&auth_token=" + HttpUtility.UrlEncode(this.auth_token);
      postBody += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      postBody += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      postBody += "&pretty=1";

      this.makeHTTPCall("security/v1/logout", "", "POST", postBody);
      if (this.statusCode == HttpStatusCode.OK)
      {
        this.userLoggedIn = false;
        auth_token = "";
      }

      // Now send the response to the browser
      aResponse.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION]["auth_token"] = this.auth_token;
      aResponse.Cookies[BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_COLLECTION].Expires = DateTime.Now.AddDays(BIM360SDKDeveloperConfig.BIM360GLUESDK_COOKIE_LIFESPAN);

      string redirURL = GetBaseURL() + "/default.aspx";
      aResponse.Redirect(redirURL);
    }

    //=================================================================
    // Get User Information
    //=================================================================
    private void getAuthenticatedUserInfo()
    {
      // Call the BIM 360 Glue API to Logout of the service...
      string timestamp = getUNIXEpochTimestamp().ToString();

      string callArgs = "";
      callArgs += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      callArgs += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      callArgs += "&auth_token=" + HttpUtility.UrlEncode(auth_token);
      callArgs += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      callArgs += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      callArgs += "&pretty=1";

      makeHTTPCall("user/v1/info", callArgs, "GET", "");
      if (this.statusCode == HttpStatusCode.OK)
      {
        JavaScriptSerializer jSer = new JavaScriptSerializer();
        user_info_response_v1 tJSON = jSer.Deserialize<user_info_response_v1>(this.responseBody);
        this.userInfo = tJSON;
        this.userLoggedIn = true;
      }
    }

    //=================================================================
    // Get Project Listing HTML for Authenticated User
    //=================================================================
    public string getProjectList(string searchTerm = "", int page = 1, int page_size = 20)
    {
      string rHTML = "";
      string timestamp = getUNIXEpochTimestamp().ToString();
      string callArgs = "";
      callArgs += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      callArgs += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      callArgs += "&auth_token=" + HttpUtility.UrlEncode(auth_token);
      callArgs += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      callArgs += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      callArgs += "&sterm=" + HttpUtility.UrlEncode(searchTerm);
      callArgs += "&page=" + page.ToString();
      callArgs += "&page_size=" + page_size.ToString();
      callArgs += "&pretty=1";

      makeHTTPCall("project/v1/list", callArgs, "GET", "");
      if (this.statusCode == HttpStatusCode.OK)
      {
        if (this.responseBody != "")
        {
          project_info_response_v1[] tProjects = null;
          JavaScriptSerializer jSer = new JavaScriptSerializer();
          project_list_response_v1 tJSON = jSer.Deserialize<project_list_response_v1>(this.responseBody);
          if (tJSON.project_list != null)
          {
            tProjects = tJSON.project_list;
          }

          if (tProjects != null)
          {
            rHTML += "<center>";
            rHTML += "<table>";
            rHTML += "<table class=\"infoTable\">";
            rHTML += "<tr>";
            rHTML += "<td class=\"infoHeaderCenter\">Project Name</td>";
            rHTML += "<td class=\"infoHeaderCenter\">Project Create Date</td>";
            rHTML += "<td class=\"infoHeaderCenter\">Project ID</td>";
            rHTML += "</tr>";

            foreach (project_info_response_v1 projectInfo in tProjects)
            {
              string pName = HttpUtility.UrlDecode(projectInfo.project_name);

              rHTML += "<tr>";
                rHTML += "<td class=\"infoCellCenter\">";
                  rHTML += "<a href=\"project.aspx?id=" + projectInfo.project_id + "\">" + pName + "</a>";
                rHTML += "</td>";

                rHTML += "<td class=\"infoCellCenter\">";
                rHTML += projectInfo.created_date;
                rHTML += "</td>";

                rHTML += "<td class=\"infoCellCenter\">";
                rHTML += projectInfo.project_id;
                rHTML += "</td>";
              rHTML += "</tr>";
            }
            rHTML += "</table>";
            rHTML += "</center>";
          }
        }
      }

      return rHTML;
    }

    //=================================================================
    // Get Developer Information
    //=================================================================
    public string getDeveloperInformation()
    {
      string rHTML = "";
      string dispSecret = BIM360SDKDeveloperConfig.BIM360GLUESDK_API_SECRET;
      dispSecret = "****************************" + dispSecret.Substring(dispSecret.Length - 4);

      rHTML += "The following information is specified in the <b>BIM360SDKDeveloperConfig.cs</b> file:";
      rHTML += "<br/>";
      rHTML += "<blockquote>";
      rHTML += "<br/>";
      rHTML += "<b>BIM 360 Glue SDK URL:</b> " + BIM360SDKDeveloperConfig.BIM360GLUESDK_BASE_URL;
      rHTML += "<br/>";
      rHTML += "<b>Company ID:</b> " + BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID;
      rHTML += "<br/>";
      rHTML += "<b>API Key:</b> " + BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY;
      rHTML += "<br/>";
      rHTML += "<b>API Secret:</b> " + dispSecret;
      rHTML += "<br/>";
      rHTML += "<b>BIM 360 Glue Display Component URL:</b> " + BIM360SDKDeveloperConfig.GLUE_VIEWER_BASE_URL;

      rHTML += "<br/>";
      rHTML += "<br/>";
      rHTML += "<b><font color=red>NOTE: Obviously, this information would never be shared openly in a production application. It is only here to help document the sample application.</font></b>";
      rHTML += "<br/>";
      rHTML += "<br/>";
      rHTML += "</blockquote>";
      return rHTML;
    }

    //=================================================================
    // Get User Information
    //=================================================================
    public string getUserInformation()
    {
      string rHTML = "";

      rHTML += "The following information is general account information for the authenticated user:";
      rHTML += "<br/>";
      rHTML += "<blockquote>";
      rHTML += "<br/>";
      rHTML += "<b>User Login Name:</b> " + this.userInfo.login_name;
      rHTML += "<br/>";
      rHTML += "<b>Auth Token:</b> " + auth_token;
      rHTML += "<br/>";
      rHTML += "<b>BIM 360 Glue User ID:</b> " + this.userInfo.user_id;
      rHTML += "<br/>";
      rHTML += "<b>Company ID:</b> " + this.userInfo.company;
      rHTML += "<br/>";
      rHTML += "<b>Account Created:</b> " + this.userInfo.created_date;
      rHTML += "<br/>";
      rHTML += "<b>Account Disabled:</b> " + this.userInfo.disabled;
      rHTML += "<br/>";

      
      rHTML += "<br/>";
      rHTML += "</blockquote>";
      return rHTML;
    }

    //=================================================================
    // Get User Information
    //=================================================================
    public project_info_response_v1 getProjectInfo(string projectID)
    {
      string timestamp = getUNIXEpochTimestamp().ToString();
      string callArgs = "";
      callArgs += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      callArgs += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      callArgs += "&auth_token=" + HttpUtility.UrlEncode(auth_token);
      callArgs += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      callArgs += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      callArgs += "&project_id=" + projectID;
      callArgs += "&pretty=1";

      makeHTTPCall("project/v1/info", callArgs, "GET", "");
      project_info_response_v1 tJSON = null;
      if (this.statusCode == HttpStatusCode.OK)
      {
        if (this.responseBody != "")
        {
          JavaScriptSerializer jSer = new JavaScriptSerializer();
          tJSON = jSer.Deserialize<project_info_response_v1>(this.responseBody);
        }
      }

      return tJSON;
    }

    //===================================================================
    // Method: serializeToJSON
    // Purpose: Serialize an object to JSON
    //===================================================================
    public static string serializeToJSON(Object aObject)
    {
      // Default: Return JSON data
      JavaScriptSerializer js = new JavaScriptSerializer();
      try
      {
        string rJSON = "";
        rJSON = js.Serialize(aObject);
        return rJSON;
      }
      catch (Exception e)
      {
        // Error Serializing
        string error = e.ToString();
        return "";
      }
    }
    
    //=================================================================
    // Get the Tree View For the Project
    //=================================================================
    tree_node[] processTreeNode(glue_folder_node[] tTreeNodes, tree_node parentNode = null)
    {
      // Handle the null case
      if (tTreeNodes == null)
      {
        return null;
      }

      tree_node[] new_folder_tree = new tree_node[tTreeNodes.Count()];
      int count = 0;
      foreach (glue_folder_node nodeInfo in tTreeNodes)
      {
        new_folder_tree[count] = new tree_node();
        if (nodeInfo.type == "FOLDER")
        {
          new_folder_tree[count].title = HttpUtility.UrlDecode(nodeInfo.name);
          new_folder_tree[count].isFolder = true;
          new_folder_tree[count].key = nodeInfo.object_id;
          new_folder_tree[count].url = "";
          new_folder_tree[count].expand = true;
          new_folder_tree[count].children = processTreeNode(nodeInfo.folder_contents, new_folder_tree[count]);
          // new_folder_tree[count].tooltip = "";
        }
        else
        {
          new_folder_tree[count].title = HttpUtility.UrlDecode(nodeInfo.name);
          new_folder_tree[count].isFolder = false;
          new_folder_tree[count].key = nodeInfo.object_id;
          // new_folder_tree[count].tooltip = nodeInfo.action_id;

          // Build the URL to view the model
          string timestamp = getUNIXEpochTimestamp().ToString();
          string tURL = "";
          tURL += BIM360SDKDeveloperConfig.GLUE_VIEWER_BASE_URL; 
          tURL += "company_id=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID;
          tURL += "&api_key=" + BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY;
          tURL += "&auth_token=" + auth_token;
          tURL += "&timestamp=" + timestamp;
          tURL += "&sig=" + generateAPISignature(timestamp);
          tURL += "&action_id=" + nodeInfo.action_id;
          tURL += "&gui=";
          new_folder_tree[count].url = tURL;
          new_folder_tree[count].children = null;
        }

        count++;
      }

      // Return the new folder tree
      return new_folder_tree;
    }

    public string getProjectTreeView(string projectID)
    {
      project_info_response_v1 tProj = getProjectInfo(projectID);
      if (tProj == null)
      {
        return "[]";
      }

      tree_node[] newTree = processTreeNode(tProj.folder_tree);
      if (newTree == null)
      {
        return "[]";
      }
      else
      {
        return serializeToJSON(newTree);
      }
    }

    public model_info_response_v1 getModelInfo(string modelID)
    {
      model_info_response_v1 tModel = null;
      string timestamp = getUNIXEpochTimestamp().ToString();
      string callArgs = "";
      callArgs += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      callArgs += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      callArgs += "&auth_token=" + HttpUtility.UrlEncode(auth_token);
      callArgs += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      callArgs += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      callArgs += "&model_id=" + modelID;
      callArgs += "&pretty=1";

      makeHTTPCall("model/v1/info", callArgs, "GET", "");
      if (this.statusCode == HttpStatusCode.OK)
      {
        if (this.responseBody != "")
        {
          JavaScriptSerializer jSer = new JavaScriptSerializer();
          tModel = jSer.Deserialize<model_info_response_v1>(this.responseBody);
        }
      }

      return tModel;
    }

    public model_markup[] getAllModelMarkups(string modelID)
    {
      string timestamp = getUNIXEpochTimestamp().ToString();
      string callArgs = "";
      callArgs += "&company_id=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_COMPANY_ID);
      callArgs += "&api_key=" + HttpUtility.UrlEncode(BIM360SDKDeveloperConfig.BIM360GLUESDK_API_KEY);
      callArgs += "&auth_token=" + HttpUtility.UrlEncode(auth_token);
      callArgs += "&timestamp=" + HttpUtility.UrlEncode(timestamp);
      callArgs += "&sig=" + HttpUtility.UrlEncode(generateAPISignature(timestamp));
      callArgs += "&model_id=" + modelID;
      callArgs += "&pretty=1";

      makeHTTPCall("model/v1/markups", callArgs, "GET", "");
      model_markups_response_v1 tServerResponse = null;
      model_markup[] tMarkups = null;
      if (this.statusCode == HttpStatusCode.OK)
      {
        if (this.responseBody != "")
        {
          JavaScriptSerializer jSer = new JavaScriptSerializer();
          tServerResponse = jSer.Deserialize<model_markups_response_v1>(this.responseBody);
          tMarkups = tServerResponse.markups;
        }
      }

      return tMarkups;
    }

  }
}