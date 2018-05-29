namespace WindowsFormsApp1
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelListening = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.labelControllerErrors = new System.Windows.Forms.Label();
            this.labelLastRequestDateTime = new System.Windows.Forms.Label();
            this.labelRequestsCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelListening);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.labelControllerErrors);
            this.groupBox1.Controls.Add(this.labelLastRequestDateTime);
            this.groupBox1.Controls.Add(this.labelRequestsCount);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(302, 93);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Статистика ";
            // 
            // labelListening
            // 
            this.labelListening.AutoSize = true;
            this.labelListening.ForeColor = System.Drawing.Color.Green;
            this.labelListening.Location = new System.Drawing.Point(162, 18);
            this.labelListening.Name = "labelListening";
            this.labelListening.Size = new System.Drawing.Size(22, 13);
            this.labelListening.TabIndex = 7;
            this.labelListening.Text = "Да";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(134, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Ожидаем запросы к API:";
            // 
            // labelControllerErrors
            // 
            this.labelControllerErrors.AutoSize = true;
            this.labelControllerErrors.Location = new System.Drawing.Point(161, 69);
            this.labelControllerErrors.Name = "labelControllerErrors";
            this.labelControllerErrors.Size = new System.Drawing.Size(13, 13);
            this.labelControllerErrors.TabIndex = 5;
            this.labelControllerErrors.Text = "0";
            // 
            // labelLastRequestDateTime
            // 
            this.labelLastRequestDateTime.AutoSize = true;
            this.labelLastRequestDateTime.Location = new System.Drawing.Point(161, 52);
            this.labelLastRequestDateTime.Name = "labelLastRequestDateTime";
            this.labelLastRequestDateTime.Size = new System.Drawing.Size(124, 13);
            this.labelLastRequestDateTime.TabIndex = 4;
            this.labelLastRequestDateTime.Text = "Запросов еще не было";
            // 
            // labelRequestsCount
            // 
            this.labelRequestsCount.AutoSize = true;
            this.labelRequestsCount.Location = new System.Drawing.Point(162, 35);
            this.labelRequestsCount.Name = "labelRequestsCount";
            this.labelRequestsCount.Size = new System.Drawing.Size(13, 13);
            this.labelRequestsCount.TabIndex = 3;
            this.labelRequestsCount.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Ошибок контроллера:";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Время последнего запроса:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Всего получено запросов:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 101);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Прокси к системе пропуска v1.2";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelControllerErrors;
        private System.Windows.Forms.Label labelLastRequestDateTime;
        private System.Windows.Forms.Label labelRequestsCount;
        private System.Windows.Forms.Label labelListening;
        private System.Windows.Forms.Label label7;
    }
}

