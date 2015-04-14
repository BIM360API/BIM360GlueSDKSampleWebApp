// Copyright 2012 Autodesk, Inc.  All rights reserved.
// Use of this software is subject to the terms of the Autodesk license agreement 
// provided at the time of installation or download, or which otherwise accompanies 
// this software in either electronic or hard copy form.   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace BIM360GlueSDKSampleWebApp
{
  public static class BIM360SDKDeveloperConfig
  {
    // API Configuration Options
    public static string BIM360GLUESDK_BASE_URL = ConfigurationManager.AppSettings["BIM360GLUESDK_BASE_URL"].ToString();
    public static string BIM360GLUESDK_COMPANY_ID = ConfigurationManager.AppSettings["BIM360GLUESDK_COMPANY_ID"].ToString();
    public static string BIM360GLUESDK_API_KEY = ConfigurationManager.AppSettings["BIM360GLUESDK_API_KEY"].ToString();
    public static string BIM360GLUESDK_API_SECRET = ConfigurationManager.AppSettings["BIM360GLUESDK_API_SECRET"].ToString();

    // Web App Configuration Options
    public static string GLUE_VIEWER_BASE_URL = ConfigurationManager.AppSettings["GLUE_VIEWER_BASE_URL"].ToString();
    public static string BIM360GLUESDK_COOKIE_COLLECTION = ConfigurationManager.AppSettings["BIM360GLUESDK_COOKIE_COLLECTION"].ToString();
    public static int BIM360GLUESDK_COOKIE_LIFESPAN = System.Convert.ToInt32(ConfigurationManager.AppSettings["BIM360GLUESDK_COOKIE_LIFESPAN"].ToString());

    // User Agent Info
    public static string BIM360GLUESDK_APP_NAME = ConfigurationManager.AppSettings["BIM360GLUESDK_APP_NAME"].ToString();
    public static string BIM360GLUESDK_APP_VERSION = ConfigurationManager.AppSettings["BIM360GLUESDK_APP_VERSION"].ToString();
    public static string BIM360GLUESDK_APP_PLATFORM = ConfigurationManager.AppSettings["BIM360GLUESDK_APP_PLATFORM"].ToString();
    public static string BIM360GLUESDK_APP_PAGE = ConfigurationManager.AppSettings["BIM360GLUESDK_APP_PAGE"].ToString();
    public static string BIM360GLUESDK_APP_INFO = ConfigurationManager.AppSettings["BIM360GLUESDK_APP_INFO"].ToString();
  }
}