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
    public partial class EditAddUsersForm : Form
    {
        object obj;
        public EditAddUsersForm(object obj)
        {
            InitializeComponent();
            this.obj = obj;
        }

        private void EditAddUsersForm_Load(object sender, EventArgs e)
        {
            if(obj != null)
            {
                txtPhoneNumber.Text = obj.GetType().GetProperty("PhoneNumber").GetValue(obj, null).ToString();
                txtFullName.Text = obj.GetType().GetProperty("FullName").GetValue(obj, null).ToString();
            }

        }

        private void EditAddUsersForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(DialogResult == DialogResult.OK)
            {
                if (obj == null)
                {
                    if (Globals.DB.Users.FindByUserId(txtPhoneNumber.Text) == null)
                    {
                        AppData.UsersRow row = Globals.DB.Users.NewUsersRow();
                        row.AccountId = Properties.Settings.Default.PhoneNumber;
                        row.UserId = txtPhoneNumber.Text;
                        row.FullName = txtFullName.Text;
                        Globals.DB.Users.AddUsersRow(row);
                        Globals.DB.Users.AcceptChanges();
                        Globals.DB.Users.WriteXml(string.Format("{0}\\users.dat", Application.StartupPath));
                        e.Cancel = false;
                    }
                    else
                    {
                        MessageBox.Show("This phone number already exists.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }

                else
                {
                    AppData.UsersRow row = Globals.DB.Users.FindByUserId(obj.GetType().GetProperty("PhoneNumber").GetValue(obj, null).ToString());
                    if (row != null)
                    {
                        row.UserId = txtPhoneNumber.Text;
                        row.FullName = txtFullName.Text;
                        Globals.DB.AcceptChanges();
                        Globals.DB.Users.WriteXml(string.Format("{0}\\users.dat", Application.StartupPath));
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                
                
                    

                }
                
            }
        }
    }
}
