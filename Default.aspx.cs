using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebSystem;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        var c = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"];

        Guid uid = Guid.NewGuid();

        WebSystem.Page.Open("~/Landing.aspx",txtData.Text, "B99628BF-FD27-4B75-B68F-767D5D038B98");
        //WebSystem.Page.Open("~/Landing.aspx", txtData.Text);
    }
}