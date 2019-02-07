namespace DSA_lims
{
    partial class FormGetCoords
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGetCoords));
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.panelGMap = new System.Windows.Forms.Panel();
            this.tools = new System.Windows.Forms.ToolStrip();
            this.cboxProviders = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.panel2.SuspendLayout();
            this.tools.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnOk);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 493);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(692, 28);
            this.panel2.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(492, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(592, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // panelGMap
            // 
            this.panelGMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGMap.Location = new System.Drawing.Point(0, 25);
            this.panelGMap.Name = "panelGMap";
            this.panelGMap.Size = new System.Drawing.Size(692, 468);
            this.panelGMap.TabIndex = 9;
            // 
            // tools
            // 
            this.tools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cboxProviders});
            this.tools.Location = new System.Drawing.Point(0, 0);
            this.tools.Name = "tools";
            this.tools.Size = new System.Drawing.Size(692, 25);
            this.tools.TabIndex = 10;
            this.tools.Text = "toolStrip1";
            // 
            // cboxProviders
            // 
            this.cboxProviders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxProviders.Name = "cboxProviders";
            this.cboxProviders.Size = new System.Drawing.Size(220, 25);
            this.cboxProviders.SelectedIndexChanged += new System.EventHandler(this.cboxProviders_SelectedIndexChanged);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(81, 22);
            this.toolStripLabel1.Text = "Map provider:";
            // 
            // FormGetCoords
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 521);
            this.Controls.Add(this.panelGMap);
            this.Controls.Add(this.tools);
            this.Controls.Add(this.panel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGetCoords";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Select a coordinate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGetCoords_FormClosing);
            this.Load += new System.EventHandler(this.FormGetCoords_Load);
            this.panel2.ResumeLayout(false);
            this.tools.ResumeLayout(false);
            this.tools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Panel panelGMap;
        private System.Windows.Forms.ToolStrip tools;
        private System.Windows.Forms.ToolStripComboBox cboxProviders;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    }
}