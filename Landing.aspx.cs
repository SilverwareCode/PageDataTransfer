using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class Landing : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string uid = Request.QueryString["uid"];

        string webData = WebSystem.Page.getData(uid,false);
        Response.Write(webData);
    }
}