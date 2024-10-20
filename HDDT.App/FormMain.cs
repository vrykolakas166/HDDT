using System;
using System.ComponentModel;
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
            InitializeComponent();

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
            tsStatus.Text = "Hoàn thành !";
            Loading(false);
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var progress = new Progress<int>(percent =>
            {
                // Report progress to the BackgroundWorker
                _worker.ReportProgress(percent);
            });

            Func.Open(_templateFile, _dataFiles, progress);
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
                    tsProgress.Maximum = _dataFiles.Length;
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
                // Get the path to the folder where the application is running
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                // Create the file path by appending the file name
                string filePath = Path.Combine(appDirectory, "last_error.txt");
                // Write to the file
                File.WriteAllText(filePath, ex.Message);
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
    }
}
