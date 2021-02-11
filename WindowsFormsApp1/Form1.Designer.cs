namespace LetsEncryptMikroTik
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
            this.components = new System.ComponentModel.Container();
            this.button_start = new System.Windows.Forms.Button();
            this.groupBox_mt = new System.Windows.Forms.GroupBox();
            this.groupBox_nat = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_wan = new System.Windows.Forms.TextBox();
            this.groupBox_ftp = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_ftpLogin = new System.Windows.Forms.TextBox();
            this.textBox_ftpPassword = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox_ssl = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_MtAddress = new System.Windows.Forms.TextBox();
            this.textBox_mtPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_MtLogin = new System.Windows.Forms.TextBox();
            this.textBox_mtPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox_lec = new System.Windows.Forms.GroupBox();
            this.checkBox_saveFile = new System.Windows.Forms.CheckBox();
            this.checkBox_force = new System.Windows.Forms.CheckBox();
            this.comboBox_lec = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_email = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_domainName = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выбратьСертификатToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.checkBox_alpn = new System.Windows.Forms.CheckBox();
            this.groupBox_mt.SuspendLayout();
            this.groupBox_nat.SuspendLayout();
            this.groupBox_ftp.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox_lec.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_start
            // 
            this.button_start.Location = new System.Drawing.Point(507, 286);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(97, 37);
            this.button_start.TabIndex = 10;
            this.button_start.Text = "Старт";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.Button_Start_Click);
            // 
            // groupBox_mt
            // 
            this.groupBox_mt.Controls.Add(this.groupBox_nat);
            this.groupBox_mt.Controls.Add(this.groupBox_ftp);
            this.groupBox_mt.Controls.Add(this.groupBox3);
            this.groupBox_mt.Location = new System.Drawing.Point(12, 27);
            this.groupBox_mt.Name = "groupBox_mt";
            this.groupBox_mt.Size = new System.Drawing.Size(360, 296);
            this.groupBox_mt.TabIndex = 0;
            this.groupBox_mt.TabStop = false;
            this.groupBox_mt.Text = "MikroTik";
            // 
            // groupBox_nat
            // 
            this.groupBox_nat.Controls.Add(this.label9);
            this.groupBox_nat.Controls.Add(this.textBox_wan);
            this.groupBox_nat.Location = new System.Drawing.Point(16, 230);
            this.groupBox_nat.Name = "groupBox_nat";
            this.groupBox_nat.Size = new System.Drawing.Size(219, 55);
            this.groupBox_nat.TabIndex = 14;
            this.groupBox_nat.TabStop = false;
            this.groupBox_nat.Text = "NAT";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(88, 15);
            this.label9.TabIndex = 15;
            this.label9.Text = "WAN ин-фейс:";
            this.toolTip1.SetToolTip(this.label9, "Имя сетевого интерфейса в микротике");
            // 
            // textBox_wan
            // 
            this.textBox_wan.Location = new System.Drawing.Point(106, 22);
            this.textBox_wan.Name = "textBox_wan";
            this.textBox_wan.Size = new System.Drawing.Size(100, 23);
            this.textBox_wan.TabIndex = 7;
            this.textBox_wan.TextChanged += new System.EventHandler(this.TextBox1_TextChanged_1);
            // 
            // groupBox_ftp
            // 
            this.groupBox_ftp.Controls.Add(this.label2);
            this.groupBox_ftp.Controls.Add(this.label6);
            this.groupBox_ftp.Controls.Add(this.textBox_ftpLogin);
            this.groupBox_ftp.Controls.Add(this.textBox_ftpPassword);
            this.groupBox_ftp.Location = new System.Drawing.Point(16, 138);
            this.groupBox_ftp.Name = "groupBox_ftp";
            this.groupBox_ftp.Size = new System.Drawing.Size(219, 86);
            this.groupBox_ftp.TabIndex = 2;
            this.groupBox_ftp.TabStop = false;
            this.groupBox_ftp.Text = "FTP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "FTP логин:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 15);
            this.label6.TabIndex = 10;
            this.label6.Text = "FTP пароль:";
            // 
            // textBox_ftpLogin
            // 
            this.textBox_ftpLogin.Location = new System.Drawing.Point(106, 21);
            this.textBox_ftpLogin.Name = "textBox_ftpLogin";
            this.textBox_ftpLogin.Size = new System.Drawing.Size(100, 23);
            this.textBox_ftpLogin.TabIndex = 5;
            this.textBox_ftpLogin.TextChanged += new System.EventHandler(this.TextBox_ftpLogin_TextChanged);
            // 
            // textBox_ftpPassword
            // 
            this.textBox_ftpPassword.Location = new System.Drawing.Point(106, 50);
            this.textBox_ftpPassword.Name = "textBox_ftpPassword";
            this.textBox_ftpPassword.Size = new System.Drawing.Size(100, 23);
            this.textBox_ftpPassword.TabIndex = 6;
            this.textBox_ftpPassword.UseSystemPasswordChar = true;
            this.textBox_ftpPassword.TextChanged += new System.EventHandler(this.TextBox_ftpPassword_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBox_ssl);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.textBox_MtAddress);
            this.groupBox3.Controls.Add(this.textBox_mtPort);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox_MtLogin);
            this.groupBox3.Controls.Add(this.textBox_mtPassword);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(16, 22);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(330, 110);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "API";
            // 
            // checkBox_ssl
            // 
            this.checkBox_ssl.AutoSize = true;
            this.checkBox_ssl.Checked = true;
            this.checkBox_ssl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_ssl.Location = new System.Drawing.Point(274, 49);
            this.checkBox_ssl.Name = "checkBox_ssl";
            this.checkBox_ssl.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBox_ssl.Size = new System.Drawing.Size(44, 19);
            this.checkBox_ssl.TabIndex = 14;
            this.checkBox_ssl.Text = "SSL";
            this.toolTip1.SetToolTip(this.checkBox_ssl, "Использовать api-ssl");
            this.checkBox_ssl.UseVisualStyleBackColor = true;
            this.checkBox_ssl.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 15);
            this.label5.TabIndex = 9;
            this.label5.Text = "Хост:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(212, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Порт:";
            // 
            // textBox_MtAddress
            // 
            this.textBox_MtAddress.Location = new System.Drawing.Point(106, 16);
            this.textBox_MtAddress.Name = "textBox_MtAddress";
            this.textBox_MtAddress.Size = new System.Drawing.Size(100, 23);
            this.textBox_MtAddress.TabIndex = 1;
            this.textBox_MtAddress.WordWrap = false;
            this.textBox_MtAddress.TextChanged += new System.EventHandler(this.TextBox_MtAddress_TextChanged);
            this.textBox_MtAddress.Leave += new System.EventHandler(this.TextBox_MtAddress_Leave);
            // 
            // textBox_mtPort
            // 
            this.textBox_mtPort.Location = new System.Drawing.Point(256, 16);
            this.textBox_mtPort.MaxLength = 5;
            this.textBox_mtPort.Name = "textBox_mtPort";
            this.textBox_mtPort.Size = new System.Drawing.Size(62, 23);
            this.textBox_mtPort.TabIndex = 2;
            this.textBox_mtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.textBox_mtPort, "По умолчанию api: 8728, api-ssl: 8729");
            this.textBox_mtPort.WordWrap = false;
            this.textBox_mtPort.TextChanged += new System.EventHandler(this.TextBox_mtPort_TextChanged);
            this.textBox_mtPort.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_mtPort_KeyDown);
            this.textBox_mtPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_mtPort_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Логин:";
            // 
            // textBox_MtLogin
            // 
            this.textBox_MtLogin.Location = new System.Drawing.Point(106, 45);
            this.textBox_MtLogin.Name = "textBox_MtLogin";
            this.textBox_MtLogin.Size = new System.Drawing.Size(100, 23);
            this.textBox_MtLogin.TabIndex = 3;
            this.textBox_MtLogin.TextChanged += new System.EventHandler(this.TextBox_MtLogin_TextChanged);
            // 
            // textBox_mtPassword
            // 
            this.textBox_mtPassword.Location = new System.Drawing.Point(106, 74);
            this.textBox_mtPassword.Name = "textBox_mtPassword";
            this.textBox_mtPassword.Size = new System.Drawing.Size(100, 23);
            this.textBox_mtPassword.TabIndex = 4;
            this.textBox_mtPassword.UseSystemPasswordChar = true;
            this.textBox_mtPassword.TextChanged += new System.EventHandler(this.TextBox3_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Пароль:";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // groupBox_lec
            // 
            this.groupBox_lec.Controls.Add(this.checkBox_alpn);
            this.groupBox_lec.Controls.Add(this.checkBox_saveFile);
            this.groupBox_lec.Controls.Add(this.checkBox_force);
            this.groupBox_lec.Controls.Add(this.comboBox_lec);
            this.groupBox_lec.Controls.Add(this.label10);
            this.groupBox_lec.Controls.Add(this.textBox_email);
            this.groupBox_lec.Controls.Add(this.label8);
            this.groupBox_lec.Controls.Add(this.label7);
            this.groupBox_lec.Controls.Add(this.textBox_domainName);
            this.groupBox_lec.Location = new System.Drawing.Point(385, 27);
            this.groupBox_lec.Name = "groupBox_lec";
            this.groupBox_lec.Size = new System.Drawing.Size(219, 206);
            this.groupBox_lec.TabIndex = 3;
            this.groupBox_lec.TabStop = false;
            this.groupBox_lec.Text = "Let\'s Encrypt";
            // 
            // checkBox_saveFile
            // 
            this.checkBox_saveFile.AutoSize = true;
            this.checkBox_saveFile.Checked = true;
            this.checkBox_saveFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_saveFile.Location = new System.Drawing.Point(81, 125);
            this.checkBox_saveFile.Name = "checkBox_saveFile";
            this.checkBox_saveFile.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBox_saveFile.Size = new System.Drawing.Size(125, 19);
            this.checkBox_saveFile.TabIndex = 15;
            this.checkBox_saveFile.Text = "Сохранить в файл";
            this.toolTip1.SetToolTip(this.checkBox_saveFile, "Позволяет сохранить полученный сертификат и приватный ключ в файл");
            this.checkBox_saveFile.UseVisualStyleBackColor = true;
            // 
            // checkBox_force
            // 
            this.checkBox_force.AutoSize = true;
            this.checkBox_force.Location = new System.Drawing.Point(94, 150);
            this.checkBox_force.Name = "checkBox_force";
            this.checkBox_force.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBox_force.Size = new System.Drawing.Size(113, 19);
            this.checkBox_force.TabIndex = 14;
            this.checkBox_force.Text = "Принудительно";
            this.toolTip1.SetToolTip(this.checkBox_force, "Позволяет добавить в микротик новый сертификат даже если старый еще актуален");
            this.checkBox_force.UseVisualStyleBackColor = true;
            // 
            // comboBox_lec
            // 
            this.comboBox_lec.DisplayMember = "Item1";
            this.comboBox_lec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_lec.FormattingEnabled = true;
            this.comboBox_lec.Location = new System.Drawing.Point(75, 96);
            this.comboBox_lec.Name = "comboBox_lec";
            this.comboBox_lec.Size = new System.Drawing.Size(132, 23);
            this.comboBox_lec.TabIndex = 13;
            this.comboBox_lec.ValueMember = "Item2";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 99);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 15);
            this.label10.TabIndex = 11;
            this.label10.Text = "Сервер:";
            // 
            // textBox_email
            // 
            this.textBox_email.Location = new System.Drawing.Point(75, 67);
            this.textBox_email.Name = "textBox_email";
            this.textBox_email.Size = new System.Drawing.Size(132, 23);
            this.textBox_email.TabIndex = 9;
            this.textBox_email.TextChanged += new System.EventHandler(this.TextBox_email_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 71);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 15);
            this.label8.TabIndex = 8;
            this.label8.Text = "Email:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 41);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "Домен:";
            // 
            // textBox_domainName
            // 
            this.textBox_domainName.Location = new System.Drawing.Point(75, 38);
            this.textBox_domainName.Name = "textBox_domainName";
            this.textBox_domainName.Size = new System.Drawing.Size(132, 23);
            this.textBox_domainName.TabIndex = 8;
            this.textBox_domainName.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Location = new System.Drawing.Point(1, 334);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(809, 315);
            this.richTextBox1.TabIndex = 11;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(810, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.выбратьСертификатToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // выбратьСертификатToolStripMenuItem
            // 
            this.выбратьСертификатToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.выбратьСертификатToolStripMenuItem.Name = "выбратьСертификатToolStripMenuItem";
            this.выбратьСертификатToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.выбратьСертификатToolStripMenuItem.Text = "Выбрать сертификат";
            this.выбратьСертификатToolStripMenuItem.Click += new System.EventHandler(this.ВыбратьСертификатToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(404, 286);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 37);
            this.button1.TabIndex = 13;
            this.button1.Text = "Открыть папку";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click_1);
            // 
            // checkBox_alpn
            // 
            this.checkBox_alpn.AutoSize = true;
            this.checkBox_alpn.Checked = true;
            this.checkBox_alpn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_alpn.Location = new System.Drawing.Point(150, 175);
            this.checkBox_alpn.Name = "checkBox_alpn";
            this.checkBox_alpn.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBox_alpn.Size = new System.Drawing.Size(56, 19);
            this.checkBox_alpn.TabIndex = 16;
            this.checkBox_alpn.Text = "ALPN";
            this.toolTip1.SetToolTip(this.checkBox_alpn, "Позволяет добавить в микротик новый сертификат даже если старый еще актуален");
            this.checkBox_alpn.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 650);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.groupBox_lec);
            this.Controls.Add(this.groupBox_mt);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(625, 455);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MikroTik";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBox_mt.ResumeLayout(false);
            this.groupBox_nat.ResumeLayout(false);
            this.groupBox_nat.PerformLayout();
            this.groupBox_ftp.ResumeLayout(false);
            this.groupBox_ftp.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox_lec.ResumeLayout(false);
            this.groupBox_lec.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.CheckBox checkBox_ssl;
        private System.Windows.Forms.CheckBox checkBox_saveFile;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBox_alpn;
    }
}

