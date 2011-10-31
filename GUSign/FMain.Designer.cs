namespace GUSign
{
    partial class FMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
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
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.bCertExport = new System.Windows.Forms.Button();
            this.bSign = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bCertExport
            // 
            this.bCertExport.Location = new System.Drawing.Point(12, 12);
            this.bCertExport.Name = "bCertExport";
            this.bCertExport.Size = new System.Drawing.Size(82, 23);
            this.bCertExport.TabIndex = 0;
            this.bCertExport.Text = "Сертификат";
            this.bCertExport.UseVisualStyleBackColor = true;
            this.bCertExport.Click += new System.EventHandler(this.bCertExport_Click);
            // 
            // bSign
            // 
            this.bSign.Location = new System.Drawing.Point(100, 12);
            this.bSign.Name = "bSign";
            this.bSign.Size = new System.Drawing.Size(82, 23);
            this.bSign.TabIndex = 1;
            this.bSign.Text = "Подпись";
            this.bSign.UseVisualStyleBackColor = true;
            this.bSign.Click += new System.EventHandler(this.bSign_Click);
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 47);
            this.Controls.Add(this.bSign);
            this.Controls.Add(this.bCertExport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Электронная подпись";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bCertExport;
        private System.Windows.Forms.Button bSign;
    }
}

