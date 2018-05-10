namespace pti2405
{
    partial class Form1
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
            this.comboBoxchan = new System.Windows.Forms.ComboBox();
            this.buttonstart = new System.Windows.Forms.Button();
            this.buttonstop = new System.Windows.Forms.Button();
            this.textBox1daqtime = new System.Windows.Forms.TextBox();
            this.comboBoxsamplerate = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxchan
            // 
            this.comboBoxchan.FormattingEnabled = true;
            this.comboBoxchan.Items.AddRange(new object[] {
            "Ch0",
            "Ch0~Ch1",
            "Ch0~Ch2",
            "Ch0~Ch3"});
            this.comboBoxchan.Location = new System.Drawing.Point(12, 21);
            this.comboBoxchan.Name = "comboBoxchan";
            this.comboBoxchan.Size = new System.Drawing.Size(168, 20);
            this.comboBoxchan.TabIndex = 0;
            // 
            // buttonstart
            // 
            this.buttonstart.Location = new System.Drawing.Point(12, 101);
            this.buttonstart.Name = "buttonstart";
            this.buttonstart.Size = new System.Drawing.Size(167, 42);
            this.buttonstart.TabIndex = 1;
            this.buttonstart.Text = "Start";
            this.buttonstart.UseVisualStyleBackColor = true;
            this.buttonstart.Click += new System.EventHandler(this.buttonstart_Click);
            // 
            // buttonstop
            // 
            this.buttonstop.Location = new System.Drawing.Point(12, 155);
            this.buttonstop.Name = "buttonstop";
            this.buttonstop.Size = new System.Drawing.Size(167, 41);
            this.buttonstop.TabIndex = 2;
            this.buttonstop.Text = "Stop";
            this.buttonstop.UseVisualStyleBackColor = true;
            this.buttonstop.Click += new System.EventHandler(this.buttonstop_Click);
            // 
            // textBox1daqtime
            // 
            this.textBox1daqtime.Location = new System.Drawing.Point(214, 219);
            this.textBox1daqtime.Name = "textBox1daqtime";
            this.textBox1daqtime.Size = new System.Drawing.Size(59, 22);
            this.textBox1daqtime.TabIndex = 3;
            // 
            // comboBoxsamplerate
            // 
            this.comboBoxsamplerate.FormattingEnabled = true;
            this.comboBoxsamplerate.Items.AddRange(new object[] {
            "1000 Hz",
            "10000Hz",
            "20000Hz"});
            this.comboBoxsamplerate.Location = new System.Drawing.Point(12, 63);
            this.comboBoxsamplerate.Name = "comboBoxsamplerate";
            this.comboBoxsamplerate.Size = new System.Drawing.Size(167, 20);
            this.comboBoxsamplerate.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "Channel";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Sampling Rate";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(193, 208);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxsamplerate);
            this.Controls.Add(this.textBox1daqtime);
            this.Controls.Add(this.buttonstop);
            this.Controls.Add(this.buttonstart);
            this.Controls.Add(this.comboBoxchan);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "ADLINK";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxchan;
        private System.Windows.Forms.Button buttonstart;
        private System.Windows.Forms.Button buttonstop;
        private System.Windows.Forms.TextBox textBox1daqtime;
        private System.Windows.Forms.ComboBox comboBoxsamplerate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

