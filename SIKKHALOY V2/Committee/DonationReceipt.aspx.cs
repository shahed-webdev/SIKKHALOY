﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationReceipt : System.Web.UI.Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
            var id = Request.QueryString["id"];
        }
    }
}