namespace WindowsFormsApp1
{
	partial class Form1
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
			System.Windows.Forms.ColumnHeader columnHeader1;
			this.listView1 = new System.Windows.Forms.ListView();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.menu1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.AllowDrop = true;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
			this.listView1.Location = new System.Drawing.Point(12, 27);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(760, 210);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListView1_DragDrop);
			this.listView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.ListView1_DragEnter);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu1ToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(784, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// menu1ToolStripMenuItem
			// 
			this.menu1ToolStripMenuItem.Name = "menu1ToolStripMenuItem";
			this.menu1ToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
			this.menu1ToolStripMenuItem.Text = "menu1";
			// 
			// columnHeader1
			// 
			columnHeader1.Width = 200;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Form1";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem menu1ToolStripMenuItem;
	}
}

