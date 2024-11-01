using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HDDT.App
{
    public partial class FormMain : Form
    {
        private FileInfo _templateFile;
        private FileInfo[] _dataFiles = { };
        private FormNotification _formNotification;

        private readonly BackgroundWorker _worker;
        private readonly Thread _silentThread;

        public FormMain()
        {
            LoadEmbeddedFonts();
            InitializeComponent();
            ApplyEmbeddedFonts();

            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };

            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.ProgressChanged += _worker_ProgressChanged;

            _silentThread = new Thread(RunNotificationHub);
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                tsProgress.Value = e.ProgressPercentage;
                if (string.IsNullOrEmpty((string)(e.UserState ?? "")))
                {
                    tsStatus.Text = $"Tiến độ: {e.ProgressPercentage}%";
                }
                else
                {
                    tsStatus.Text = $"Tiến độ: {e.ProgressPercentage} - {e.UserState}";
                }
            }
            catch
            {
                tsProgress.Value = 0;
                tsStatus.Text = "Hủy.";
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MyLogger.Error(e.Error.Message);
                using (new CenterWinDialog(this)) MessageBox.Show("Vui lòng liên hệ nhà phát triển", "Lỗi chưa xác định", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tsStatus.Text = "Thất bại !";
            }
            else if (e.Cancelled)
            {
                // operation is cancelled
            }
            else
            {
                tsStatus.Text = "Hoàn thành !";
            }
            Loading(false);
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var progress = new Progress<int>(percent =>
                {
                    // Report progress to the BackgroundWorker
                    _worker.ReportProgress(percent);
                });

                Func.Open(_templateFile, _dataFiles, progress);
            }
            catch (Exception ex)
            {
                e.Result = ex;
                e.Cancel = true;
                throw;
            }
        }

        private void Loading(bool con)
        {
            if (con)
            {
                // start
                foreach (Control control in Controls)
                {
                    control.Invoke((MethodInvoker)delegate
                    {
                        control.Enabled = false;
                    });
                }
            }
            else
            {
                foreach (Control control in Controls)
                {
                    control.Invoke((MethodInvoker)delegate
                    {
                        control.Enabled = true;
                    });
                }
                // end
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            _silentThread.Start();
            _formNotification = new FormNotification(this);
            lblVersion.Text = $"Phiên bản: {Program.GetCurrentVersion()}";
        }

        private void btnSelectTemplate_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select a template",
                Multiselect = false,
                Filter = "Excel File (*.xls;*.xlsx)|*.xls;*.xlsx"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(ofd.FileName))
                {
                    _templateFile = new FileInfo(ofd.FileName);
                    txtTemplate.Text = _templateFile.Name;
                }
            }
        }

        private void btnSelectData_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select data files",
                Multiselect = true,
                Filter = "Data files (*.json)|*.json"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileNames.Length > 0)
                {
                    _dataFiles = new FileInfo[ofd.FileNames.Length];
                    for (int i = 0; i < ofd.FileNames.Length; i++)
                    {
                        _dataFiles[i] = new FileInfo(ofd.FileNames[i]);
                    }
                    txtData.Text = string.Join("; ", _dataFiles.Select(x => x.Name).ToArray());
                }
            }
        }

        private async void toolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Loading(true);
            await Func.DownloadExtensionAsync(this);
            Loading(false);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void reportErrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Loading(true);
            await MyLogger.SendLogFileByEmail(this);
            Loading(false);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this)) MessageBox.Show("From your boyfriend with love.\n\n\t\tCopyright © 2024", "love you ttd.19", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                tsStatus.Text = "";

                if (ValidateInput())
                {
                    var output = Path.GetDirectoryName(_templateFile.FullName) + "\\output.xlsx";
                    if (Func.IsFileInUse(output))
                    {
                        using (new CenterWinDialog(this)) MessageBox.Show("Vui lòng đóng tệp excel trước.", "Output", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Loading(true);
                    tsProgress.Maximum = 100;
                    _worker.RunWorkerAsync();
                }
                else
                {
                    tsStatus.Text = "Lỗi: Dữ liệu không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                using (new CenterWinDialog(this)) MessageBox.Show("Vui lòng liên hệ nhà phát triển", "Lỗi không xác định", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MyLogger.Error(ex.ToString());
            }
        }

        private bool ValidateInput()
        {
            if (_templateFile == null)
            {
                return false;
            }

            if (_dataFiles.Length == 0)
            {
                return false;
            }

            return true;
        }

        private void ApplyEmbeddedFonts()
        {
            btnRun.Font = new Font(boldFontFamily, 11.25F, FontStyle.Bold); // new System.Drawing.Font("SF Pro Text", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            txtData.Font = new Font(regularFontFamily, 9.75F, FontStyle.Regular); // new System.Drawing.Font("SF Pro Text", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            txtTemplate.Font = new Font(regularFontFamily, 9.75F, FontStyle.Regular);  // new System.Drawing.Font("SF Pro Text", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Font = new Font(regularFontFamily, 8.25F, FontStyle.Regular);  // new System.Drawing.Font("SF Pro Text", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

        private async void RunNotificationHub()
        {
            while (true)
            {
                // check update
                var newUpdate = await Program.CheckUpdate();

                // notify
                if (newUpdate)
                {
                    btnNotification.Invoke((MethodInvoker)delegate
                    {
                        btnNotification.BackgroundImage = Properties.Resources.bell_new;
                    });
                    _formNotification.Notification = new ThongBao()
                    {
                        Content = "Có phiên bản mới.",
                    };
                    break; // end thread
                }

                Debug.WriteLine("slient thread's running...");
                Thread.Sleep(5 * 60 * 1000); // 5 mins
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                if (_silentThread.IsAlive)
                {
                    _silentThread.Abort();
                }
                if (File.Exists(Path.GetTempPath() + "hddt_error.txt"))
                {
                    File.Delete(Path.GetTempPath() + "hddt_error.txt");
                }
            }
            catch { }
            base.OnClosed(e);
        }

        private void btnNotification_Click(object sender, EventArgs e)
        {
            this.btnNotification.BackgroundImage = Properties.Resources.bell;
            _formNotification.Show();
        }
    }
}
