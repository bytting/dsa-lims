namespace DSA_lims
{
    partial class FormScan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScan));
            this.tblScan = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cboxScanner = new System.Windows.Forms.ComboBox();
            this.cbDuplex = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboxFlipType = new System.Windows.Forms.ComboBox();
            this.cboxPixelType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnStore = new System.Windows.Forms.Button();
            this.tblScan.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblScan
            // 
            this.tblScan.ColumnCount = 2;
            this.tblScan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblScan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tblScan.Controls.Add(this.label1, 0, 1);
            this.tblScan.Controls.Add(this.cboxScanner, 1, 1);
            this.tblScan.Controls.Add(this.cbDuplex, 1, 2);
            this.tblScan.Controls.Add(this.label2, 0, 5);
            this.tblScan.Controls.Add(this.tbFileName, 1, 5);
            this.tblScan.Controls.Add(this.label3, 0, 3);
            this.tblScan.Controls.Add(this.cboxFlipType, 1, 3);
            this.tblScan.Controls.Add(this.cboxPixelType, 1, 4);
            this.tblScan.Controls.Add(this.label4, 0, 4);
            this.tblScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblScan.Location = new System.Drawing.Point(0, 0);
            this.tblScan.Name = "tblScan";
            this.tblScan.RowCount = 7;
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblScan.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblScan.Size = new System.Drawing.Size(438, 183);
            this.tblScan.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose scanner";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboxScanner
            // 
            this.cboxScanner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxScanner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxScanner.FormattingEnabled = true;
            this.cboxScanner.Location = new System.Drawing.Point(112, 11);
            this.cboxScanner.Name = "cboxScanner";
            this.cboxScanner.Size = new System.Drawing.Size(323, 23);
            this.cboxScanner.TabIndex = 1;
            // 
            // cbDuplex
            // 
            this.cbDuplex.AutoSize = true;
            this.cbDuplex.Location = new System.Drawing.Point(112, 41);
            this.cbDuplex.Name = "cbDuplex";
            this.cbDuplex.Size = new System.Drawing.Size(113, 19);
            this.cbDuplex.TabIndex = 2;
            this.cbDuplex.Text = "Scan both sides";
            this.cbDuplex.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 30);
            this.label2.TabIndex = 3;
            this.label2.Text = "Document name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbFileName
            // 
            this.tbFileName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFileName.Location = new System.Drawing.Point(112, 131);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(323, 21);
            this.tbFileName.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 30);
            this.label3.TabIndex = 7;
            this.label3.Text = "Flip type";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboxFlipType
            // 
            this.cboxFlipType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxFlipType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxFlipType.FormattingEnabled = true;
            this.cboxFlipType.Items.AddRange(new object[] {
            "Book",
            "Fanfold"});
            this.cboxFlipType.Location = new System.Drawing.Point(112, 71);
            this.cboxFlipType.Name = "cboxFlipType";
            this.cboxFlipType.Size = new System.Drawing.Size(323, 23);
            this.cboxFlipType.TabIndex = 6;
            // 
            // cboxPixelType
            // 
            this.cboxPixelType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxPixelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPixelType.FormattingEnabled = true;
            this.cboxPixelType.Items.AddRange(new object[] {
            "Color",
            "Black and White"});
            this.cboxPixelType.Location = new System.Drawing.Point(112, 101);
            this.cboxPixelType.Name = "cboxPixelType";
            this.cboxPixelType.Size = new System.Drawing.Size(323, 23);
            this.cboxPixelType.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 30);
            this.label4.TabIndex = 8;
            this.label4.Text = "Pixel type";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnPreview);
            this.panel1.Controls.Add(this.btnScan);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnStore);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 183);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(438, 30);
            this.panel1.TabIndex = 1;
            // 
            // btnPreview
            // 
            this.btnPreview.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPreview.Enabled = false;
            this.btnPreview.Location = new System.Drawing.Point(90, 0);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(90, 30);
            this.btnPreview.TabIndex = 2;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnScan
            // 
            this.btnScan.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnScan.Location = new System.Drawing.Point(0, 0);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(90, 30);
            this.btnScan.TabIndex = 0;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(258, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnStore
            // 
            this.btnStore.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnStore.Enabled = false;
            this.btnStore.Location = new System.Drawing.Point(348, 0);
            this.btnStore.Name = "btnStore";
            this.btnStore.Size = new System.Drawing.Size(90, 30);
            this.btnStore.TabIndex = 3;
            this.btnStore.Text = "Store";
            this.btnStore.UseVisualStyleBackColor = true;
            this.btnStore.Click += new System.EventHandler(this.btnStore_Click);
            // 
            // FormScan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 213);
            this.Controls.Add(this.tblScan);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormScan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Scanning";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormScan_FormClosing);
            this.Load += new System.EventHandler(this.FormScan_Load);
            this.tblScan.ResumeLayout(false);
            this.tblScan.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblScan;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboxScanner;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbDuplex;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.ComboBox cboxPixelType;
        private System.Windows.Forms.ComboBox cboxFlipType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnStore;
    }
}