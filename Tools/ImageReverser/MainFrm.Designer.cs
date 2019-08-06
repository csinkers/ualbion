namespace UAlbion.Tools.ImageReverser
{
    partial class MainFrm
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
            this.fileTree = new System.Windows.Forms.TreeView();
            this.trackWidth = new System.Windows.Forms.TrackBar();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.textName = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.canvas = new System.Windows.Forms.PictureBox();
            this.trackFrameCount = new System.Windows.Forms.TrackBar();
            this.numFrameCount = new System.Windows.Forms.NumericUpDown();
            this.listPalettes = new System.Windows.Forms.ListBox();
            this.trackFrame = new System.Windows.Forms.TrackBar();
            this.numFrame = new System.Windows.Forms.NumericUpDown();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkAnimate = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrameCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrameCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrame)).BeginInit();
            this.SuspendLayout();
            // 
            // fileTree
            // 
            this.fileTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.fileTree.Location = new System.Drawing.Point(181, 13);
            this.fileTree.Margin = new System.Windows.Forms.Padding(2);
            this.fileTree.Name = "fileTree";
            this.fileTree.Size = new System.Drawing.Size(236, 1463);
            this.fileTree.TabIndex = 0;
            this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FileTree_AfterSelect);
            this.fileTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileTree_KeyDown);
            // 
            // trackWidth
            // 
            this.trackWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackWidth.Location = new System.Drawing.Point(520, 1318);
            this.trackWidth.Margin = new System.Windows.Forms.Padding(2);
            this.trackWidth.Maximum = 1024;
            this.trackWidth.Minimum = 1;
            this.trackWidth.Name = "trackWidth";
            this.trackWidth.Size = new System.Drawing.Size(1072, 56);
            this.trackWidth.TabIndex = 1;
            this.trackWidth.Value = 32;
            this.trackWidth.ValueChanged += new System.EventHandler(this.TrackWidth_ValueChanged);
            this.trackWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TrackWidth_KeyDown);
            // 
            // numWidth
            // 
            this.numWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numWidth.Location = new System.Drawing.Point(1596, 1323);
            this.numWidth.Margin = new System.Windows.Forms.Padding(2);
            this.numWidth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(75, 22);
            this.numWidth.TabIndex = 2;
            this.numWidth.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numWidth.ValueChanged += new System.EventHandler(this.NumWidth_ValueChanged);
            this.numWidth.Enter += new System.EventHandler(this.NumWidth_Enter);
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textName.Location = new System.Drawing.Point(520, 1292);
            this.textName.Margin = new System.Windows.Forms.Padding(2);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(518, 22);
            this.textName.TabIndex = 3;
            this.textName.TextChanged += new System.EventHandler(this.TextName_TextChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(1527, 1443);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(144, 42);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // canvas
            // 
            this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvas.Location = new System.Drawing.Point(688, 13);
            this.canvas.Margin = new System.Windows.Forms.Padding(2);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(983, 1275);
            this.canvas.TabIndex = 5;
            this.canvas.TabStop = false;
            // 
            // trackFrameCount
            // 
            this.trackFrameCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackFrameCount.Location = new System.Drawing.Point(520, 1378);
            this.trackFrameCount.Margin = new System.Windows.Forms.Padding(2);
            this.trackFrameCount.Maximum = 1024;
            this.trackFrameCount.Minimum = 1;
            this.trackFrameCount.Name = "trackFrameCount";
            this.trackFrameCount.Size = new System.Drawing.Size(1072, 56);
            this.trackFrameCount.TabIndex = 7;
            this.trackFrameCount.Value = 1;
            this.trackFrameCount.ValueChanged += new System.EventHandler(this.TrackFrameCount_ValueChanged);
            // 
            // numFrameCount
            // 
            this.numFrameCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numFrameCount.Location = new System.Drawing.Point(1596, 1378);
            this.numFrameCount.Margin = new System.Windows.Forms.Padding(2);
            this.numFrameCount.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numFrameCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numFrameCount.Name = "numFrameCount";
            this.numFrameCount.Size = new System.Drawing.Size(75, 22);
            this.numFrameCount.TabIndex = 8;
            this.numFrameCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numFrameCount.ValueChanged += new System.EventHandler(this.NumFrameCount_ValueChanged);
            // 
            // listPalettes
            // 
            this.listPalettes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listPalettes.FormattingEnabled = true;
            this.listPalettes.ItemHeight = 16;
            this.listPalettes.Location = new System.Drawing.Point(13, 13);
            this.listPalettes.Name = "listPalettes";
            this.listPalettes.Size = new System.Drawing.Size(163, 1460);
            this.listPalettes.TabIndex = 11;
            this.listPalettes.SelectedIndexChanged += new System.EventHandler(this.ListPalettes_SelectedIndexChanged);
            // 
            // trackFrame
            // 
            this.trackFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackFrame.Location = new System.Drawing.Point(520, 1429);
            this.trackFrame.Margin = new System.Windows.Forms.Padding(2);
            this.trackFrame.Maximum = 1024;
            this.trackFrame.Name = "trackFrame";
            this.trackFrame.Size = new System.Drawing.Size(924, 56);
            this.trackFrame.TabIndex = 12;
            this.trackFrame.Value = 32;
            this.trackFrame.ValueChanged += new System.EventHandler(this.TrackFrame_ValueChanged);
            // 
            // numFrame
            // 
            this.numFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numFrame.Location = new System.Drawing.Point(1448, 1443);
            this.numFrame.Margin = new System.Windows.Forms.Padding(2);
            this.numFrame.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numFrame.Name = "numFrame";
            this.numFrame.Size = new System.Drawing.Size(75, 22);
            this.numFrame.TabIndex = 13;
            this.numFrame.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numFrame.ValueChanged += new System.EventHandler(this.NumFrame_ValueChanged);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtInfo.Location = new System.Drawing.Point(422, 12);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(261, 1274);
            this.txtInfo.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(422, 1383);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 15;
            this.label1.Text = "Frame Count:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(422, 1323);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 17);
            this.label2.TabIndex = 16;
            this.label2.Text = "Width:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(422, 1443);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 17);
            this.label3.TabIndex = 17;
            this.label3.Text = "Frame:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(422, 1292);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 17);
            this.label4.TabIndex = 18;
            this.label4.Text = "Name:";
            // 
            // chkAnimate
            // 
            this.chkAnimate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAnimate.AutoSize = true;
            this.chkAnimate.Checked = true;
            this.chkAnimate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAnimate.Location = new System.Drawing.Point(1056, 1294);
            this.chkAnimate.Name = "chkAnimate";
            this.chkAnimate.Size = new System.Drawing.Size(81, 21);
            this.chkAnimate.TabIndex = 19;
            this.chkAnimate.Text = "Animate";
            this.chkAnimate.UseVisualStyleBackColor = true;
            this.chkAnimate.CheckedChanged += new System.EventHandler(this.ChkAnimate_CheckedChanged);
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1682, 1496);
            this.Controls.Add(this.chkAnimate);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.numFrame);
            this.Controls.Add(this.trackFrame);
            this.Controls.Add(this.listPalettes);
            this.Controls.Add(this.numFrameCount);
            this.Controls.Add(this.trackFrameCount);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.textName);
            this.Controls.Add(this.numWidth);
            this.Controls.Add(this.trackWidth);
            this.Controls.Add(this.fileTree);
            this.Controls.Add(this.canvas);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainFrm";
            this.Text = "Image Reverser";
            this.Load += new System.EventHandler(this.MainFrm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrameCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrameCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrame)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView fileTree;
        private System.Windows.Forms.TrackBar trackWidth;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.PictureBox canvas;
        private System.Windows.Forms.TrackBar trackFrameCount;
        private System.Windows.Forms.NumericUpDown numFrameCount;
        private System.Windows.Forms.ListBox listPalettes;
        private System.Windows.Forms.TrackBar trackFrame;
        private System.Windows.Forms.NumericUpDown numFrame;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkAnimate;
    }
}

