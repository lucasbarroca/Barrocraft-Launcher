
namespace MineLauncher
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.chkForceUpdate = new System.Windows.Forms.CheckBox();
            this.cmbLaunchers = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lblOptions = new System.Windows.Forms.Label();
            this.cmbOptions = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btPlay = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.wbNews = new System.Windows.Forms.WebBrowser();
            this.pbStatus = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.oFileLauncherExe = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkForceUpdate
            // 
            this.chkForceUpdate.AutoSize = true;
            this.chkForceUpdate.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkForceUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkForceUpdate.Location = new System.Drawing.Point(3, 31);
            this.chkForceUpdate.Name = "chkForceUpdate";
            this.chkForceUpdate.Size = new System.Drawing.Size(118, 22);
            this.chkForceUpdate.TabIndex = 2;
            this.chkForceUpdate.Text = "[Force Update]";
            this.chkForceUpdate.UseVisualStyleBackColor = true;
            // 
            // cmbLaunchers
            // 
            this.cmbLaunchers.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmbLaunchers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLaunchers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbLaunchers.FormattingEnabled = true;
            this.cmbLaunchers.Location = new System.Drawing.Point(3, 3);
            this.cmbLaunchers.Name = "cmbLaunchers";
            this.cmbLaunchers.Size = new System.Drawing.Size(173, 24);
            this.cmbLaunchers.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btPlay, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 457);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(820, 62);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.lblOptions, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmbOptions, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(616, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(201, 56);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // lblOptions
            // 
            this.lblOptions.BackColor = System.Drawing.Color.Transparent;
            this.lblOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOptions.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblOptions.Location = new System.Drawing.Point(3, 0);
            this.lblOptions.Name = "lblOptions";
            this.lblOptions.Size = new System.Drawing.Size(195, 28);
            this.lblOptions.TabIndex = 3;
            this.lblOptions.Text = "[Options]";
            // 
            // cmbOptions
            // 
            this.cmbOptions.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbOptions.FormattingEnabled = true;
            this.cmbOptions.Location = new System.Drawing.Point(3, 31);
            this.cmbOptions.Name = "cmbOptions";
            this.cmbOptions.Size = new System.Drawing.Size(159, 24);
            this.cmbOptions.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.cmbLaunchers, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.chkForceUpdate, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(240, 56);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // btPlay
            // 
            this.btPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btPlay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btPlay.Location = new System.Drawing.Point(273, 3);
            this.btPlay.Name = "btPlay";
            this.btPlay.Size = new System.Drawing.Size(272, 56);
            this.btPlay.TabIndex = 1;
            this.btPlay.Text = "[Play]";
            this.btPlay.UseVisualStyleBackColor = true;
            this.btPlay.Click += new System.EventHandler(this.btPlay_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.wbNews);
            this.panel1.Controls.Add(this.pbStatus);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(844, 436);
            this.panel1.TabIndex = 6;
            // 
            // wbNews
            // 
            this.wbNews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbNews.Location = new System.Drawing.Point(0, 0);
            this.wbNews.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbNews.Name = "wbNews";
            this.wbNews.Size = new System.Drawing.Size(844, 413);
            this.wbNews.TabIndex = 2;
            // 
            // pbStatus
            // 
            this.pbStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbStatus.Location = new System.Drawing.Point(0, 413);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(844, 23);
            this.pbStatus.TabIndex = 1;
            this.pbStatus.Value = 50;
            this.pbStatus.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStatus.Location = new System.Drawing.Point(12, 438);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(820, 16);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "[Installing Status]";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // oFileLauncherExe
            // 
            this.oFileLauncherExe.FileName = "launcher.jar";
            this.oFileLauncherExe.Title = "[File select title]";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 531);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "[MineLauncher]";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox chkForceUpdate;
        private System.Windows.Forms.ComboBox cmbLaunchers;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar pbStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.WebBrowser wbNews;
        private System.Windows.Forms.Button btPlay;
        private System.Windows.Forms.OpenFileDialog oFileLauncherExe;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ComboBox cmbOptions;
        private System.Windows.Forms.Label lblOptions;
    }
}