namespace FNFBot20
{
    partial class Form1
    {
        System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.treSngSelect = new System.Windows.Forms.TreeView();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnResetOffset = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.lblVer = new System.Windows.Forms.Label();
            this.txtbxDir = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btnManual = new System.Windows.Forms.Button();
            this.pnlPlayField = new System.Windows.Forms.Panel();
            this.rchConsole = new System.Windows.Forms.RichTextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.grpKeybinds = new System.Windows.Forms.GroupBox();
            this.btnBindOffsetDown = new System.Windows.Forms.Button();
            this.btnBindOffsetUp = new System.Windows.Forms.Button();
            this.btnBindPlay = new System.Windows.Forms.Button();
            this.lblOffsetDownKey = new System.Windows.Forms.Label();
            this.lblOffsetUpKey = new System.Windows.Forms.Label();
            this.lblPlayKey = new System.Windows.Forms.Label();
            this.pnlTop.SuspendLayout();
            this.grpKeybinds.SuspendLayout();
            this.SuspendLayout();
            // 
            // treSngSelect
            // 
            this.treSngSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treSngSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(15)))), ((int)(((byte)(64)))));
            this.treSngSelect.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treSngSelect.ForeColor = System.Drawing.Color.White;
            this.treSngSelect.Location = new System.Drawing.Point(0, 353);
            this.treSngSelect.Name = "treSngSelect";
            this.treSngSelect.Size = new System.Drawing.Size(697, 142);
            this.treSngSelect.TabIndex = 0;
            this.treSngSelect.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treSngSelect_NodeMouseDoubleClick);
            // 
            // pnlLogo
            // 
            this.pnlLogo.BackgroundImage = global::FNFBot20.Properties.Resources.FNFBotLogo;
            this.pnlLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlLogo.Location = new System.Drawing.Point(3, 6);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Size = new System.Drawing.Size(173, 43);
            this.pnlLogo.TabIndex = 1;
            this.pnlLogo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlLogo_MouseDown);
            // 
            // pnlTop
            // 
            this.pnlTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.pnlTop.Controls.Add(this.btnResetOffset);
            this.pnlTop.Controls.Add(this.label2);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.button1);
            this.pnlTop.Controls.Add(this.lblVer);
            this.pnlTop.Controls.Add(this.pnlLogo);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(697, 52);
            this.pnlTop.TabIndex = 2;
            this.pnlTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTop_MouseDown);
            // 
            // btnResetOffset
            // 
            this.btnResetOffset.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnResetOffset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.btnResetOffset.FlatAppearance.BorderSize = 0;
            this.btnResetOffset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetOffset.ForeColor = System.Drawing.Color.White;
            this.btnResetOffset.Location = new System.Drawing.Point(358, 6);
            this.btnResetOffset.Name = "btnResetOffset";
            this.btnResetOffset.Size = new System.Drawing.Size(34, 21);
            this.btnResetOffset.TabIndex = 6;
            this.btnResetOffset.Text = "R";
            this.btnResetOffset.UseVisualStyleBackColor = false;
            this.btnResetOffset.Click += new System.EventHandler(this.btnResetOffset_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 12F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(400, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(194, 21);
            this.label2.TabIndex = 5;
            this.label2.Text = "Offset: 25";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 12F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(400, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "Time: 00:00:000";
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label1_MouseDown);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(77)))), ((int)(((byte)(75)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(655, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(34, 34);
            this.button1.TabIndex = 3;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblVer
            // 
            this.lblVer.Font = new System.Drawing.Font("Arial", 24F);
            this.lblVer.ForeColor = System.Drawing.Color.White;
            this.lblVer.Location = new System.Drawing.Point(177, 6);
            this.lblVer.Name = "lblVer";
            this.lblVer.Size = new System.Drawing.Size(170, 43);
            this.lblVer.TabIndex = 2;
            this.lblVer.Text = "Reworked";
            this.lblVer.Click += new System.EventHandler(this.lblVer_Click);
            this.lblVer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblVer_MouseDown);
            // 
            // txtbxDir
            // 
            this.txtbxDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtbxDir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.txtbxDir.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtbxDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtbxDir.ForeColor = System.Drawing.Color.White;
            this.txtbxDir.Location = new System.Drawing.Point(3, 331);
            this.txtbxDir.Name = "txtbxDir";
            this.txtbxDir.Size = new System.Drawing.Size(420, 16);
            this.txtbxDir.TabIndex = 3;
            this.txtbxDir.Text = "FNF Game Directory (ex: C:/Users/user/Documents/FNF)";
            this.txtbxDir.Enter += new System.EventHandler(this.txtbxDir_Enter);
            this.txtbxDir.Leave += new System.EventHandler(this.txtbxDir_Leave);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(429, 331);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 16);
            this.button2.TabIndex = 4;
            this.button2.Text = "Check Dir";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnManual
            // 
            this.btnManual.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.btnManual.FlatAppearance.BorderSize = 0;
            this.btnManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.btnManual.ForeColor = System.Drawing.Color.White;
            this.btnManual.Location = new System.Drawing.Point(551, 331);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(141, 16);
            this.btnManual.TabIndex = 9;
            this.btnManual.Text = "Manual";
            this.btnManual.UseVisualStyleBackColor = false;
            this.btnManual.Click += new System.EventHandler(this.btnManual_Click);
            // 
            // pnlPlayField
            // 
            this.pnlPlayField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlPlayField.Location = new System.Drawing.Point(0, 70);
            this.pnlPlayField.Name = "pnlPlayField";
            this.pnlPlayField.Size = new System.Drawing.Size(250, 255);
            this.pnlPlayField.TabIndex = 5;
            // 
            // rchConsole
            // 
            this.rchConsole.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rchConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(15)))), ((int)(((byte)(64)))));
            this.rchConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rchConsole.ForeColor = System.Drawing.Color.White;
            this.rchConsole.Location = new System.Drawing.Point(256, 52);
            this.rchConsole.Name = "rchConsole";
            this.rchConsole.ReadOnly = true;
            this.rchConsole.Size = new System.Drawing.Size(441, 212);
            this.rchConsole.TabIndex = 6;
            this.rchConsole.Text = "";
            // 
            // checkBox1
            // 
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(3, 52);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(120, 17);
            this.checkBox1.TabIndex = 7;
            this.checkBox1.Text = "Render Notes";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // grpKeybinds
            // 
            this.grpKeybinds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpKeybinds.Controls.Add(this.btnBindOffsetDown);
            this.grpKeybinds.Controls.Add(this.btnBindOffsetUp);
            this.grpKeybinds.Controls.Add(this.btnBindPlay);
            this.grpKeybinds.Controls.Add(this.lblOffsetDownKey);
            this.grpKeybinds.Controls.Add(this.lblOffsetUpKey);
            this.grpKeybinds.Controls.Add(this.lblPlayKey);
            this.grpKeybinds.ForeColor = System.Drawing.Color.White;
            this.grpKeybinds.Location = new System.Drawing.Point(256, 257);
            this.grpKeybinds.Name = "grpKeybinds";
            this.grpKeybinds.Size = new System.Drawing.Size(441, 68);
            this.grpKeybinds.TabIndex = 8;
            this.grpKeybinds.TabStop = false;
            this.grpKeybinds.Text = "Keybinds";
            // 
            // btnBindOffsetDown
            // 
            this.btnBindOffsetDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBindOffsetDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.btnBindOffsetDown.FlatAppearance.BorderSize = 0;
            this.btnBindOffsetDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBindOffsetDown.ForeColor = System.Drawing.Color.White;
            this.btnBindOffsetDown.Location = new System.Drawing.Point(327, 40);
            this.btnBindOffsetDown.Name = "btnBindOffsetDown";
            this.btnBindOffsetDown.Size = new System.Drawing.Size(100, 23);
            this.btnBindOffsetDown.TabIndex = 5;
            this.btnBindOffsetDown.Text = "Bind";
            this.btnBindOffsetDown.UseVisualStyleBackColor = false;
            this.btnBindOffsetDown.Click += new System.EventHandler(this.btnBindOffsetDown_Click);
            // 
            // btnBindOffsetUp
            // 
            this.btnBindOffsetUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBindOffsetUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.btnBindOffsetUp.FlatAppearance.BorderSize = 0;
            this.btnBindOffsetUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBindOffsetUp.ForeColor = System.Drawing.Color.White;
            this.btnBindOffsetUp.Location = new System.Drawing.Point(327, 24);
            this.btnBindOffsetUp.Name = "btnBindOffsetUp";
            this.btnBindOffsetUp.Size = new System.Drawing.Size(100, 23);
            this.btnBindOffsetUp.TabIndex = 4;
            this.btnBindOffsetUp.Text = "Bind";
            this.btnBindOffsetUp.UseVisualStyleBackColor = false;
            this.btnBindOffsetUp.Click += new System.EventHandler(this.btnBindOffsetUp_Click);
            // 
            // btnBindPlay
            // 
            this.btnBindPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBindPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(107)))));
            this.btnBindPlay.FlatAppearance.BorderSize = 0;
            this.btnBindPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBindPlay.ForeColor = System.Drawing.Color.White;
            this.btnBindPlay.Location = new System.Drawing.Point(327, 8);
            this.btnBindPlay.Name = "btnBindPlay";
            this.btnBindPlay.Size = new System.Drawing.Size(100, 23);
            this.btnBindPlay.TabIndex = 3;
            this.btnBindPlay.Text = "Bind";
            this.btnBindPlay.UseVisualStyleBackColor = false;
            this.btnBindPlay.Click += new System.EventHandler(this.btnBindPlay_Click);
            // 
            // lblOffsetDownKey
            // 
            this.lblOffsetDownKey.Location = new System.Drawing.Point(12, 44);
            this.lblOffsetDownKey.Name = "lblOffsetDownKey";
            this.lblOffsetDownKey.Size = new System.Drawing.Size(300, 16);
            this.lblOffsetDownKey.TabIndex = 2;
            this.lblOffsetDownKey.Text = "Offset-: F3";
            // 
            // lblOffsetUpKey
            // 
            this.lblOffsetUpKey.Location = new System.Drawing.Point(12, 28);
            this.lblOffsetUpKey.Name = "lblOffsetUpKey";
            this.lblOffsetUpKey.Size = new System.Drawing.Size(300, 16);
            this.lblOffsetUpKey.TabIndex = 1;
            this.lblOffsetUpKey.Text = "Offset+: F2";
            // 
            // lblPlayKey
            // 
            this.lblPlayKey.Location = new System.Drawing.Point(12, 12);
            this.lblPlayKey.Name = "lblPlayKey";
            this.lblPlayKey.Size = new System.Drawing.Size(300, 16);
            this.lblPlayKey.TabIndex = 0;
            this.lblPlayKey.Text = "Play: F1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(15)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(697, 495);
            this.Controls.Add(this.grpKeybinds);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.rchConsole);
            this.Controls.Add(this.pnlPlayField);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtbxDir);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.treSngSelect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "Form1";
            this.Text = "FNFBot";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.pnlTop.ResumeLayout(false);
            this.grpKeybinds.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        System.Windows.Forms.Button btnResetOffset;
        System.Windows.Forms.Button btnManual;
        System.Windows.Forms.GroupBox grpKeybinds;
        System.Windows.Forms.Button btnBindOffsetDown;
        System.Windows.Forms.Button btnBindOffsetUp;
        System.Windows.Forms.Button btnBindPlay;
        System.Windows.Forms.Label lblOffsetDownKey;
        System.Windows.Forms.Label lblOffsetUpKey;
        System.Windows.Forms.Label lblPlayKey;
        System.Windows.Forms.Label label2;
        System.Windows.Forms.CheckBox checkBox1;
        System.Windows.Forms.Label label1;
        System.Windows.Forms.RichTextBox rchConsole;
        System.Windows.Forms.Panel pnlPlayField;
        System.Windows.Forms.Button button2;
        System.Windows.Forms.TextBox txtbxDir;
        System.Windows.Forms.Button button1;
        System.Windows.Forms.Label lblVer;
        System.Windows.Forms.Panel pnlTop;
        System.Windows.Forms.Panel pnlLogo;
        System.Windows.Forms.TreeView treSngSelect;
    }
}
