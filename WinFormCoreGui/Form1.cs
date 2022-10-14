using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LetsEncryptMikroTik;

public partial class Form1 : Form
{
    // Create byte array for additional entropy when using Protect method.
    private static readonly byte[] s_aditionalEntropy = { 41, 23, 98, 128, 246, 100, 21, 24 };
    private readonly Action<string> _onLogMessageHandler;

    //[DllImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //private static extern bool AllocConsole();

    //[DllImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //private static extern bool FreeConsole();

    //[DllImport("kernel32.dll")]
    //static extern bool AttachConsole(int dwProcessId);
    //private const int ATTACH_PARENT_PROCESS = -1;

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
        groupBox_ftp.Enabled = checkBox1.Checked;

        comboBox_lec.DataSource = Core.Program.GetAddresses();
        var selIndex = Properties.Settings.Default.SelectedAddress;
        if (comboBox_lec.Items.Count - 1 >= selIndex)
        {
            comboBox_lec.SelectedIndex = Properties.Settings.Default.SelectedAddress;
        }
        comboBox_lec.SelectedIndexChanged += ComboBox_lec_SelectedIndexChanged;

        _onLogMessageHandler = OnLogMessage;
    }

    private async void Button_Start_Click(object sender, EventArgs e)
    {
        var mtLogin = textBox_MtLogin.Text.Trim();
        var mtPassword = textBox_mtPassword.Text;

        string ftpLogin;
        string ftpPassword;
        if (checkBox1.Checked)
        {
            ftpLogin = textBox_ftpLogin.Text.Trim();
            ftpPassword = textBox_ftpPassword.Text;
        }
        else
        {
            ftpLogin = mtLogin;
            ftpPassword = mtPassword;
        }

        var config = new Core.ConfigClass
        {
            MikroTikAddress = textBox_MtAddress.Text.Trim(),
            MikroTikPort = int.Parse(textBox_mtPort.Text),
            MikroTikLogin = mtLogin,
            MikroTikPassword = mtPassword,
            FtpLogin = ftpLogin,
            FtpPassword = ftpPassword,
            DomainName = textBox_domainName.Text.Trim(),
            Email = textBox_email.Text.Trim(),
            WanIface = textBox_wan.Text.Trim(),
            Force = checkBox_force.Checked,
            LetsEncryptAddress = (Core.LeUri)(Uri)comboBox_lec.SelectedValue,
            SaveFile = checkBox_saveFile.Checked,
            UseAlpn = radioButton_alpn.Checked,
        };

        var program = new Core.Program(config);

        //AllocConsole();
        //AttachConsole(ATTACH_PARENT_PROCESS);

        button_start.Enabled = false;
        groupBox_mt.Enabled = false;
        groupBox_ftp.Enabled = false;
        textBox_wan.Enabled = false;
        groupBox_lec.Enabled = false;
        richTextBox1.Clear();
        try
        {
            await Task.Run(() => program.RunAsync(logToFile: true, logSink: new InMemorySinkLog(this)));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            button_start.Enabled = true;
            groupBox_mt.Enabled = true;
            groupBox_ftp.Enabled = true;
            textBox_wan.Enabled = true;
            groupBox_lec.Enabled = true;
            //FreeConsole();
        }
    }

    internal void OnLogMessage(string message)
    {
        if (richTextBox1.InvokeRequired)
        {
            richTextBox1.BeginInvoke(_onLogMessageHandler, message);
        }
        else
        {
            richTextBox1.AppendText(message);
        }
    }

    private static string Protect(string s)
    {
#if NET461
        throw new NotImplementedException();
#else
        try
        {
            // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
            // only by the same current user.
            var data = Encoding.UTF8.GetBytes(s);
            var @protected = ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(@protected);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not encrypted. An error occurred.");
            Console.WriteLine(e.ToString());
            return null;
        }
#endif
    }

    private static string Unprotect(string dataBase64)
    {
#if NET461
        throw new NotImplementedException();
#else
        if (string.IsNullOrEmpty(dataBase64))
            return null;

        try
        {
            var data = Convert.FromBase64String(dataBase64);
            //Decrypt the data using DataProtectionScope.CurrentUser.
            var unprotected = ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(unprotected);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not decrypted. An error occurred.");
            Console.WriteLine(e.ToString());
            return null;
        }
#endif
    }

    private void TextBox1_Leave(object sender, EventArgs e)
    {

    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
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
        Properties.Settings.Default.Save();
    }

    private void Button1_Click_1(object sender, EventArgs e)
    {
        var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cert");

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
        if (!выбратьСертификатToolStripMenuItem.Checked)
        {
            using (var dialog = new OpenFileDialog())
            {
                var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cert");
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

    private void textBox_MtLogin_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void textBox_mtPassword_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private static void SaveProperties()
    {
        Properties.Settings.Default.Save();
    }

    private void textBox_ftpLogin_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        groupBox_ftp.Enabled = checkBox1.Checked;
    }

    private void textBox_mtPort_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void textBox_ftpPassword_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void textBox_wan_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void textBox_domainName_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void textBox_email_Leave(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void comboBox_lec_SelectedIndexChanged_1(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void checkBox_saveFile_CheckedChanged(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void checkBox_force_CheckedChanged(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void radioButton_http_CheckedChanged(object sender, EventArgs e)
    {
        SaveProperties();
    }

    private void radioButton_alpn_CheckedChanged(object sender, EventArgs e)
    {
        SaveProperties();
    }
}
