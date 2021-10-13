
namespace Sum_Calculator_RPC_Client
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.connectBtn = new System.Windows.Forms.Button();
            this.sendBtn = new System.Windows.Forms.Button();
            this.disconnectBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.clearBtn = new System.Windows.Forms.Button();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.sendTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(8, 80);
            this.connectBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(133, 28);
            this.connectBtn.TabIndex = 0;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // sendBtn
            // 
            this.sendBtn.Location = new System.Drawing.Point(12, 63);
            this.sendBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(100, 28);
            this.sendBtn.TabIndex = 1;
            this.sendBtn.Text = "Send";
            this.sendBtn.UseVisualStyleBackColor = true;
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // disconnectBtn
            // 
            this.disconnectBtn.Enabled = false;
            this.disconnectBtn.Location = new System.Drawing.Point(176, 78);
            this.disconnectBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.disconnectBtn.Name = "disconnectBtn";
            this.disconnectBtn.Size = new System.Drawing.Size(128, 28);
            this.disconnectBtn.TabIndex = 2;
            this.disconnectBtn.Text = "Disconnect";
            this.disconnectBtn.UseVisualStyleBackColor = true;
            this.disconnectBtn.Click += new System.EventHandler(this.disconnectBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtIP);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.disconnectBtn);
            this.groupBox1.Controls.Add(this.txtPort);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.connectBtn);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(647, 123);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông tin server";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(91, 18);
            this.txtIP.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(213, 22);
            this.txtIP.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 52);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(91, 48);
            this.txtPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(213, 22);
            this.txtPort.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Địa chỉ IP:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.clearBtn);
            this.groupBox2.Controls.Add(this.logTextBox);
            this.groupBox2.Controls.Add(this.sendTextBox);
            this.groupBox2.Controls.Add(this.sendBtn);
            this.groupBox2.Location = new System.Drawing.Point(16, 145);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(647, 393);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ứng dụng";
            // 
            // clearBtn
            // 
            this.clearBtn.Location = new System.Drawing.Point(539, 74);
            this.clearBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(100, 28);
            this.clearBtn.TabIndex = 11;
            this.clearBtn.Text = "Clear";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(12, 110);
            this.logTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.Size = new System.Drawing.Size(625, 275);
            this.logTextBox.TabIndex = 10;
            // 
            // sendTextBox
            // 
            this.sendTextBox.Location = new System.Drawing.Point(12, 23);
            this.sendTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sendTextBox.Name = "sendTextBox";
            this.sendTextBox.Size = new System.Drawing.Size(213, 22);
            this.sendTextBox.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(679, 553);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sum Calculator RPC Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.Button disconnectBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.TextBox sendTextBox;
        private System.Windows.Forms.Button clearBtn;
        private System.Windows.Forms.TextBox txtIP;
    }
}

