using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ExportDAE
{
	public class ExporterDialog : Form
	{
		private IContainer components;

		private Label label3;

		private Label label4;

		private Button button1;

		private Button button2;

		private Label label1;

		public ComboBox InsertionPoint;

		private Label label2;

		public SaveFileDialog SaveFileDialog;

		public NumericUpDown SkipSmallerThan;

		public CheckBox SkipInteriorDetails;

		public CheckBox CollectTextures;

		public CheckBox UnicodeSupport;

		public TrackBar levelOfDetail;

		private Label label5;

		private Label label6;

		private Label label7;

		private Label label8;

		private ToolTip toolTip1;

		private ToolTip toolTip2;

		private ToolTip toolTip3;

		private ToolTip toolTip4;
        private PictureBox pictureBox1;
        public CheckBox GeometryOptimization;

		public ExporterDialog()
		{
			this.InitializeComponent();
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Act-3D\\RevitToLumionBridge");
			this.InsertionPoint.SelectedIndex = Convert.ToInt32(registryKey.GetValue("InsertionPoint", 0), CultureInfo.InvariantCulture);
			this.SkipSmallerThan.Value = Convert.ToDecimal(registryKey.GetValue("SkipSmallerThan", 0.1), CultureInfo.InvariantCulture);
			this.SkipInteriorDetails.Checked = Convert.ToBoolean(registryKey.GetValue("SkipInteriorDetails", 0), CultureInfo.InvariantCulture);
			this.CollectTextures.Checked = Convert.ToBoolean(registryKey.GetValue("CollectTextures", 1), CultureInfo.InvariantCulture);
			this.UnicodeSupport.Checked = Convert.ToBoolean(registryKey.GetValue("UnicodeSupport", 0), CultureInfo.InvariantCulture);
			this.GeometryOptimization.Checked = Convert.ToBoolean(registryKey.GetValue("GeometryOptimization", 0), CultureInfo.InvariantCulture);
			this.levelOfDetail.Value = Math.Min(Convert.ToInt32(registryKey.GetValue("LevelOfDetail", 4), CultureInfo.InvariantCulture), 10);
			registryKey.Close();
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.lumion3d.com");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.SaveFileDialog.Filter = "COLLADA file|*.dae";
			this.SaveFileDialog.Title = "Save an COLLADA file";
			if (this.SaveFileDialog.ShowDialog() == DialogResult.Cancel)
			{
				base.DialogResult = DialogResult.None;
			}
			RegistryKey expr_45 = Registry.CurrentUser.OpenSubKey("Software\\Act-3D\\RevitToLumionBridge", true);
			expr_45.SetValue("InsertionPoint", this.InsertionPoint.SelectedIndex);
			expr_45.SetValue("SkipSmallerThan", this.SkipSmallerThan.Value);
			expr_45.SetValue("SkipInteriorDetails", this.SkipInteriorDetails.Checked);
			expr_45.SetValue("CollectTextures", this.CollectTextures.Checked);
			expr_45.SetValue("UnicodeSupport", this.UnicodeSupport.Checked);
			expr_45.SetValue("GeometryOptimization", this.GeometryOptimization.Checked);
			expr_45.SetValue("LevelOfDetail", this.levelOfDetail.Value);
			expr_45.Close();
		}

		private void levelOfDetail_ValueChanged(object sender, EventArgs e)
		{
			this.toolTip4.SetToolTip(this.levelOfDetail, (this.levelOfDetail.Value - 5).ToString());
		}

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExporterDialog));
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.InsertionPoint = new System.Windows.Forms.ComboBox();
            this.SkipSmallerThan = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SkipInteriorDetails = new System.Windows.Forms.CheckBox();
            this.CollectTextures = new System.Windows.Forms.CheckBox();
            this.UnicodeSupport = new System.Windows.Forms.CheckBox();
            this.levelOfDetail = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip3 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip4 = new System.Windows.Forms.ToolTip(this.components);
            this.GeometryOptimization = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.SkipSmallerThan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.levelOfDetail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(116, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 25);
            this.label3.TabIndex = 0;
            this.label3.Text = "Export DAE";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(120, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "version 1.7";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(21, 480);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Export";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(163, 480);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(98, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 228);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "插入点：   ";
            // 
            // InsertionPoint
            // 
            this.InsertionPoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InsertionPoint.FormattingEnabled = true;
            this.InsertionPoint.Items.AddRange(new object[] {
            "Base Point (project north)",
            "Survey Point (true north)"});
            this.InsertionPoint.Location = new System.Drawing.Point(115, 225);
            this.InsertionPoint.Name = "InsertionPoint";
            this.InsertionPoint.Size = new System.Drawing.Size(145, 23);
            this.InsertionPoint.TabIndex = 7;
            // 
            // SkipSmallerThan
            // 
            this.SkipSmallerThan.DecimalPlaces = 1;
            this.SkipSmallerThan.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.SkipSmallerThan.Location = new System.Drawing.Point(116, 192);
            this.SkipSmallerThan.Name = "SkipSmallerThan";
            this.SkipSmallerThan.Size = new System.Drawing.Size(145, 25);
            this.SkipSmallerThan.TabIndex = 8;
            this.SkipSmallerThan.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label2.Location = new System.Drawing.Point(18, 196);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "丢弃小于[m]";
            this.label2.Visible = false;
            // 
            // SkipInteriorDetails
            // 
            this.SkipInteriorDetails.AutoSize = true;
            this.SkipInteriorDetails.Location = new System.Drawing.Point(21, 398);
            this.SkipInteriorDetails.Name = "SkipInteriorDetails";
            this.SkipInteriorDetails.Size = new System.Drawing.Size(119, 19);
            this.SkipInteriorDetails.TabIndex = 10;
            this.SkipInteriorDetails.Text = "忽略内部细节";
            this.toolTip3.SetToolTip(this.SkipInteriorDetails, "Skip all interior elements.");
            this.SkipInteriorDetails.UseVisualStyleBackColor = true;
            // 
            // CollectTextures
            // 
            this.CollectTextures.AutoSize = true;
            this.CollectTextures.Location = new System.Drawing.Point(21, 348);
            this.CollectTextures.Name = "CollectTextures";
            this.CollectTextures.Size = new System.Drawing.Size(89, 19);
            this.CollectTextures.TabIndex = 11;
            this.CollectTextures.Text = "包含纹理";
            this.toolTip1.SetToolTip(this.CollectTextures, "Copy all textures to subfolder and change texture paths to relative.");
            this.CollectTextures.UseVisualStyleBackColor = true;
            // 
            // UnicodeSupport
            // 
            this.UnicodeSupport.AutoSize = true;
            this.UnicodeSupport.Location = new System.Drawing.Point(21, 423);
            this.UnicodeSupport.Name = "UnicodeSupport";
            this.UnicodeSupport.Size = new System.Drawing.Size(123, 19);
            this.UnicodeSupport.TabIndex = 12;
            this.UnicodeSupport.Text = "Unicode 支持";
            this.toolTip2.SetToolTip(this.UnicodeSupport, "Unicode support");
            this.UnicodeSupport.UseVisualStyleBackColor = true;
            // 
            // levelOfDetail
            // 
            this.levelOfDetail.Location = new System.Drawing.Point(115, 258);
            this.levelOfDetail.Name = "levelOfDetail";
            this.levelOfDetail.Size = new System.Drawing.Size(145, 56);
            this.levelOfDetail.TabIndex = 13;
            this.levelOfDetail.Value = 5;
            this.levelOfDetail.ValueChanged += new System.EventHandler(this.levelOfDetail_ValueChanged);
            this.levelOfDetail.DragEnter += new System.Windows.Forms.DragEventHandler(this.levelOfDetail_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 260);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 15);
            this.label5.TabIndex = 14;
            this.label5.Text = "几何精度：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(160, 281);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 15);
            this.label6.TabIndex = 15;
            this.label6.Text = "正常";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(112, 281);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(22, 15);
            this.label7.TabIndex = 16;
            this.label7.Text = "低";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(221, 281);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 15);
            this.label8.TabIndex = 17;
            this.label8.Text = "高";
            // 
            // GeometryOptimization
            // 
            this.GeometryOptimization.AutoSize = true;
            this.GeometryOptimization.Location = new System.Drawing.Point(21, 373);
            this.GeometryOptimization.Name = "GeometryOptimization";
            this.GeometryOptimization.Size = new System.Drawing.Size(89, 19);
            this.GeometryOptimization.TabIndex = 18;
            this.GeometryOptimization.Text = "几何优化";
            this.toolTip4.SetToolTip(this.GeometryOptimization, "Try to optimize geometry.");
            this.GeometryOptimization.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(25, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(85, 88);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            // 
            // ExporterDialog
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(281, 547);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.GeometryOptimization);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.levelOfDetail);
            this.Controls.Add(this.UnicodeSupport);
            this.Controls.Add(this.CollectTextures);
            this.Controls.Add(this.SkipInteriorDetails);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SkipSmallerThan);
            this.Controls.Add(this.InsertionPoint);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExporterDialog";
            ((System.ComponentModel.ISupportInitialize)(this.SkipSmallerThan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.levelOfDetail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

    }
}
