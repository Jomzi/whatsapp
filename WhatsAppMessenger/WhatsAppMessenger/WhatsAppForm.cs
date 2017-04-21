using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhatsAppApi;

namespace WhatsAppMessenger
{
    public partial class WhatsAppForm : Form
    {
        WhatsApp wa;

        public WhatsAppForm()
        {
            InitializeComponent();
        }

        private void WhatsAppForm_Load(object sender, EventArgs e)
        {
            signOutToolStripMenuItem.Visible = false;
            panel1.BringToFront();
            panel2.Enabled = false;
            listUsers.DisplayMember = "Display";
            listUsers.ValueMember = "PhoneNumber";
            if(Properties.Settings.Default.Remember)
            {
                txtNumber.Text = Properties.Settings.Default.PhoneNumber;
                txtPassword.Text = Properties.Settings.Default.Password;
                chkRemeber.Checked = Properties.Settings.Default.Remember;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            signOutToolStripMenuItem.Visible = false;
            wa.Disconnect();
            panel2.Enabled = false;
            panel1.Enabled = true;
            panel1.BringToFront();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutForm frm = new AboutForm())
            {
                frm.ShowDialog();
            }
        }

        private void chkRemeber_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Remember = chkRemeber.Checked;
            Properties.Settings.Default.PhoneNumber = txtNumber.Text;
            Properties.Settings.Default.Password = txtPassword.Text;
            Properties.Settings.Default.Save();
        }

        private void linkNewAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (RegisterForm frm = new RegisterForm())
            {
                if(frm.ShowDialog() == DialogResult.OK)
                {
                    txtNumber.Text = Properties.Settings.Default.PhoneNumber;
                    txtPassword.Text = Properties.Settings.Default.Password;
                }
            }
        }

        private void btnAddEdit_Click(object sender, EventArgs e)
        {
            using (EditAddUsersForm frm = new EditAddUsersForm(listUsers.SelectedItem))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                    LoadData();
                    listUsers.SelectedIndex = -1;

            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listUsers.Items.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to remove phone number?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var obj = listUsers.SelectedItem;
                    if (obj != null)
                    {
                        AppData.UsersRow row = Globals.DB.Users.FindByUserId(obj.GetType().GetProperty("PhoneNumber").GetValue(obj, null).ToString());
                        Globals.DB.Users.RemoveUsersRow(row);
                        Globals.DB.Users.AcceptChanges();
                        Globals.DB.Users.WriteXml(string.Format("{0}\\users.dat", Application.StartupPath));
                        LoadData();
                        listUsers.SelectedIndex = -1;


                    }
                }
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void btnSignIn_Click(object sender, EventArgs e)
        {
            wa = new WhatsApp(Properties.Settings.Default.PhoneNumber, Properties.Settings.Default.Password, Properties.Settings.Default.FullName, true);
            wa.OnLoginSuccess += Wa_OnLoginSuccess;
            wa.OnLoginFailed += Wa_OnLoginFailed;
            wa.OnConnectFailed += Wa_OnConnectFailed;
            wa.Connect();
            wa.Login();
        }

        private void Wa_OnConnectFailed(Exception ex)
        {
            MessageBox.Show(string.Format("Connect failed: {0}", ex.StackTrace), "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Wa_OnLoginFailed(string data)
        {
            MessageBox.Show(string.Format("Login failed = {0}", data), "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Wa_OnLoginSuccess(string phoneNumber, byte[] data)
        {
            panel1.Enabled = false;
            panel2.BringToFront();
            panel2.Enabled = true;
            signOutToolStripMenuItem.Visible = true;
            Globals.DB.Users.Clear();
            Globals.DB.Accounts.Clear();
            Globals.DB.AcceptChanges();
            string accountFile = string.Format("{0}\\accounts.dat", Application.StartupPath);
            if (File.Exists(accountFile))
                Globals.DB.Accounts.ReadXml(accountFile);
            string userFile = string.Format("{0}\\users.dat", Application.StartupPath);
            if (File.Exists(userFile))
                Globals.DB.Users.ReadXml(userFile);
            LoadData();
        }

        private void LoadData()
        {
            var query = from o in Globals.DB.Users
                        where o.AccountId == Properties.Settings.Default.PhoneNumber
                        select new
                        {
                            PhoneNumber = o.UserId,
                            FullName = o.FullName,
                            Display = string.Format("{0} (+{1})", o.FullName, o.UserId)
                        };
            listUsers.DataSource = query.ToList();       

        }

        private void listUsers_DoubleClick(object sender, EventArgs e)
        {
            var obj = listUsers.SelectedItem;
            if(obj != null)
            {
                using (ChatForm frm = new ChatForm(wa, obj) { Text = obj.GetType().GetProperty("FulName").GetValue(obj, null).ToString() })
                {
                    frm.ShowDialog();
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
