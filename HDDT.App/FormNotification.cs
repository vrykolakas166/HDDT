using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HDDT.App
{
    public partial class FormNotification : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
        // Copied from dwmapi.h
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                          DWMWINDOWATTRIBUTE attribute,
                                                          ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                          uint cbAttribute);

        public FormNotification(Form parent)
        {
            InitializeComponent();

            var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute(this.Handle, attribute, ref preference, sizeof(uint));

            Location = new Point(parent.Location.X + 30, parent.Location.Y + 200);
            Size = new Size(200, 40);
        }

        public ThongBao Notification { get; set; }

        private void FormNotification_Load(object sender, EventArgs e)
        {
            LoadNotification();
        }

        private void LoadNotification()
        {
            flow.Controls.Clear();
            if (Notification != null)
            {
                Label message = new Label
                {
                    Text = Notification.Content,
                    Font = new Font("SF Pro text", 8.25f, FontStyle.Bold),
                    AutoSize = true,
                };
                Label timeline = new Label
                {
                    Text = Notification.CreatedAt.ToString("hh:MM:ss dd/MM/yyyy"),
                    Font = new Font("SF Pro text", 7)
                };
                flow.Controls.Add(message);
                flow.Controls.Add(timeline);
            }
            else
            {
                Label message = new Label
                {
                    Text = "Không có thông báo mới.",
                    Font = new Font("SF Pro text", 8.25f, FontStyle.Bold),
                    AutoSize = true,
                };
                flow.Controls.Add(message);
            }
        }

        private void FormNotification_Deactivate(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
