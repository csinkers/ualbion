namespace UAlbion.Tools.ImageReverser
{
    partial class ImageViewer
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.canvas = new System.Windows.Forms.PictureBox();
            this.chkAnimate = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numFrame = new System.Windows.Forms.NumericUpDown();
            this.trackFrame = new System.Windows.Forms.TrackBar();
            this.numFrameCount = new System.Windows.Forms.NumericUpDown();
            this.trackFrameCount = new System.Windows.Forms.TrackBar();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.trackWidth = new System.Windows.Forms.TrackBar();
            this.chkListPalettes = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrameCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrameCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackWidth)).BeginInit();
            this.SuspendLayout();
            //
            // canvas
            //
            this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvas.Location = new System.Drawing.Point(297, 19);
            this.canvas.Margin = new System.Windows.Forms.Padding(4);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(1465, 1096);
            this.canvas.TabIndex = 5;
            this.canvas.TabStop = false;
            //
            // chkAnimate
            //
            this.chkAnimate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAnimate.AutoSize = true;
            this.chkAnimate.Checked = true;
            this.chkAnimate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAnimate.Location = new System.Drawing.Point(1589, 1326);
            this.chkAnimate.Margin = new System.Windows.Forms.Padding(4);
            this.chkAnimate.Name = "chkAnimate";
            this.chkAnimate.Size = new System.Drawing.Size(122, 29);
            this.chkAnimate.TabIndex = 22;
            this.chkAnimate.Text = "Animate";
            this.chkAnimate.UseVisualStyleBackColor = true;
            this.chkAnimate.CheckedChanged += ChkAnimate_CheckedChanged;
            //
            // label3
            //
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 1330);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 25);
            this.label3.TabIndex = 28;
            this.label3.Text = "Frame:";
            //
            // label2
            //
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 1145);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 25);
            this.label2.TabIndex = 27;
            this.label2.Text = "Width:";
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 1238);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 25);
            this.label1.TabIndex = 26;
            this.label1.Text = "Frame Count:";
            //
            // numFrame
            //
            this.numFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numFrame.Location = new System.Drawing.Point(1353, 1330);
            this.numFrame.Margin = new System.Windows.Forms.Padding(4);
            this.numFrame.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numFrame.Name = "numFrame";
            this.numFrame.Size = new System.Drawing.Size(120, 31);
            this.numFrame.TabIndex = 25;
            this.numFrame.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numFrame.ValueChanged += NumFrame_ValueChanged;
            //
            // trackFrame
            //
            this.trackFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackFrame.Location = new System.Drawing.Point(213, 1309);
            this.trackFrame.Margin = new System.Windows.Forms.Padding(4);
            this.trackFrame.Maximum = 1024;
            this.trackFrame.Name = "trackFrame";
            this.trackFrame.Size = new System.Drawing.Size(1132, 90);
            this.trackFrame.TabIndex = 21;
            this.trackFrame.Value = 32;
            this.trackFrame.ValueChanged += TrackFrame_ValueChanged;
            //
            // numFrameCount
            //
            this.numFrameCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numFrameCount.Location = new System.Drawing.Point(1589, 1230);
            this.numFrameCount.Margin = new System.Windows.Forms.Padding(4);
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
            this.numFrameCount.Size = new System.Drawing.Size(120, 31);
            this.numFrameCount.TabIndex = 24;
            this.numFrameCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numFrameCount.ValueChanged += NumFrameCount_ValueChanged;
            //
            // trackFrameCount
            //
            this.trackFrameCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackFrameCount.Location = new System.Drawing.Point(213, 1230);
            this.trackFrameCount.Margin = new System.Windows.Forms.Padding(4);
            this.trackFrameCount.Maximum = 1024;
            this.trackFrameCount.Minimum = 1;
            this.trackFrameCount.Name = "trackFrameCount";
            this.trackFrameCount.Size = new System.Drawing.Size(1368, 90);
            this.trackFrameCount.TabIndex = 20;
            this.trackFrameCount.Value = 1;
            this.trackFrameCount.ValueChanged += TrackFrameCount_ValueChanged;
            //
            // numWidth
            //
            this.numWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numWidth.Location = new System.Drawing.Point(1589, 1145);
            this.numWidth.Margin = new System.Windows.Forms.Padding(4);
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
            this.numWidth.Size = new System.Drawing.Size(120, 31);
            this.numWidth.TabIndex = 23;
            this.numWidth.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numWidth.Enter += NumWidth_Enter;
            this.numWidth.ValueChanged += NumWidth_ValueChanged;
            //
            // trackWidth
            //
            this.trackWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackWidth.Location = new System.Drawing.Point(213, 1138);
            this.trackWidth.Margin = new System.Windows.Forms.Padding(4);
            this.trackWidth.Maximum = 1024;
            this.trackWidth.Minimum = 1;
            this.trackWidth.Name = "trackWidth";
            this.trackWidth.Size = new System.Drawing.Size(1368, 90);
            this.trackWidth.TabIndex = 19;
            this.trackWidth.Value = 32;
            this.trackWidth.ValueChanged += TrackWidth_ValueChanged;
            this.trackWidth.KeyDown += TrackWidth_KeyDown;
            //
            // chkListPalettes
            //
            this.chkListPalettes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkListPalettes.FormattingEnabled = true;
            this.chkListPalettes.Location = new System.Drawing.Point(4, 19);
            this.chkListPalettes.Margin = new System.Windows.Forms.Padding(4);
            this.chkListPalettes.Name = "chkListPalettes";
            this.chkListPalettes.Size = new System.Drawing.Size(285, 1096);
            this.chkListPalettes.TabIndex = 30;
            this.chkListPalettes.SelectedIndexChanged += ChkListPalettes_SelectedIndexChanged;
            this.chkListPalettes.ItemCheck += ChkListPalettes_ItemCheck;
            //
            // ImageViewer
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkListPalettes);
            this.Controls.Add(this.chkAnimate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numFrame);
            this.Controls.Add(this.trackFrame);
            this.Controls.Add(this.numFrameCount);
            this.Controls.Add(this.trackFrameCount);
            this.Controls.Add(this.numWidth);
            this.Controls.Add(this.trackWidth);
            this.Controls.Add(this.canvas);
            this.Name = "ImageViewer";
            this.Size = new System.Drawing.Size(1766, 1415);
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrameCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrameCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox canvas;
        private System.Windows.Forms.CheckBox chkAnimate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numFrame;
        private System.Windows.Forms.TrackBar trackFrame;
        private System.Windows.Forms.NumericUpDown numFrameCount;
        private System.Windows.Forms.TrackBar trackFrameCount;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.TrackBar trackWidth;
        private System.Windows.Forms.CheckedListBox chkListPalettes;
    }
}
