namespace DSA_lims
{
    partial class FormOrderAddSampleType
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOrderAddSampleType));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSelectSampleType = new System.Windows.Forms.Button();
            this.panelSampleType = new System.Windows.Forms.Panel();
            this.cboxSampleType = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.cboxSampleComponent = new System.Windows.Forms.ComboBox();
            this.cbReturnToSender = new System.Windows.Forms.CheckBox();
            this.cboxRequestedUnit = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboxRequestedUnitType = new System.Windows.Forms.ComboBox();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbNumSamples = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panelSampleType.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 324);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(618, 28);
            this.panel1.TabIndex = 6;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(418, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(518, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Selected sample type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Number of samples";
            // 
            // btnSelectSampleType
            // 
            this.btnSelectSampleType.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSelectSampleType.Image = global::DSA_lims.Properties.Resources.tree_16;
            this.btnSelectSampleType.Location = new System.Drawing.Point(425, 0);
            this.btnSelectSampleType.Name = "btnSelectSampleType";
            this.btnSelectSampleType.Size = new System.Drawing.Size(24, 24);
            this.btnSelectSampleType.TabIndex = 10;
            this.btnSelectSampleType.Text = "...";
            this.btnSelectSampleType.UseVisualStyleBackColor = true;
            this.btnSelectSampleType.Click += new System.EventHandler(this.btnSelectSampleType_Click);
            // 
            // panelSampleType
            // 
            this.panelSampleType.Controls.Add(this.cboxSampleType);
            this.panelSampleType.Controls.Add(this.btnSelectSampleType);
            this.panelSampleType.Location = new System.Drawing.Point(140, 21);
            this.panelSampleType.Name = "panelSampleType";
            this.panelSampleType.Size = new System.Drawing.Size(449, 24);
            this.panelSampleType.TabIndex = 11;
            // 
            // cboxSampleType
            // 
            this.cboxSampleType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboxSampleType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboxSampleType.DisplayMember = "Name";
            this.cboxSampleType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxSampleType.FormattingEnabled = true;
            this.cboxSampleType.Location = new System.Drawing.Point(0, 0);
            this.cboxSampleType.Name = "cboxSampleType";
            this.cboxSampleType.Size = new System.Drawing.Size(425, 21);
            this.cboxSampleType.TabIndex = 0;
            this.cboxSampleType.ValueMember = "Id";
            this.cboxSampleType.SelectedIndexChanged += new System.EventHandler(this.cboxSampleType_SelectedIndexChanged);
            this.cboxSampleType.Leave += new System.EventHandler(this.cboxSampleType_Leave);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 352);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(618, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(68, 17);
            this.lblStatus.Text = "<lblStatus>";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Sample component";
            // 
            // cboxSampleComponent
            // 
            this.cboxSampleComponent.DisplayMember = "Name";
            this.cboxSampleComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSampleComponent.FormattingEnabled = true;
            this.cboxSampleComponent.Location = new System.Drawing.Point(140, 50);
            this.cboxSampleComponent.Name = "cboxSampleComponent";
            this.cboxSampleComponent.Size = new System.Drawing.Size(449, 21);
            this.cboxSampleComponent.TabIndex = 1;
            this.cboxSampleComponent.ValueMember = "Id";
            // 
            // cbReturnToSender
            // 
            this.cbReturnToSender.AutoSize = true;
            this.cbReturnToSender.Location = new System.Drawing.Point(140, 141);
            this.cbReturnToSender.Name = "cbReturnToSender";
            this.cbReturnToSender.Size = new System.Drawing.Size(146, 17);
            this.cbReturnToSender.TabIndex = 5;
            this.cbReturnToSender.Text = "Return samples to sender";
            this.cbReturnToSender.UseVisualStyleBackColor = true;
            // 
            // cboxRequestedUnit
            // 
            this.cboxRequestedUnit.DisplayMember = "Name";
            this.cboxRequestedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxRequestedUnit.FormattingEnabled = true;
            this.cboxRequestedUnit.Location = new System.Drawing.Point(140, 103);
            this.cboxRequestedUnit.Name = "cboxRequestedUnit";
            this.cboxRequestedUnit.Size = new System.Drawing.Size(222, 21);
            this.cboxRequestedUnit.TabIndex = 3;
            this.cboxRequestedUnit.ValueMember = "Id";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(55, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Requested unit";
            // 
            // cboxRequestedUnitType
            // 
            this.cboxRequestedUnitType.DisplayMember = "Name";
            this.cboxRequestedUnitType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxRequestedUnitType.FormattingEnabled = true;
            this.cboxRequestedUnitType.Location = new System.Drawing.Point(368, 103);
            this.cboxRequestedUnitType.Name = "cboxRequestedUnitType";
            this.cboxRequestedUnitType.Size = new System.Drawing.Size(221, 21);
            this.cboxRequestedUnitType.TabIndex = 4;
            this.cboxRequestedUnitType.ValueMember = "Id";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(140, 176);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(449, 115);
            this.tbComment.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(83, 179);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Comment";
            // 
            // tbNumSamples
            // 
            this.tbNumSamples.Location = new System.Drawing.Point(140, 77);
            this.tbNumSamples.MaxLength = 32;
            this.tbNumSamples.Name = "tbNumSamples";
            this.tbNumSamples.Size = new System.Drawing.Size(449, 20);
            this.tbNumSamples.TabIndex = 2;
            // 
            // FormOrderAddSampleType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 374);
            this.Controls.Add(this.tbNumSamples);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panelSampleType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboxRequestedUnitType);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbReturnToSender);
            this.Controls.Add(this.cboxRequestedUnit);
            this.Controls.Add(this.cboxSampleComponent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOrderAddSampleType";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Add entry to order";
            this.Load += new System.EventHandler(this.FormOrderAddSampleType_Load);
            this.panel1.ResumeLayout(false);
            this.panelSampleType.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelectSampleType;
        private System.Windows.Forms.Panel panelSampleType;
        private System.Windows.Forms.ComboBox cboxSampleType;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboxSampleComponent;
        private System.Windows.Forms.CheckBox cbReturnToSender;
        private System.Windows.Forms.ComboBox cboxRequestedUnit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboxRequestedUnitType;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbNumSamples;
    }
}