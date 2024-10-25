using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HDDT.App
{
    public partial class FormMain : Form
    {
        private FileInfo _templateFile;
        private FileInfo[] _dataFiles = { };

        private readonly BackgroundWorker _worker;

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
                MessageBox.Show("Vui lòng liên hệ nhà phát triển", "Lỗi chưa xác định", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            catch(Exception ex)
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void reportErrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Loading(true);
            await MyLogger.SendLogFileByEmail();
            Loading(false);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("From your boyfriend with love.\n\n\t\tCopyright © 2024", "love you ttd.19", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
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
                MessageBox.Show("Vui lòng liên hệ nhà phát triển", "Lỗi không xác định", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MyLogger.Error(ex.Message);
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
    }
}
