namespace FinancialLiteracyGame
{
    partial class ProfessionForm
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
            this.listProfessions = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblStartCash = new System.Windows.Forms.Label();
            this.lblStartExpenses = new System.Windows.Forms.Label();
            this.lblStartSalary = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listProfessions
            // 
            this.listProfessions.Dock = System.Windows.Forms.DockStyle.Left;
            this.listProfessions.FormattingEnabled = true;
            this.listProfessions.Location = new System.Drawing.Point(0, 0);
            this.listProfessions.Name = "listProfessions";
            this.listProfessions.Size = new System.Drawing.Size(120, 450);
            this.listProfessions.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(126, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(160, 149);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDescription.Location = new System.Drawing.Point(126, 164);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(0, 13);
            this.lblDescription.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblStartCash);
            this.groupBox1.Controls.Add(this.lblStartExpenses);
            this.groupBox1.Controls.Add(this.lblStartSalary);
            this.groupBox1.Location = new System.Drawing.Point(292, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 149);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Начальные финансы";
            // 
            // lblStartCash
            // 
            this.lblStartCash.AutoSize = true;
            this.lblStartCash.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStartCash.Location = new System.Drawing.Point(6, 88);
            this.lblStartCash.Name = "lblStartCash";
            this.lblStartCash.Size = new System.Drawing.Size(114, 31);
            this.lblStartCash.TabIndex = 2;
            this.lblStartCash.Text = "Баланс:";
            // 
            // lblStartExpenses
            // 
            this.lblStartExpenses.AutoSize = true;
            this.lblStartExpenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStartExpenses.Location = new System.Drawing.Point(6, 57);
            this.lblStartExpenses.Name = "lblStartExpenses";
            this.lblStartExpenses.Size = new System.Drawing.Size(132, 31);
            this.lblStartExpenses.TabIndex = 1;
            this.lblStartExpenses.Text = "Расходы:";
            // 
            // lblStartSalary
            // 
            this.lblStartSalary.AutoSize = true;
            this.lblStartSalary.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStartSalary.Location = new System.Drawing.Point(6, 26);
            this.lblStartSalary.Name = "lblStartSalary";
            this.lblStartSalary.Size = new System.Drawing.Size(141, 31);
            this.lblStartSalary.TabIndex = 0;
            this.lblStartSalary.Text = "Зарплата:";
            // 
            // txtName
            // 
            this.txtName.AcceptsTab = true;
            this.txtName.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtName.Location = new System.Drawing.Point(0, 0);
            this.txtName.Multiline = true;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(481, 59);
            this.txtName.TabIndex = 4;
            this.txtName.WordWrap = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Controls.Add(this.txtName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(120, 391);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(944, 59);
            this.panel1.TabIndex = 5;
            // 
            // btnStart
            // 
            this.btnStart.AutoSize = true;
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnStart.Location = new System.Drawing.Point(487, 0);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(457, 59);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Начать";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // ProfessionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1064, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.listProfessions);
            this.Name = "ProfessionForm";
            this.Text = "Выбор профессии";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listProfessions;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblStartCash;
        private System.Windows.Forms.Label lblStartExpenses;
        private System.Windows.Forms.Label lblStartSalary;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnStart;
    }
}