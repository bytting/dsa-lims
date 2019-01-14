namespace DSA_lims
{
    partial class FormImportAnalysisLIS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImportAnalysisLIS));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnShowLIS = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLIMSSampleName = new System.Windows.Forms.TextBox();
            this.tbLIMSPrepGeom = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbLIMSGeomFillHeight = new System.Windows.Forms.TextBox();
            this.tbLIMSGeomAmount = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.tbNuclideLibrary = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbDetLimLib = new System.Windows.Forms.TextBox();
            this.grid = new System.Windows.Forms.DataGridView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.btnCutshall = new System.Windows.Forms.ToolStripButton();
            this.btnWeightedMean = new System.Windows.Forms.ToolStripButton();
            this.lblLIMSGeomQuantity = new System.Windows.Forms.Label();
            this.tbLIMSGeomQuantity = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.panel3.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 688);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1058, 28);
            this.panel1.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(824, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(117, 28);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(941, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(117, 28);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Import";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnShowLIS});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1058, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnShowLIS
            // 
            this.btnShowLIS.Image = ((System.Drawing.Image)(resources.GetObject("btnShowLIS.Image")));
            this.btnShowLIS.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowLIS.Name = "btnShowLIS";
            this.btnShowLIS.Size = new System.Drawing.Size(93, 22);
            this.btnShowLIS.Text = "Show LIS file";
            this.btnShowLIS.Click += new System.EventHandler(this.btnShowLIS_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbFilename, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbLIMSSampleName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbLIMSPrepGeom, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbLIMSGeomFillHeight, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbLIMSGeomAmount, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblLIMSGeomQuantity, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbLIMSGeomQuantity, 5, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1058, 94);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(169, 28);
            this.label3.TabIndex = 4;
            this.label3.Text = "LIMS prep. geometry";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filename";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbFilename
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tbFilename, 5);
            this.tbFilename.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFilename.Location = new System.Drawing.Point(178, 3);
            this.tbFilename.Name = "tbFilename";
            this.tbFilename.ReadOnly = true;
            this.tbFilename.Size = new System.Drawing.Size(877, 21);
            this.tbFilename.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(169, 28);
            this.label2.TabIndex = 2;
            this.label2.Text = "LIMS sample name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbLIMSSampleName
            // 
            this.tbLIMSSampleName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLIMSSampleName.Location = new System.Drawing.Point(178, 31);
            this.tbLIMSSampleName.Name = "tbLIMSSampleName";
            this.tbLIMSSampleName.ReadOnly = true;
            this.tbLIMSSampleName.Size = new System.Drawing.Size(171, 21);
            this.tbLIMSSampleName.TabIndex = 3;
            // 
            // tbLIMSPrepGeom
            // 
            this.tbLIMSPrepGeom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLIMSPrepGeom.Location = new System.Drawing.Point(178, 59);
            this.tbLIMSPrepGeom.Name = "tbLIMSPrepGeom";
            this.tbLIMSPrepGeom.ReadOnly = true;
            this.tbLIMSPrepGeom.Size = new System.Drawing.Size(171, 21);
            this.tbLIMSPrepGeom.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(355, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(169, 28);
            this.label4.TabIndex = 6;
            this.label4.Text = "LIMS geom.fill height";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(355, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(169, 28);
            this.label5.TabIndex = 7;
            this.label5.Text = "LIMS geom.amount";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbLIMSGeomFillHeight
            // 
            this.tbLIMSGeomFillHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLIMSGeomFillHeight.Location = new System.Drawing.Point(530, 31);
            this.tbLIMSGeomFillHeight.Name = "tbLIMSGeomFillHeight";
            this.tbLIMSGeomFillHeight.ReadOnly = true;
            this.tbLIMSGeomFillHeight.Size = new System.Drawing.Size(171, 21);
            this.tbLIMSGeomFillHeight.TabIndex = 8;
            // 
            // tbLIMSGeomAmount
            // 
            this.tbLIMSGeomAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLIMSGeomAmount.Location = new System.Drawing.Point(530, 59);
            this.tbLIMSGeomAmount.Name = "tbLIMSGeomAmount";
            this.tbLIMSGeomAmount.ReadOnly = true;
            this.tbLIMSGeomAmount.Size = new System.Drawing.Size(171, 21);
            this.tbLIMSGeomAmount.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 612);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1058, 76);
            this.panel2.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tbNuclideLibrary, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tbDetLimLib, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1058, 76);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(169, 28);
            this.label6.TabIndex = 0;
            this.label6.Text = "Nuclide lib.";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbNuclideLibrary
            // 
            this.tbNuclideLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbNuclideLibrary.Location = new System.Drawing.Point(178, 3);
            this.tbNuclideLibrary.Name = "tbNuclideLibrary";
            this.tbNuclideLibrary.ReadOnly = true;
            this.tbNuclideLibrary.Size = new System.Drawing.Size(348, 21);
            this.tbNuclideLibrary.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(169, 28);
            this.label7.TabIndex = 2;
            this.label7.Text = "Detection Limit lib.";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbDetLimLib
            // 
            this.tbDetLimLib.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDetLimLib.Location = new System.Drawing.Point(178, 31);
            this.tbDetLimLib.Name = "tbDetLimLib";
            this.tbDetLimLib.ReadOnly = true;
            this.tbDetLimLib.Size = new System.Drawing.Size(348, 21);
            this.tbDetLimLib.TabIndex = 3;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 25);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.RowHeadersVisible = false;
            this.grid.Size = new System.Drawing.Size(1058, 468);
            this.grid.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.grid);
            this.panel3.Controls.Add(this.toolStrip2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 119);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1058, 493);
            this.panel3.TabIndex = 5;
            // 
            // toolStrip2
            // 
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCutshall,
            this.btnWeightedMean});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1058, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // btnCutshall
            // 
            this.btnCutshall.Image = ((System.Drawing.Image)(resources.GetObject("btnCutshall.Image")));
            this.btnCutshall.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCutshall.Name = "btnCutshall";
            this.btnCutshall.Size = new System.Drawing.Size(94, 22);
            this.btnCutshall.Text = "Run Cutshall";
            // 
            // btnWeightedMean
            // 
            this.btnWeightedMean.Image = ((System.Drawing.Image)(resources.GetObject("btnWeightedMean.Image")));
            this.btnWeightedMean.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWeightedMean.Name = "btnWeightedMean";
            this.btnWeightedMean.Size = new System.Drawing.Size(133, 22);
            this.btnWeightedMean.Text = "Run weighted mean";
            // 
            // lblLIMSGeomQuantity
            // 
            this.lblLIMSGeomQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLIMSGeomQuantity.Location = new System.Drawing.Point(707, 28);
            this.lblLIMSGeomQuantity.Name = "lblLIMSGeomQuantity";
            this.lblLIMSGeomQuantity.Size = new System.Drawing.Size(169, 28);
            this.lblLIMSGeomQuantity.TabIndex = 10;
            this.lblLIMSGeomQuantity.Text = "LIMS geom.quantity";
            this.lblLIMSGeomQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbLIMSGeomQuantity
            // 
            this.tbLIMSGeomQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLIMSGeomQuantity.Location = new System.Drawing.Point(882, 31);
            this.tbLIMSGeomQuantity.Name = "tbLIMSGeomQuantity";
            this.tbLIMSGeomQuantity.ReadOnly = true;
            this.tbLIMSGeomQuantity.Size = new System.Drawing.Size(173, 21);
            this.tbLIMSGeomQuantity.TabIndex = 12;
            // 
            // FormImportAnalysisLIS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 716);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "FormImportAnalysisLIS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Import LIS file";
            this.Load += new System.EventHandler(this.FormImportAnalysisLIS_Load);
            this.panel1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnShowLIS;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLIMSSampleName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbLIMSPrepGeom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbLIMSGeomFillHeight;
        private System.Windows.Forms.TextBox tbLIMSGeomAmount;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbNuclideLibrary;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbDetLimLib;
        private System.Windows.Forms.ToolStripButton btnCutshall;
        private System.Windows.Forms.ToolStripButton btnWeightedMean;
        private System.Windows.Forms.Label lblLIMSGeomQuantity;
        private System.Windows.Forms.TextBox tbLIMSGeomQuantity;
    }
}