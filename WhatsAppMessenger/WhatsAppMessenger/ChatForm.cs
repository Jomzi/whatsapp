using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhatsAppApi;

namespace WhatsAppMessenger
{
    public partial class ChatForm : Form
    {
        string fileName;
        int index;
        WhatsApp wa;
        string phoneNumber;
        string fullName;
        //System.Timers.Timer timer;
        Thread thread;

        public ChatForm(WhatsApp wa,object obj)
        {
            InitializeComponent();
            this.phoneNumber = obj.GetType().GetProperty("PhoneNumber").GetValue(obj, null).ToString();
            this.fullName = obj.GetType().GetProperty("FullName").GetValue(obj, null).ToString();
            this.wa = wa;
            wa.OnGetMessage += Wa_OnGetMessage;
            //wa.OnGetMessageImage += Wa_OnGetMessageImage;
        }

        delegate void UpdateTextMessage(WebBrowser web, string value);

        void UpdateTextData(WebBrowser web, string value)
        {
            if (web.Document != null)
            {
                HtmlElement element = web.Document.CreateElement("p");
                element.InnerText = value;
                web.Document.Body.AppendChild(element);
            }
            else
                web.DocumentText = value;
        }

        delegate void UpdateImageMessage(WebBrowser web, string file, string url, byte[] data);
         
        void UpdateImageData(WebBrowser web, String file, string url, byte[]data)
        {
            if (web.Document != null)
            {
                HtmlElement pElement = web.Document.CreateElement("p");
                pElement.InnerText = String.Empty;
                web.Document.Body.AppendChild(pElement);

                HtmlElement imgElement = web.Document.CreateElement("img");
                imgElement.SetAttribute("src", url);
                web.Document.Body.AppendChild(imgElement);
            }
            else
                web.DocumentText = "<img src= '" + url + "'/>";
        }

        private void Wa_OnGetMessageImage(WhatsAppApi.Helper.ProtocolTreeNode mediaNode, string from, string id, string fileName, int fileSize, string url, byte[] preview)
        {
            UpdateImageMessage img = UpdateImageData;
            if (webBrowser.InvokeRequired)
                Invoke(img, webBrowser, fileName, url, preview);
        }

        private void Wa_OnGetMessage(WhatsAppApi.Helper.ProtocolTreeNode messageNode, string from, string id, string name, string message, bool receipt_sent)
        {
            UpdateTextMessage text = UpdateTextData;
            if (webBrowser.InvokeRequired)
                Invoke(text, webBrowser, string.Format("{0}:{1}", fullName, message));
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(wa.ConnectionStatus == ApiBase.CONNECTION_STATUS.LOGGEDIN)
            {
                if(!string.IsNullOrEmpty(fileName))
                {
                    byte[] img = File.ReadAllBytes(fileName);
                    switch(index)
                    {
                        case 1:
                            wa.SendMessageImage(phoneNumber + "@s.whatsapp.net", img, ApiBase.ImageType.PNG);
                            break;

                        case 2:
                            wa.SendMessageImage(phoneNumber + "@s.whatsapp.net", img, ApiBase.ImageType.JPEG);
                            break;

                        case 3:
                            wa.SendMessageImage(phoneNumber + "@s.whatsapp.net", img, ApiBase.ImageType.GIF);
                            break;
                    }
                    if (webBrowser.Document != null)
                    {
                        HtmlElement pElement = webBrowser.Document.CreateElement("p");
                        pElement.InnerText = String.Empty;
                        webBrowser.Document.Body.AppendChild(pElement);

                        HtmlElement imgElement = webBrowser.Document.CreateElement("img");
                        imgElement.SetAttribute("src", fileName);
                        webBrowser.Document.Body.AppendChild(imgElement);
                    }
                    else
                        webBrowser.DocumentText = "<img src= '" + fileName + "'/>";

                    fileName = null;
                    lblPath.Text = "Image ???";
                    //timer.Start();
                }
                else
                {
                    //timer.Stop();
                    if (string.IsNullOrEmpty(txtMessage.Text))
                        return;
                    wa.SendMessage(phoneNumber, txtMessage.Text);
                    if (webBrowser.Document != null)
                    {
                        HtmlElement element = webBrowser.Document.CreateElement("p");
                        element.InnerText = string.Format("{0}:{1}",Properties.Settings.Default.FullName,txtMessage.Text);
                        webBrowser.Document.Body.AppendChild(element);
                    }
                    else
                        webBrowser.DocumentText = string.Format("{0}:{1}", Properties.Settings.Default.FullName, txtMessage.Text);
                    txtMessage.Clear();
                    txtMessage.Focus();
                    //timer.Start();
                }
            }
        }

        private void btnImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "PNG|*.png|JPG|+.jpg|GIF|*.gif", ValidateNames = true, Multiselect = false })

            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fileName = ofd.FileName;
                    index = ofd.FilterIndex;
                    lblPath.Text = string.Format("Path: {0}", ofd.FileName);
                    //timer.Stop();
                    wa.Disconnect();
                    Thread.Sleep(1000);
                    wa.Connect();
                    wa.Login();
                }
            }
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            //timer = new System.Timers.Timer();
            //timer.Interval = 6000;
            //timer.Elapsed += Timer_Elapsed;
            thread = new Thread(t =>
                {
                    while (wa != null)
                    {
                        if (wa.ConnectionStatus == ApiBase.CONNECTION_STATUS.LOGGEDIN)
                        {
                            if (string.IsNullOrEmpty(fileName))
                            {
                                wa.PollMessages();
                                Thread.Sleep(3000);
                                continue;
                            }
                            else
                            {
                                Thread.Sleep(3000);
                                continue;
                            }
                        }
                    }
                })

            { IsBackground = true };
            thread.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (wa.ConnectionStatus == ApiBase.CONNECTION_STATUS.LOGGEDIN)
            {
                if (!wa.HasMessages())
                    wa.PollMessages();
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //timer.Stop();
            if (thread.IsAlive)
                thread.Abort();
        }
    }
}
