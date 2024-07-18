using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Philotic_Knight
{
    public partial class PowerPicker : UserControl
    {
        public PowerPicker()
        {
            InitializeComponent();
        }

        public bool ComboBoxEnabled
        {
            get
            {
                return cboPowerPicker.Enabled;
            }
            set => cboPowerPicker.Enabled = value;
        }
    }
}
