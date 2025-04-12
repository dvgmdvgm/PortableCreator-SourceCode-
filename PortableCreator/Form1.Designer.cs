namespace PortableCreator
{
	public partial class Form1 : System.Windows.Forms.Form
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.createBtn = new System.Windows.Forms.Button();
            this.projectName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.exePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.clearLogs = new System.Windows.Forms.CheckBox();
            this.customArgs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.blockConnection = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.drvsExport = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.englishLang = new System.Windows.Forms.PictureBox();
            this.russianLang = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.englishLang)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.russianLang)).BeginInit();
            this.SuspendLayout();
            // 
            // createBtn
            // 
            this.createBtn.Location = new System.Drawing.Point(208, 235);
            this.createBtn.Name = "createBtn";
            this.createBtn.Size = new System.Drawing.Size(162, 36);
            this.createBtn.TabIndex = 0;
            this.createBtn.Text = "Создать";
            this.createBtn.UseVisualStyleBackColor = true;
            this.createBtn.Click += new System.EventHandler(this.CreateBtn_Click);
            // 
            // projectName
            // 
            this.projectName.Location = new System.Drawing.Point(92, 13);
            this.projectName.Name = "projectName";
            this.projectName.Size = new System.Drawing.Size(255, 20);
            this.projectName.TabIndex = 1;
            this.toolTip1.SetToolTip(this.projectName, "Введите имя проекта или название приложения.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Имя проекта :";
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(95, 277);
            this.txtSource.Multiline = true;
            this.txtSource.Name = "txtSource";
            this.txtSource.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSource.Size = new System.Drawing.Size(79, 36);
            this.txtSource.TabIndex = 15;
            this.txtSource.TabStop = false;
            this.txtSource.Text = resources.GetString("txtSource.Text");
            this.txtSource.Visible = false;
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(12, 277);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtStatus.Size = new System.Drawing.Size(73, 36);
            this.txtStatus.TabIndex = 16;
            this.txtStatus.TabStop = false;
            this.txtStatus.Visible = false;
            // 
            // exePath
            // 
            this.exePath.Location = new System.Drawing.Point(92, 49);
            this.exePath.Name = "exePath";
            this.exePath.Size = new System.Drawing.Size(255, 20);
            this.exePath.TabIndex = 2;
            this.toolTip1.SetToolTip(this.exePath, "Полный путь к исполняемому файлу приложения.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Путь к *.exe :";
            // 
            // clearLogs
            // 
            this.clearLogs.AutoSize = true;
            this.clearLogs.Location = new System.Drawing.Point(12, 86);
            this.clearLogs.Name = "clearLogs";
            this.clearLogs.Size = new System.Drawing.Size(214, 17);
            this.clearLogs.TabIndex = 3;
            this.clearLogs.Text = "Очищать папку логов после работы?";
            this.toolTip1.SetToolTip(this.clearLogs, "Будут удалены папки \"C:\\Users\\Имя\\AppData\\Local\\Microsoft\\CLR*\".");
            this.clearLogs.UseVisualStyleBackColor = true;
            // 
            // customArgs
            // 
            this.customArgs.Location = new System.Drawing.Point(92, 149);
            this.customArgs.Name = "customArgs";
            this.customArgs.Size = new System.Drawing.Size(255, 20);
            this.customArgs.TabIndex = 4;
            this.toolTip1.SetToolTip(this.customArgs, "Параметры командной строки для запуска приложения, если нужно.");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Аргументы :";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.blockConnection);
            this.panel1.Controls.Add(this.customArgs);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.projectName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.clearLogs);
            this.panel1.Controls.Add(this.exePath);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(12, 35);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(358, 184);
            this.panel1.TabIndex = 10;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(287, 80);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(60, 60);
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            // 
            // blockConnection
            // 
            this.blockConnection.AutoSize = true;
            this.blockConnection.Location = new System.Drawing.Point(12, 119);
            this.blockConnection.Name = "blockConnection";
            this.blockConnection.Size = new System.Drawing.Size(235, 17);
            this.blockConnection.TabIndex = 15;
            this.blockConnection.Text = "Запретить портативке доступ в интернет";
            this.toolTip1.SetToolTip(this.blockConnection, "Твоя программа не будет проверять обновления и лицензию через интернет.\r\n(Все сет" +
        "евые функции приложения будут заблокированы, однако Ты по прежнему можешь исполь" +
        "зовать локальную сеть)");
            this.blockConnection.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Параметры загрузчика:";
            // 
            // drvsExport
            // 
            this.drvsExport.Location = new System.Drawing.Point(12, 235);
            this.drvsExport.Name = "drvsExport";
            this.drvsExport.Size = new System.Drawing.Size(162, 36);
            this.drvsExport.TabIndex = 5;
            this.drvsExport.Text = "Экспорт драйверов";
            this.drvsExport.UseVisualStyleBackColor = true;
            this.drvsExport.Click += new System.EventHandler(this.DrvsExport_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // englishLang
            // 
            this.englishLang.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.englishLang.Cursor = System.Windows.Forms.Cursors.Hand;
            this.englishLang.Location = new System.Drawing.Point(347, 11);
            this.englishLang.Name = "englishLang";
            this.englishLang.Size = new System.Drawing.Size(23, 16);
            this.englishLang.TabIndex = 18;
            this.englishLang.TabStop = false;
            this.englishLang.Click += new System.EventHandler(this.englishLang_Click);
            // 
            // russianLang
            // 
            this.russianLang.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.russianLang.Cursor = System.Windows.Forms.Cursors.Hand;
            this.russianLang.Location = new System.Drawing.Point(318, 11);
            this.russianLang.Name = "russianLang";
            this.russianLang.Size = new System.Drawing.Size(23, 16);
            this.russianLang.TabIndex = 17;
            this.russianLang.TabStop = false;
            this.russianLang.Click += new System.EventHandler(this.russianLang_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 283);
            this.Controls.Add(this.englishLang);
            this.Controls.Add(this.russianLang);
            this.Controls.Add(this.drvsExport);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.createBtn);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Opacity = 0.97D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PortableCreator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.englishLang)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.russianLang)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button createBtn;
		private System.Windows.Forms.TextBox projectName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtSource;
		private System.Windows.Forms.TextBox txtStatus;
		private System.Windows.Forms.TextBox exePath;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox clearLogs;
		private System.Windows.Forms.TextBox customArgs;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button drvsExport;
		private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox russianLang;
        private System.Windows.Forms.PictureBox englishLang;
        private System.Windows.Forms.CheckBox blockConnection;
        private System.Windows.Forms.PictureBox thispictureBox2;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
