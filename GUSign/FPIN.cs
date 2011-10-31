using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GUSign
{
    public partial class FPIN : Form
    {
        
        public FPIN()
        {
            InitializeComponent();
        }

        public string PIN
        {
            get
            {
                return tbPIN.Text;
            }
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void tbPIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                bOK_Click(this, e);
        }

    }
}
