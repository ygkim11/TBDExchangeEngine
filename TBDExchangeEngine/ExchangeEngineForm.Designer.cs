namespace TBDExchangeEngine
{
    partial class ExchangeEngineForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExchangeEngineForm));
            this.axKHOpenAPI1 = new AxKHOpenAPILib.AxKHOpenAPI();
            this.userIdLabel = new System.Windows.Forms.Label();
            this.userNameLabel = new System.Windows.Forms.Label();
            this.serverTypeLabel = new System.Windows.Forms.Label();
            this.realtimeReadyLabel = new System.Windows.Forms.Label();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.accountLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.axKHOpenAPI1)).BeginInit();
            this.SuspendLayout();
            // 
            // axKHOpenAPI1
            // 
            this.axKHOpenAPI1.Enabled = true;
            this.axKHOpenAPI1.Location = new System.Drawing.Point(12, 12);
            this.axKHOpenAPI1.Margin = new System.Windows.Forms.Padding(1);
            this.axKHOpenAPI1.Name = "axKHOpenAPI1";
            this.axKHOpenAPI1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axKHOpenAPI1.OcxState")));
            this.axKHOpenAPI1.Size = new System.Drawing.Size(0, 0);
            this.axKHOpenAPI1.TabIndex = 0;
            // 
            // userIdLabel
            // 
            this.userIdLabel.AutoSize = true;
            this.userIdLabel.Location = new System.Drawing.Point(5, 10);
            this.userIdLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.userIdLabel.Name = "userIdLabel";
            this.userIdLabel.Size = new System.Drawing.Size(0, 12);
            this.userIdLabel.TabIndex = 1;
            // 
            // userNameLabel
            // 
            this.userNameLabel.AutoSize = true;
            this.userNameLabel.Location = new System.Drawing.Point(5, 36);
            this.userNameLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.userNameLabel.Name = "userNameLabel";
            this.userNameLabel.Size = new System.Drawing.Size(0, 12);
            this.userNameLabel.TabIndex = 2;
            // 
            // serverTypeLabel
            // 
            this.serverTypeLabel.AutoSize = true;
            this.serverTypeLabel.Location = new System.Drawing.Point(5, 61);
            this.serverTypeLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.serverTypeLabel.Name = "serverTypeLabel";
            this.serverTypeLabel.Size = new System.Drawing.Size(0, 12);
            this.serverTypeLabel.TabIndex = 3;
            // 
            // realtimeReadyLabel
            // 
            this.realtimeReadyLabel.AutoSize = true;
            this.realtimeReadyLabel.Location = new System.Drawing.Point(12, 267);
            this.realtimeReadyLabel.Name = "realtimeReadyLabel";
            this.realtimeReadyLabel.Size = new System.Drawing.Size(109, 12);
            this.realtimeReadyLabel.TabIndex = 4;
            this.realtimeReadyLabel.Text = "실시간 등록 준비...";
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(12, 117);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.Size = new System.Drawing.Size(351, 138);
            this.logTextBox.TabIndex = 5;
            this.logTextBox.Text = "";
            // 
            // accountLabel
            // 
            this.accountLabel.AutoSize = true;
            this.accountLabel.Location = new System.Drawing.Point(12, 102);
            this.accountLabel.Name = "accountLabel";
            this.accountLabel.Size = new System.Drawing.Size(57, 12);
            this.accountLabel.TabIndex = 6;
            this.accountLabel.Text = "계좌번호:";
            // 
            // ExchangeEngineForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(375, 288);
            this.Controls.Add(this.accountLabel);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.realtimeReadyLabel);
            this.Controls.Add(this.serverTypeLabel);
            this.Controls.Add(this.userNameLabel);
            this.Controls.Add(this.userIdLabel);
            this.Controls.Add(this.axKHOpenAPI1);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "ExchangeEngineForm";
            this.Text = "Exchange Engine";
            this.Load += new System.EventHandler(this.ExchangeEngineForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axKHOpenAPI1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        private System.Windows.Forms.Label userIdLabel;
        private System.Windows.Forms.Label userNameLabel;
        private System.Windows.Forms.Label serverTypeLabel;
        private System.Windows.Forms.Label realtimeReadyLabel;
        private System.Windows.Forms.RichTextBox logTextBox;
        private System.Windows.Forms.Label accountLabel;
    }
}

