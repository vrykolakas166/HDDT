using DocumentFormat.OpenXml.InkML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new FormMain());
        }

        private static string GetCurrentVersion()
        {
            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();
            // Get the version information
            var version = assembly.GetName().Version;
            return version.ToString(); // Returns a string representation of the version
        }

        static async void CheckUpdate(string currentVersion)
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

                if(GetCurrentVersion() != version)
                {
                    MessageBox.Show("Có phiên bản mới.", "Cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

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
    }
}
