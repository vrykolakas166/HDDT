﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HDDT.App
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static async void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (await CheckUpdate())
            {
                // update
                Update();
                Environment.Exit(0);
            }
            else
            {
                Application.Run(new FormMain());
            }
        }

        private static string GetCurrentVersion()
        {
            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();
            // Get the version information
            var version = assembly.GetName().Version;
            return version.ToString(); // Returns a string representation of the version
        }

        static async Task<bool> CheckUpdate()
        {
            const string versionUrl = "https://raw.githubusercontent.com/vrykolakas166/HDDT/master/Build/version.txt";
            string versionInfo;
            using (HttpClient client = new HttpClient())
            {
                versionInfo = (await client.GetStringAsync(versionUrl)).Trim();
            }

            if (!string.IsNullOrEmpty(versionInfo))
            {
                string version = ExtractVersion();
                DateTime date = ExtractDate();
                string author = ExtractAuthor();
                string notes = ExtractNotes();

                if (GetCurrentVersion() != version)
                {
                    MessageBox.Show("Có phiên bản mới.", "Cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }

            return false;

            #region nested_func
            string ExtractVersion()
            {
                var match = Regex.Match(versionInfo, @"Version:\s*(\S+)");
                return match.Success ? match.Groups[1].Value : "Unknown";
            }

            DateTime ExtractDate()
            {
                var match = Regex.Match(versionInfo, @"Date:\s*(\d{4}-\d{2}-\d{2})");
                return match.Success ? DateTime.Parse(match.Groups[1].Value) : DateTime.MinValue;
            }

            string ExtractAuthor()
            {
                var match = Regex.Match(versionInfo, @"Author:\s*(.+)");
                return match.Success ? match.Groups[1].Value : "Unknown";
            }

            string ExtractNotes()
            {
                var match = Regex.Match(versionInfo, @"Notes:\s*(.+)");
                return match.Success ? match.Groups[1].Value : "No notes available";
            }
            #endregion
        }

        static void Update()
        {
            const string appUrl = "https://raw.githubusercontent.com/vrykolakas166/HDDT/master/Build/HDDT.App.exe";

            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\HDDT.App.exe";

                // Prepare the PowerShell command with a 3-second delay, download, and launch
                string command = $@"
                    Start-Sleep -Seconds 5; 
                    Invoke-WebRequest -Uri '{appUrl}' -OutFile '{outputPath}'; 
                    if (Test-Path '{outputPath}') {{ Start-Process -FilePath '{outputPath}' }} 
                    else {{ Write-Host 'Download failed. File not found.' }}";

                // Create a new process to run PowerShell
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
