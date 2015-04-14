// Copyright 2012 Autodesk, Inc.  All rights reserved.
// Use of this software is subject to the terms of the Autodesk license agreement 
// provided at the time of installation or download, or which otherwise accompanies 
// this software in either electronic or hard copy form.   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BIM360GlueSDKSampleWebApp
{
  public partial class logout : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      BIM360WebServiceAPI apiObj = new BIM360WebServiceAPI(Request);
      apiObj.doLogout(Response);
    }
  }
}