using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YouTube_Downloader_1._0
{
    public partial class M_Form : Form
    {

        #region Private variables

        private const int cGrip = 16;      // Grip size (resize)
        public int ControlBarHeight { get; set; } = 50; //title bar height
        public Color ControlBarColor { get; set; } = Color.CornflowerBlue; //Control bar color
        public Color TopLineColor { get; set; } = Color.DarkOrchid;

        public string Title { get; set; } = "Title";
        public Point TitleLocation { get; set; } = new Point(10, 40);
        public Color TitleBackgroundColor { get; set; } = Color.SteelBlue;
        public Font TitleFont { get; set; } = new Font("Arial", 16);
        public Color TitleForeColor { get; set; } = Color.Black;

        // Control buttons (Close,min,max) Forecolor on mouseover/mouseLeave/default
        public Color ControlButtonsMouseOverForecolor { get; set; } = Color.Gray;
        public Color ControlButtonsMouseLeaveForecolor { get; set; } = Color.Black;
        public Color ControlButtonsForeColor { get; set; } = Color.Black;

        Button CloseButton = new Button();
        Button MinButton = new Button();
        Button MaxButton = new Button();
        #endregion

        public M_Form()
        {
            InitializeComponent();
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            AddCloseButton();
            AddMaximizeButton();
            AddMinimizeButton();
        }
      
        private void AddCloseButton()
        {
            CloseButton.Name = "CloseButton";
            CloseButton.Text = "X";
            CloseButton.ForeColor = ControlButtonsForeColor;
            CloseButton.Font = new Font(CloseButton.Font.FontFamily, 11, FontStyle.Bold);
            CloseButton.BackColor = Color.Transparent;
            CloseButton.FlatStyle = FlatStyle.Flat;
            CloseButton.FlatAppearance.BorderSize = 0;
            CloseButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            CloseButton.Size = new Size(25, 25);
            CloseButton.Click += CloseButton_Click;
            CloseButton.MouseEnter += CloseButton_MouseEnter;
            CloseButton.MouseLeave += CloseButton_MouseLeave;
            Controls.Add(CloseButton);
        }
        private void CloseButton_MouseLeave(object sender, EventArgs e)
        {
            CloseButton.ForeColor = ControlButtonsMouseLeaveForecolor;
        }
        private void CloseButton_MouseEnter(object sender, EventArgs e)
        {
            CloseButton.ForeColor = ControlButtonsMouseOverForecolor;
        }
        private void AddMaximizeButton()
        {
            MaxButton.Name = "MaximizeButton";
            MaxButton.Text = "▀";
            MaxButton.ForeColor = ControlButtonsForeColor;
            MaxButton.Font = new Font(MaxButton.Font.FontFamily, 11, FontStyle.Bold);
            MaxButton.BackColor = Color.Transparent;
            MaxButton.FlatStyle = FlatStyle.Flat;
            MaxButton.FlatAppearance.BorderSize = 0;
            MaxButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            MaxButton.Size = new Size(25, 25);
            MaxButton.Click += MaxButton_Click;
            MaxButton.MouseLeave += MaxButton_MouseLeave;
            MaxButton.MouseEnter += MaxButton_MouseEnter;
            Controls.Add(MaxButton);
        }
        private void MaxButton_MouseEnter(object sender, EventArgs e)
        {
            MaxButton.ForeColor = ControlButtonsMouseOverForecolor;
        }
        private void MaxButton_MouseLeave(object sender, EventArgs e)
        {
            MaxButton.ForeColor = ControlButtonsMouseLeaveForecolor;
        }
        private void AddMinimizeButton()
        {
            MinButton.Name = "MaximizeButton";
            MinButton.Text = "‗";
            MinButton.ForeColor = ControlButtonsForeColor;
            MinButton.Font = new Font(MinButton.Font.FontFamily, 11, FontStyle.Bold);
            MinButton.BackColor = Color.Transparent;
            MinButton.FlatStyle = FlatStyle.Flat;
            MinButton.FlatAppearance.BorderSize = 0;
            MinButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            MinButton.Size = new Size(25, 25);
            MinButton.Click += MinButton_Click;
            MinButton.MouseEnter += MinButton_MouseEnter;
            MinButton.MouseLeave += MinButton_MouseLeave;
            Controls.Add(MinButton);
        }

        private void MinButton_MouseLeave(object sender, EventArgs e)
        {
            MinButton.ForeColor = ControlButtonsMouseLeaveForecolor;
        }
        private void MinButton_MouseEnter(object sender, EventArgs e)
        {
            MinButton.ForeColor = ControlButtonsMouseOverForecolor;
        }
        private void MinButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        private void MaxButton_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized) WindowState = FormWindowState.Normal;
            else WindowState = FormWindowState.Maximized;
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip, this.ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);

            rc = new Rectangle(0, 0, this.ClientSize.Width, ControlBarHeight);
            e.Graphics.DrawLine(new Pen(TopLineColor, 2), 0, 1, Width, 1); // Colored line above the control bar
            e.Graphics.FillRectangle(new SolidBrush(ControlBarColor), 0, 2, Width, 25); // Control bar
            e.Graphics.FillRectangle(new SolidBrush(TitleBackgroundColor), 0, 26, Width, ControlBarHeight); // Filling the title background color
            e.Graphics.DrawString(Title, TitleFont, new SolidBrush(TitleForeColor), TitleLocation); // The Title String!

            CloseButton.Location = new Point(Width - 26, 2);
            MaxButton.Location = new Point(Width - 52, 2);
            MinButton.Location = new Point(Width - 78, 2);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                pos = this.PointToClient(pos);
                if (pos.Y < ControlBarHeight)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

    }
}
