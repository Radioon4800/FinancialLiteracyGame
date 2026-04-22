namespace FinancialLiteracyGame
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbStats = new System.Windows.Forms.GroupBox();
            this.gbProfession = new System.Windows.Forms.GroupBox();
            this.panel = new System.Windows.Forms.Panel();
            this.lblDiceResult = new System.Windows.Forms.Label();
            this.Action = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.LeftSide = new System.Windows.Forms.Panel();
            this.btnNewGame_Click = new System.Windows.Forms.Button();
            this.panel.SuspendLayout();
            this.Action.SuspendLayout();
            this.LeftSide.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbStats
            // 
            this.gbStats.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbStats.Location = new System.Drawing.Point(0, 0);
            this.gbStats.Name = "gbStats";
            this.gbStats.Size = new System.Drawing.Size(214, 205);
            this.gbStats.TabIndex = 0;
            this.gbStats.TabStop = false;
            this.gbStats.Text = "Статистика";
            // 
            // gbProfession
            // 
            this.gbProfession.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbProfession.Location = new System.Drawing.Point(0, 208);
            this.gbProfession.Name = "gbProfession";
            this.gbProfession.Size = new System.Drawing.Size(214, 124);
            this.gbProfession.TabIndex = 1;
            this.gbProfession.TabStop = false;
            this.gbProfession.Text = "Профессия";
            // 
            // panel
            // 
            this.panel.Controls.Add(this.lblDiceResult);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(1064, 450);
            this.panel.TabIndex = 2;
            // 
            // lblDiceResult
            // 
            this.lblDiceResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDiceResult.AutoSize = true;
            this.lblDiceResult.Location = new System.Drawing.Point(388, 13);
            this.lblDiceResult.Name = "lblDiceResult";
            this.lblDiceResult.Size = new System.Drawing.Size(0, 13);
            this.lblDiceResult.TabIndex = 0;
            // 
            // Action
            // 
            this.Action.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Action.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Action.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Action.Controls.Add(this.btnNewGame_Click);
            this.Action.Controls.Add(this.button3);
            this.Action.Controls.Add(this.button2);
            this.Action.Controls.Add(this.button1);
            this.Action.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Action.Location = new System.Drawing.Point(0, 350);
            this.Action.Name = "Action";
            this.Action.Size = new System.Drawing.Size(1064, 100);
            this.Action.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.AutoSize = true;
            this.button3.Dock = System.Windows.Forms.DockStyle.Left;
            this.button3.Location = new System.Drawing.Point(533, 0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(264, 96);
            this.button3.TabIndex = 2;
            this.button3.Text = "Взять кредит";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.AutoSize = true;
            this.button2.Dock = System.Windows.Forms.DockStyle.Left;
            this.button2.Location = new System.Drawing.Point(270, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(263, 96);
            this.button2.TabIndex = 1;
            this.button2.Text = "Купить актив";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Dock = System.Windows.Forms.DockStyle.Left;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(270, 96);
            this.button1.TabIndex = 0;
            this.button1.Text = "Бросить кубик";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LeftSide
            // 
            this.LeftSide.Controls.Add(this.gbStats);
            this.LeftSide.Controls.Add(this.gbProfession);
            this.LeftSide.Location = new System.Drawing.Point(12, 13);
            this.LeftSide.Name = "LeftSide";
            this.LeftSide.Size = new System.Drawing.Size(214, 332);
            this.LeftSide.TabIndex = 3;
            // 
            // btnNewGame_Click
            // 
            this.btnNewGame_Click.AutoSize = true;
            this.btnNewGame_Click.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnNewGame_Click.Location = new System.Drawing.Point(797, 0);
            this.btnNewGame_Click.Name = "btnNewGame_Click";
            this.btnNewGame_Click.Size = new System.Drawing.Size(264, 96);
            this.btnNewGame_Click.TabIndex = 3;
            this.btnNewGame_Click.Text = "Новая игра";
            this.btnNewGame_Click.UseVisualStyleBackColor = true;
            this.btnNewGame_Click.Click += new System.EventHandler(this.btnNewGame_Click_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1064, 450);
            this.Controls.Add(this.Action);
            this.Controls.Add(this.LeftSide);
            this.Controls.Add(this.panel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.Action.ResumeLayout(false);
            this.Action.PerformLayout();
            this.LeftSide.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbStats;
        private System.Windows.Forms.GroupBox gbProfession;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Panel Action;
        private System.Windows.Forms.Panel LeftSide;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblDiceResult;
        private System.Windows.Forms.Button btnNewGame_Click;
    }
}

