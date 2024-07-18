using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PKs_C_Web_Field_Trainer
{
    public partial class PowerPicker : System.Web.UI.UserControl
    {
        public DefaultForm ParentForm { get; set; }

        public DropDownList DdlPowerPicker => ddl;
        public Label LblName => lbl;

        public DataTable DataSource { get; set; }
        public string ValueMember { get; set; }
        public string DisplayMember { get; set; }

        public void CheckedChanged(object sender, EventArgs e)
        {
            ParentForm.ActionHappened(sender, e); //Trigger the parentform's ActionHappened event
        }
        public void DdlIndex_Changed(object sender, EventArgs e)
        {
            ParentForm.ActionHappened(sender, e); //Trigger the parentform's ActionHappened event
        }
    }
}