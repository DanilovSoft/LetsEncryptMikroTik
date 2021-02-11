using LetsEncryptMikroTik;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LetsEncryptMikroTik
{
    public partial class Form1 : Form
    {
        // Create byte array for additional entropy when using Protect method.
        private static readonly byte[] s_aditionalEntropy = { 41, 23, 98, 128, 246, 100, 21, 24 };

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        private string _certFilePath;
        private string _privKeyPath;

        public Form1()
        {
            InitializeComponent();

            textBox_MtAddress.Text = Properties.Settings.Default.MtAddress;
            textBox_mtPort.Text = Properties.Settings.Default.MikroTikPort.ToString();
            textBox_MtLogin.Text = Properties.Settings.Default.MikroTikLogin;
            textBox_ftpLogin.Text = Properties.Settings.Default.FtpLogin;
            textBox_mtPassword.Text = Unprotect(Properties.Settings.Default.MikroTikPassword);
            textBox_ftpPassword.Text = Unprotect(Properties.Settings.Default.FtpPassword);
            textBox_domainName.Text = Properties.Settings.Default.DomainName;
            textBox_email.Text = Properties.Settings.Default.Email;
            textBox_wan.Text = Properties.Settings.Default.WanIface;
            checkBox_ssl.Checked = Properties.Settings.Default.MtApiSsl;

            //comboBox_lec.DataSource = Program.GetAddresses();
            int selIndex = Properties.Settings.Default.SelectedAddress;
            if (comboBox_lec.Items.Count - 1 >= selIndex)
            {
                comboBox_lec.SelectedIndex = Properties.Settings.Default.SelectedAddress;
            }
            comboBox_lec.SelectedIndexChanged += ComboBox_lec_SelectedIndexChanged;
        }

        private void Button_Start_Click(object sender, EventArgs e)
        {
            
        }

        private void OnLogMessage(string message)
        {
            if(richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke((Action<string>)OnLogMessage, message);
            }
            else
            {
                richTextBox1.AppendText(message);
            }
        }

        private static string Protect(string s)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                byte[] data = Encoding.UTF8.GetBytes(s);
                byte[] @protected = ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(@protected);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private static string Unprotect(string dataBase64)
        {
            if (string.IsNullOrEmpty(dataBase64))
                return null;

            try
            {
                byte[] data = Convert.FromBase64String(dataBase64);
                //Decrypt the data using DataProtectionScope.CurrentUser.
                byte[] unprotected = ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(unprotected);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private void TextBox1_Leave(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                Properties.Settings.Default.Save();
            }
        }

        private void TextBox_MtAddress_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MtAddress = textBox_MtAddress.Text.Trim();
        }

        private void MikrotikPort_ValueChanged(object sender, EventArgs e)
        {
            
        }

        private void TextBox_MtLogin_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MikroTikLogin = textBox_MtLogin.Text.Trim();
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MikroTikPassword = Protect(textBox_mtPassword.Text);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {

        }

        private void TextBox_mtPort_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void TextBox_mtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TextBox_mtPort_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MikroTikPort = int.Parse(textBox_mtPort.Text);
        }

        private void TextBox_ftpPassword_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FtpPassword = Protect(textBox_ftpPassword.Text.Trim());
        }

        private void TextBox_ftpLogin_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FtpLogin = textBox_ftpLogin.Text;
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DomainName = textBox_domainName.Text.Trim();
        }

        private void TextBox_email_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Email = textBox_email.Text.Trim();
        }

        private void TextBox1_TextChanged_1(object sender, EventArgs e)
        {
            Properties.Settings.Default.WanIface = textBox_wan.Text.Trim();
        }

        private void TextBox_MtAddress_Leave(object sender, EventArgs e)
        {
            
        }

        //private class Log : InMemorySink
        //{
        //    private readonly Form1 _form;

        //    public Log(Form1 form)
        //    {
        //        _form = form;
        //    }

        //    public override void NewEntry(string message)
        //    {
        //        _form.OnLogMessage(message);
        //    }
        //}

        private void Button1_Click_1(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cert");

            if (checkBox_saveFile.Checked)
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                Process.Start("explorer.exe", folderPath);
            }
            else
            {
                if (!Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
                }
                else
                {
                    Process.Start("explorer.exe", folderPath);
                }
            }
        }

        private void ВыбратьСертификатToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!выбратьСертификатToolStripMenuItem.Checked)
            {
                using (var dialog = new OpenFileDialog())
                {
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cert");
                    dialog.InitialDirectory = folderPath;

                    dialog.Title = "Выберите сертификат";
                    dialog.FileName = "cert.pem";
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    _certFilePath = dialog.FileName;

                    dialog.Title = "Выберите приватный ключ";
                    dialog.FileName = "private_key.pem";
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    _privKeyPath = dialog.FileName;
                }

                groupBox_ftp.Enabled = false;
                groupBox_nat.Enabled = false;
                groupBox_lec.Enabled = false;
                выбратьСертификатToolStripMenuItem.Checked = true;
            }
            else
            {
                groupBox_ftp.Enabled = true;
                groupBox_nat.Enabled = true;
                groupBox_lec.Enabled = true;
                выбратьСертификатToolStripMenuItem.Checked = false;
            }
        }

        private void ComboBox_lec_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectedAddress = comboBox_lec.SelectedIndex;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MtApiSsl = checkBox_ssl.Checked;
        }
    }
}
