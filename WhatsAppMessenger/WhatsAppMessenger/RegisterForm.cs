using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhatsAppMessenger
{
    
    public partial class RegisterForm : Form
    {
        string password;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void grpRequest_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            //Testing.TestClass testClass = new Testing.TestClass();
            //testClass.DoSomething();

            if (String.IsNullOrEmpty(txtNumber.Text))
            {
                MessageBox.Show("Please enter your phone number.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNumber.Focus();
                return;
            }
            if (String.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Please enter your full name.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Focus();
                return;
            }
            String error;

            if (WhatsAppApi.Register.WhatsRegisterV2.RequestCode(txtNumber.Text, out password, out error, "sms"))
            {
                if (!string.IsNullOrEmpty(password))
                    Save();
                else
                {
                    grbRequest.Enabled = false;
                    grbConfirm.Enabled = true;
                }
            }
            else
                MessageBox.Show("Could not generate password.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Save()
        {
            this.grbConfirm.Enabled = false;
            this.grbRequest.Enabled = false;
            Properties.Settings.Default.PhoneNumber = txtNumber.Text;
            Properties.Settings.Default.Password = password;
            Properties.Settings.Default.FullName = txtName.Text;
            Properties.Settings.Default.Save();
            if(Globals.DB.Accounts.FindByAccountId(txtNumber.Text)==null)
            {
                AppData.AccountsRow row = Globals.DB.Accounts.NewAccountsRow();
                row.AccountId = txtNumber.Text;
                row.FullName = txtName.Text;
                row.Password = password;
                Globals.DB.Accounts.AddAccountsRow(row);
                Globals.DB.Accounts.AcceptChanges();
                Globals.DB.Accounts.WriteXml(string.Format("{0}\\accounts.dat", Application.StartupPath));
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtSms.Text))
            {
                MessageBox.Show("Please enter your sms code,", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSms.Focus();
                return;
            }
            String error;

            password = WhatsAppApi.Register.WhatsRegisterV2.RegisterCode(txtNumber.Text, txtSms.Text, out error);
            Save();
        }

    }
}
