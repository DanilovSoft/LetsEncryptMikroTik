namespace LetsEncryptMikroTik.WinForm
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            button_start = new System.Windows.Forms.Button();
            groupBox_mt = new System.Windows.Forms.GroupBox();
            checkBox_ftpCredentials = new System.Windows.Forms.CheckBox();
            groupBox_nat = new System.Windows.Forms.GroupBox();
            label9 = new System.Windows.Forms.Label();
            textBox_wan = new System.Windows.Forms.TextBox();
            groupBox_ftp = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            textBox_ftpLogin = new System.Windows.Forms.TextBox();
            textBox_ftpPassword = new System.Windows.Forms.TextBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            textBox_MtAddress = new System.Windows.Forms.TextBox();
            textBox_mtPort = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            textBox_MtLogin = new System.Windows.Forms.TextBox();
            textBox_mtPassword = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            groupBox_lec = new System.Windows.Forms.GroupBox();
            radioButton_alpn = new System.Windows.Forms.RadioButton();
            radioButton_http = new System.Windows.Forms.RadioButton();
            checkBox_saveFile = new System.Windows.Forms.CheckBox();
            checkBox_force = new System.Windows.Forms.CheckBox();
            comboBox_lec = new System.Windows.Forms.ComboBox();
            label10 = new System.Windows.Forms.Label();
            textBox_email = new System.Windows.Forms.TextBox();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            textBox_domainName = new System.Windows.Forms.TextBox();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            выбратьСертификатToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            button1 = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            radioButton_dns01 = new System.Windows.Forms.RadioButton();
            groupBox_mt.SuspendLayout();
            groupBox_nat.SuspendLayout();
            groupBox_ftp.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox_lec.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // button_start
            // 
            button_start.Location = new System.Drawing.Point(507, 304);
            button_start.Name = "button_start";
            button_start.Size = new System.Drawing.Size(97, 37);
            button_start.TabIndex = 10;
            button_start.Text = "Старт";
            button_start.UseVisualStyleBackColor = true;
            button_start.Click += Button_Start_Click;
            // 
            // groupBox_mt
            // 
            groupBox_mt.Controls.Add(checkBox_ftpCredentials);
            groupBox_mt.Controls.Add(groupBox_nat);
            groupBox_mt.Controls.Add(groupBox_ftp);
            groupBox_mt.Controls.Add(groupBox3);
            groupBox_mt.Location = new System.Drawing.Point(12, 27);
            groupBox_mt.Name = "groupBox_mt";
            groupBox_mt.Size = new System.Drawing.Size(360, 324);
            groupBox_mt.TabIndex = 0;
            groupBox_mt.TabStop = false;
            groupBox_mt.Text = "MikroTik";
            // 
            // checkBox1
            // 
            checkBox_ftpCredentials.AutoSize = true;
            checkBox_ftpCredentials.Location = new System.Drawing.Point(16, 138);
            checkBox_ftpCredentials.Name = "checkBox1";
            checkBox_ftpCredentials.Size = new System.Drawing.Size(185, 19);
            checkBox_ftpCredentials.TabIndex = 15;
            checkBox_ftpCredentials.Text = "Отдельный профиль для FTP";
            checkBox_ftpCredentials.UseVisualStyleBackColor = true;
            checkBox_ftpCredentials.CheckedChanged += CheckBox1_CheckedChanged;
            // 
            // groupBox_nat
            // 
            groupBox_nat.Controls.Add(label9);
            groupBox_nat.Controls.Add(textBox_wan);
            groupBox_nat.Location = new System.Drawing.Point(16, 255);
            groupBox_nat.Name = "groupBox_nat";
            groupBox_nat.Size = new System.Drawing.Size(219, 55);
            groupBox_nat.TabIndex = 14;
            groupBox_nat.TabStop = false;
            groupBox_nat.Text = "NAT";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(12, 25);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(88, 15);
            label9.TabIndex = 15;
            label9.Text = "WAN ин-фейс:";
            toolTip1.SetToolTip(label9, "Имя сетевого интерфейса в микротике");
            // 
            // textBox_wan
            // 
            textBox_wan.Location = new System.Drawing.Point(106, 22);
            textBox_wan.Name = "textBox_wan";
            textBox_wan.Size = new System.Drawing.Size(100, 23);
            textBox_wan.TabIndex = 7;
            textBox_wan.TextChanged += TextBox1_TextChanged_1;
            textBox_wan.Leave += TextBox_wan_Leave;
            // 
            // groupBox_ftp
            // 
            groupBox_ftp.Controls.Add(label2);
            groupBox_ftp.Controls.Add(label6);
            groupBox_ftp.Controls.Add(textBox_ftpLogin);
            groupBox_ftp.Controls.Add(textBox_ftpPassword);
            groupBox_ftp.Location = new System.Drawing.Point(16, 163);
            groupBox_ftp.Name = "groupBox_ftp";
            groupBox_ftp.Size = new System.Drawing.Size(219, 86);
            groupBox_ftp.TabIndex = 2;
            groupBox_ftp.TabStop = false;
            groupBox_ftp.Text = "FTP";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 24);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(65, 15);
            label2.TabIndex = 4;
            label2.Text = "FTP логин:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(12, 53);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(72, 15);
            label6.TabIndex = 10;
            label6.Text = "FTP пароль:";
            // 
            // textBox_ftpLogin
            // 
            textBox_ftpLogin.Location = new System.Drawing.Point(106, 21);
            textBox_ftpLogin.Name = "textBox_ftpLogin";
            textBox_ftpLogin.Size = new System.Drawing.Size(100, 23);
            textBox_ftpLogin.TabIndex = 5;
            textBox_ftpLogin.TextChanged += TextBox_ftpLogin_TextChanged;
            textBox_ftpLogin.Leave += TextBox_ftpLogin_Leave;
            // 
            // textBox_ftpPassword
            // 
            textBox_ftpPassword.Location = new System.Drawing.Point(106, 50);
            textBox_ftpPassword.Name = "textBox_ftpPassword";
            textBox_ftpPassword.Size = new System.Drawing.Size(100, 23);
            textBox_ftpPassword.TabIndex = 6;
            textBox_ftpPassword.UseSystemPasswordChar = true;
            textBox_ftpPassword.TextChanged += TextBox_ftpPassword_TextChanged;
            textBox_ftpPassword.Leave += TextBox_ftpPassword_Leave;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(label1);
            groupBox3.Controls.Add(textBox_MtAddress);
            groupBox3.Controls.Add(textBox_mtPort);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(textBox_MtLogin);
            groupBox3.Controls.Add(textBox_mtPassword);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new System.Drawing.Point(16, 22);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(330, 110);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "API";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(12, 19);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(35, 15);
            label5.TabIndex = 9;
            label5.Text = "Хост:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(212, 19);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 15);
            label1.TabIndex = 3;
            label1.Text = "Порт:";
            // 
            // textBox_MtAddress
            // 
            textBox_MtAddress.Location = new System.Drawing.Point(106, 16);
            textBox_MtAddress.Name = "textBox_MtAddress";
            textBox_MtAddress.Size = new System.Drawing.Size(100, 23);
            textBox_MtAddress.TabIndex = 1;
            textBox_MtAddress.WordWrap = false;
            textBox_MtAddress.TextChanged += TextBox_MtAddress_TextChanged;
            textBox_MtAddress.Leave += TextBox_MtAddress_Leave;
            // 
            // textBox_mtPort
            // 
            textBox_mtPort.Location = new System.Drawing.Point(256, 16);
            textBox_mtPort.MaxLength = 5;
            textBox_mtPort.Name = "textBox_mtPort";
            textBox_mtPort.Size = new System.Drawing.Size(62, 23);
            textBox_mtPort.TabIndex = 2;
            textBox_mtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip1.SetToolTip(textBox_mtPort, "По умолчанию api: 8728, api-ssl: 8729");
            textBox_mtPort.WordWrap = false;
            textBox_mtPort.TextChanged += TextBox_mtPort_TextChanged;
            textBox_mtPort.KeyDown += TextBox_mtPort_KeyDown;
            textBox_mtPort.KeyPress += TextBox_mtPort_KeyPress;
            textBox_mtPort.Leave += TextBox_mtPort_Leave;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 48);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(44, 15);
            label3.TabIndex = 5;
            label3.Text = "Логин:";
            // 
            // textBox_MtLogin
            // 
            textBox_MtLogin.Location = new System.Drawing.Point(106, 45);
            textBox_MtLogin.Name = "textBox_MtLogin";
            textBox_MtLogin.Size = new System.Drawing.Size(100, 23);
            textBox_MtLogin.TabIndex = 3;
            textBox_MtLogin.TextChanged += TextBox_MtLogin_TextChanged;
            textBox_MtLogin.Leave += TextBox_MtLogin_Leave;
            // 
            // textBox_mtPassword
            // 
            textBox_mtPassword.Location = new System.Drawing.Point(106, 74);
            textBox_mtPassword.Name = "textBox_mtPassword";
            textBox_mtPassword.Size = new System.Drawing.Size(100, 23);
            textBox_mtPassword.TabIndex = 4;
            textBox_mtPassword.UseSystemPasswordChar = true;
            textBox_mtPassword.TextChanged += TextBox3_TextChanged;
            textBox_mtPassword.Leave += TextBox_mtPassword_Leave;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 77);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(52, 15);
            label4.TabIndex = 6;
            label4.Text = "Пароль:";
            // 
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;
            // 
            // groupBox_lec
            // 
            groupBox_lec.Controls.Add(radioButton_dns01);
            groupBox_lec.Controls.Add(radioButton_alpn);
            groupBox_lec.Controls.Add(radioButton_http);
            groupBox_lec.Controls.Add(checkBox_saveFile);
            groupBox_lec.Controls.Add(checkBox_force);
            groupBox_lec.Controls.Add(comboBox_lec);
            groupBox_lec.Controls.Add(label10);
            groupBox_lec.Controls.Add(textBox_email);
            groupBox_lec.Controls.Add(label8);
            groupBox_lec.Controls.Add(label7);
            groupBox_lec.Controls.Add(textBox_domainName);
            groupBox_lec.Location = new System.Drawing.Point(385, 27);
            groupBox_lec.Name = "groupBox_lec";
            groupBox_lec.Size = new System.Drawing.Size(219, 271);
            groupBox_lec.TabIndex = 3;
            groupBox_lec.TabStop = false;
            groupBox_lec.Text = "Let's Encrypt";
            // 
            // radioButton_alpn
            // 
            radioButton_alpn.AutoSize = true;
            radioButton_alpn.Checked = true;
            radioButton_alpn.Location = new System.Drawing.Point(54, 213);
            radioButton_alpn.Name = "radioButton_alpn";
            radioButton_alpn.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            radioButton_alpn.Size = new System.Drawing.Size(153, 19);
            radioButton_alpn.TabIndex = 18;
            radioButton_alpn.TabStop = true;
            radioButton_alpn.Text = "TLS-ALPN-01 (порт 443)";
            toolTip1.SetToolTip(radioButton_alpn, "Использует ALPN алгоритм валидации");
            radioButton_alpn.UseVisualStyleBackColor = true;
            radioButton_alpn.CheckedChanged += RadioButton_alpn_CheckedChanged;
            // 
            // radioButton_http
            // 
            radioButton_http.AutoSize = true;
            radioButton_http.Location = new System.Drawing.Point(85, 188);
            radioButton_http.Name = "radioButton_http";
            radioButton_http.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            radioButton_http.Size = new System.Drawing.Size(122, 19);
            radioButton_http.TabIndex = 17;
            radioButton_http.Text = "HTTP-01 (порт 80)";
            radioButton_http.UseVisualStyleBackColor = true;
            radioButton_http.CheckedChanged += RadioButton_http_CheckedChanged;
            // 
            // checkBox_saveFile
            // 
            checkBox_saveFile.AutoSize = true;
            checkBox_saveFile.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox_saveFile.Checked = true;
            checkBox_saveFile.Location = new System.Drawing.Point(81, 125);
            checkBox_saveFile.Name = "checkBox_saveFile";
            checkBox_saveFile.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            checkBox_saveFile.Size = new System.Drawing.Size(126, 19);
            checkBox_saveFile.TabIndex = 15;
            checkBox_saveFile.Text = "Сохранить в файл";
            toolTip1.SetToolTip(checkBox_saveFile, "Позволяет сохранить полученный сертификат и приватный ключ в файл");
            checkBox_saveFile.UseVisualStyleBackColor = true;
            checkBox_saveFile.CheckedChanged += CheckBox_saveFile_CheckedChanged;
            // 
            // checkBox_force
            // 
            checkBox_force.AutoSize = true;
            checkBox_force.Location = new System.Drawing.Point(94, 150);
            checkBox_force.Name = "checkBox_force";
            checkBox_force.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            checkBox_force.Size = new System.Drawing.Size(113, 19);
            checkBox_force.TabIndex = 14;
            checkBox_force.Text = "Принудительно";
            toolTip1.SetToolTip(checkBox_force, "Позволяет добавить в микротик новый сертификат даже если старый еще актуален");
            checkBox_force.UseVisualStyleBackColor = true;
            checkBox_force.CheckedChanged += CheckBox_force_CheckedChanged;
            // 
            // comboBox_lec
            // 
            comboBox_lec.DisplayMember = "Item1";
            comboBox_lec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox_lec.FormattingEnabled = true;
            comboBox_lec.Location = new System.Drawing.Point(75, 96);
            comboBox_lec.Name = "comboBox_lec";
            comboBox_lec.Size = new System.Drawing.Size(132, 23);
            comboBox_lec.TabIndex = 13;
            comboBox_lec.ValueMember = "Item2";
            comboBox_lec.SelectedIndexChanged += ComboBox_lec_SelectedIndexChanged_1;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(10, 99);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(50, 15);
            label10.TabIndex = 11;
            label10.Text = "Сервер:";
            // 
            // textBox_email
            // 
            textBox_email.Location = new System.Drawing.Point(75, 67);
            textBox_email.Name = "textBox_email";
            textBox_email.Size = new System.Drawing.Size(132, 23);
            textBox_email.TabIndex = 9;
            textBox_email.TextChanged += TextBox_email_TextChanged;
            textBox_email.Leave += TextBox_email_Leave;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(10, 71);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(39, 15);
            label8.TabIndex = 8;
            label8.Text = "Email:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(10, 41);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(47, 15);
            label7.TabIndex = 7;
            label7.Text = "Домен:";
            // 
            // textBox_domainName
            // 
            textBox_domainName.Location = new System.Drawing.Point(75, 38);
            textBox_domainName.Name = "textBox_domainName";
            textBox_domainName.Size = new System.Drawing.Size(132, 23);
            textBox_domainName.TabIndex = 8;
            textBox_domainName.TextChanged += TextBox1_TextChanged;
            textBox_domainName.Leave += TextBox_domainName_Leave;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            richTextBox1.DetectUrls = false;
            richTextBox1.Location = new System.Drawing.Point(1, 357);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new System.Drawing.Size(809, 292);
            richTextBox1.TabIndex = 11;
            richTextBox1.Text = "";
            richTextBox1.WordWrap = false;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { файлToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(810, 24);
            menuStrip1.TabIndex = 12;
            menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { выбратьСертификатToolStripMenuItem });
            файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            файлToolStripMenuItem.Text = "Файл";
            // 
            // выбратьСертификатToolStripMenuItem
            // 
            выбратьСертификатToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            выбратьСертификатToolStripMenuItem.Name = "выбратьСертификатToolStripMenuItem";
            выбратьСертификатToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            выбратьСертификатToolStripMenuItem.Text = "Выбрать сертификат";
            выбратьСертификатToolStripMenuItem.Click += ВыбратьСертификатToolStripMenuItem_Click;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(404, 304);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(97, 37);
            button1.TabIndex = 13;
            button1.Text = "Открыть папку";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click_1;
            // 
            // radioButton1
            // 
            radioButton_dns01.AutoSize = true;
            radioButton_dns01.Location = new System.Drawing.Point(90, 238);
            radioButton_dns01.Name = "radioButton1";
            radioButton_dns01.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            radioButton_dns01.Size = new System.Drawing.Size(117, 19);
            radioButton_dns01.TabIndex = 19;
            radioButton_dns01.Text = "DNS-01 (порт 53)";
            radioButton_dns01.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(810, 650);
            Controls.Add(button1);
            Controls.Add(richTextBox1);
            Controls.Add(groupBox_lec);
            Controls.Add(groupBox_mt);
            Controls.Add(button_start);
            Controls.Add(menuStrip1);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            MainMenuStrip = menuStrip1;
            MinimumSize = new System.Drawing.Size(625, 455);
            Name = "Form1";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "MikroTik";
            FormClosing += Form1_FormClosing;
            groupBox_mt.ResumeLayout(false);
            groupBox_mt.PerformLayout();
            groupBox_nat.ResumeLayout(false);
            groupBox_nat.PerformLayout();
            groupBox_ftp.ResumeLayout(false);
            groupBox_ftp.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox_lec.ResumeLayout(false);
            groupBox_lec.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.GroupBox groupBox_mt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_mtPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox_ftpPassword;
        private System.Windows.Forms.TextBox textBox_ftpLogin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox_lec;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_domainName;
        private System.Windows.Forms.TextBox textBox_email;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_wan;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_MtAddress;
        private System.Windows.Forms.TextBox textBox_mtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_MtLogin;
        private System.Windows.Forms.GroupBox groupBox_ftp;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBox_lec;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBox_force;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox_nat;
        private System.Windows.Forms.ToolStripMenuItem выбратьСертификатToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox_saveFile;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.RadioButton radioButton_alpn;
        private System.Windows.Forms.RadioButton radioButton_http;
        private System.Windows.Forms.CheckBox checkBox_ftpCredentials;
        private System.Windows.Forms.RadioButton radioButton_dns01;
    }
}

