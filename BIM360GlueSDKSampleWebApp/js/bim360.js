/*
 * These are utility Javascript functions for use with the Sample Web App
 */
// Function: glueUIMsgClear - Clear all messages
function glueUIMsgClear() {
  $.gritter.removeAll();
}

//=============================================================
// Display an informational message
//=============================================================
function glueUIMsgShow(aTitle, aMsg) {
  $.gritter.add({
    title: aTitle,
    text: aMsg,
    image: glue_base_url + 'styles/images/glue_48.png',
    sticky: false,
    fade_in_speed: 'fast',
    fade_out_speed: 250,
    time: '4000'
  });
}

//=============================================================
// Display an error message
//=============================================================
function glueUIMsgShowError(aTitle, aMsg) {
  $.gritter.add({
    class_name: 'gritter-error',
    title: aTitle,
    text: aMsg,
    image: glue_base_url + 'styles/images/glue_48.png',
    sticky: false,
    fade_in_speed: 'fast',
    fade_out_speed: 250,
    time: '4000'
  });
}

//=============================================================
// jQuery Delay Function for fading elements
//=============================================================
jQuery.fn.delay = function (time, func) {
  return this.each(function () {
    setTimeout(func, time);
  });
};

//=============================================================
// Show Framed Info
//=============================================================
function showFramedFaceboxContent(aHeader, aCont) {
  var tContent = '';
  tContent += '<div style="width: 600px">'+
              '<div class="dialog_header">' +
              aHeader +
              '</div>';

  tContent += '<div class="docs">';
  tContent += '<p>';
  tContent += aCont;
  tContent += '</p>';
  tContent += '</div>';

  tContent += '<div class="footer">';
  tContent += '<a class="blueButton" href="javascript:void(0)" onclick="closeFacebox();">Close</a>';
  tContent += '</div>';
  $('.target').hide();

  // Hack for browser bug
  $("[name=contentFrame]").hide();
  $(document).bind('close.facebox', function ()
  {
    $("[name=contentFrame]").show();
  });

  jQuery.facebox(tContent);
}

function showFaceboxContent(aCont) {
  // Hack for browser bug
  $(document).bind('close.facebox', function () {
    $("[name=contentFrame]").show();
  });
  jQuery.facebox(aCont);
}

//=============================================================
// View Developer Info
//=============================================================
function getDeveloperInfo(aID) 
{
  jQuery('.id_link_' + aID).busy(); 

  $.ajax({
    type: "POST",
    url: glue_base_url + 'default.aspx/ajax_GetDeveloperInfo',
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + aID + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      showFramedFaceboxContent("Developer Info", jobj.d);
      jQuery('.id_link_' + aID).busy("hide");
    },
    error: function (result) {
      jQuery('.id_link_' + aID).busy("hide");
      showFramedFaceboxContent("Developer Info", "ERROR: " + result.status + " " + result.statusText);
    }
  });
}

//=============================================================
// View User Info
//=============================================================
function getUserInfo(aID) {
  jQuery('.id_link_' + aID).busy();

  $.ajax({
    type: "POST",
    url: glue_base_url + 'default.aspx/ajax_GetUserInfo',
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + aID + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      showFramedFaceboxContent("User Info", jobj.d);
      jQuery('.id_link_' + aID).busy("hide");
    },
    error: function (result) {
      jQuery('.id_link_' + aID).busy("hide");
      showFramedFaceboxContent("User Info", "ERROR: " + result.status + " " + result.statusText);
    }
  });
}

//=============================================================
// Sanity check for logging in
//=============================================================
function passwordValidate()
{
  if (($('#MainContent_txt_login_name').val() == "") ||
      ($('#MainContent_txt_password').val() == "")) 
  {
    glueUIMsgShowError("Input Needed", "Please enter a user name and password");
    return false;
  }

  return true;
}

//=============================================================
// Load the Project Tree View
//=============================================================
function loadProjectTree(project_id) 
{
  // Show Busy
  jQuery('.tree_busy').busy();

  $.ajax({
    type: "POST",
    url: glue_base_url + 'project.aspx/ajax_GetProjectTree?id=' + project_id,
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + project_id + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      loadTree(jobj.d);
      jQuery('.tree_busy').busy("hide");
    },
    error: function (result, textStatus, errorThrown) {
      glueUIMsgShowError("Server Error", "Unable to load the Project Tree View");
      jQuery('.tree_busy').busy("hide");
    }
  });
}

function loadTree(jsonText)
{
  $("#tree").dynatree({
    autoCollapse: false,
    minExpandLevel: 1,
    imagePath: '../../../styles/dynatree/',
    persist: false,
    /***
    onActivate: function (node) {
    if (node.data.url) {
    $("[name=contentFrame]").attr("src", node.data.url);
    }
    },
    ***/
    children: jQuery.parseJSON(jsonText),

    onActivate: function (node) {
      // $("#link_info").text(node.data.url);
    },
    onClick: function (node, event) {
      // Eat mouse events, while a menu is open
      if ($(".contextMenu:visible").length > 0) {
        return false;
      }
    },
    onDblClick: function (node, event) {
      if (node.data.url) {
        loadModel(node.data.url);
      }
    },
    onKeydown: function (node, event) {
      // Eat keyboard events, when a menu is open
      if ($(".contextMenu:visible").length > 0)
        return false;

      switch (event.which) {
        // Open context menu on [Space] key (simulate right click)   
        case 32: // [Space]
          $(node.span).trigger("mousedown", {
            preventDefault: true,
            button: 2
          })
				.trigger("mouseup", {
				  preventDefault: true,
				  pageX: node.span.offsetLeft,
				  pageY: node.span.offsetTop,
				  button: 2
				});
          return false;
      }
    }

  });

  // Add context menu handler to tree nodes
  bindContextMenu();
}

// --- Contextmenu helper --------------------------------------------------
function bindContextMenu() 
{
  // Add context menu to all nodes:
  $("span.dynatree-node")
			.destroyContextMenu() // unbind first, to prevent duplicates
			.contextMenu({ menu: "myMenu" }, function (action, el, pos) {
			  // The event was bound to the <span> tag, but the node object
			  // is stored in the parent <li> tag
			  var node = el.parents("[dtnode]").attr("dtnode");
			  if (node.data.isFolder) {
			    return;
			  }
			  switch (action) {
			    case "view":
			      // alert("View Model: " + node.data.url);
			      if (node.data.url) {
			        loadModel(node.data.url);
			      }
			      break;
			    case "info":
			      getModelInfo(node.data.key);
			      // alert("Model Info: " + node.data.key);
			      break;
			    case "views":
			      getModelViews(node.data.key);
			      break;
			    case "markups":
			      getModelMarkups(node.data.key);
			      break;
			    default:
			      // alert("Todo: appply action '" + action + "' to node " + node);
			  }
			});
}

//=============================================================
// View Model Information
//=============================================================
function getModelInfo(aID) 
{
  jQuery('.tree_busy').busy();

  $.ajax({
    type: "POST",
    url: glue_base_url + 'project.aspx/ajax_GetModelInfo?id=' + aID,
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + aID + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      showFramedFaceboxContent("Model Information", jobj.d);
      jQuery('.tree_busy').busy("hide");
    },
    error: function (result) {
      jQuery('.tree_busy').busy("hide");
      showFramedFaceboxContent("Model Info", "ERROR: " + result.status + " " + result.statusText);
    }
  });
}

//=============================================================
// View Model Information
//=============================================================
function viewProjectRoster(aID) {
  jQuery('.roster_link').busy();

  $.ajax({
    type: "POST",
    url: glue_base_url + 'project.aspx/ajax_GetProjectRoster?id=' + aID,
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + aID + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      showFramedFaceboxContent("Project Roster", jobj.d);
      jQuery('.roster_link').busy("hide");
    },
    error: function (result) {
      showFramedFaceboxContent("Project Roster", "ERROR: " + result.status + " " + result.statusText);
      jQuery('.roster_link').busy("hide");
    }
  });
}

//=============================================================
// View Model Information
//=============================================================
function getModelViews(aID) {
  jQuery('.tree_busy').busy();

  $.ajax({
    type: "POST",
    url: glue_base_url + 'project.aspx/ajax_GetModelViews?id=' + aID,
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + aID + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      showFramedFaceboxContent("Model Views", jobj.d);
      jQuery('.tree_busy').busy("hide");
    },
    error: function (result) {
      jQuery('.tree_busy').busy("hide");
      showFramedFaceboxContent("Model Views", "ERROR: " + result.status + " " + result.statusText);
    }
  });
}

//=============================================================
// View Model Information
//=============================================================
function getModelMarkups(aID) {
  jQuery('.tree_busy').busy();

  $.ajax({
    type: "POST",
    url: glue_base_url + 'project.aspx/ajax_GetModelMarkups?id=' + aID,
    contentType: "application/json; charset=utf-8",
    data: "{'id': '" + aID + "'}",
    dataType: "text/plain",
    success: function (result) {
      var jobj = jQuery.parseJSON(result);
      showFramedFaceboxContent("Model Markups", jobj.d);
      jQuery('.tree_busy').busy("hide");
    },
    error: function (result) {
      jQuery('.tree_busy').busy("hide");
      showFramedFaceboxContent("Model Markups", "ERROR: " + result.status + " " + result.statusText);
    }
  });
}

//=============================================================
// Load the Model into the Display Component
//=============================================================
function loadModel(aURL) {
  closeFacebox();
  showModelBusyIndicator();

  // Hack for IE
  var modelIFrame = document.getElementById("contentFrame");
  modelIFrame.onreadystatechange = function () {
    if (modelIFrame.readyState == "complete") 
    {
      hideModelBusyIndicator();
    }
  }

  $("[name=contentFrame]").attr("src", aURL);
}

function model_loaded() {
  hideModelBusyIndicator();
}

function showModelBusyIndicator() {
  jQuery('.model_busy').busy();
  $("#model_busy_msg").show();
}

function hideModelBusyIndicator() {
  jQuery('.model_busy').busy("hide");
  $("#model_busy_msg").hide();
}

function closeFacebox() {
  // Hack for browser bug
  $("[name=contentFrame]").show();
  $.facebox.close();
}