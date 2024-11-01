using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace HDDT.App
{
    public class MyLogger
    {
        static readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_error.log");

        public static void Error(string msg)
        {
            // Write to the file
            File.WriteAllText(_filePath, $"{DateTime.Now}\n" + msg);

        }

        /// <summary>
        /// https://app.elasticemail.com/marketing/logs/email
        /// </summary>
        public static async Task SendLogFileByEmail(Form parent = null)
        {
            try
            {
                // 
                if (!File.Exists(_filePath))
                {
                    using (new CenterWinDialog(parent)) MessageBox.Show("Ứng dụng có lỗi đ đâu mà báo.", "HEHE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string logFilePath = Path.GetTempPath() + "hddt_error.txt";
                File.Copy(_filePath, logFilePath);

                // Set email credentials
                string smtpServer = "smtp.elasticemail.com";
                int smtpPort = 2525;  // Port on https://app.elasticemail.com SMTP provider
                string senderEmail = "munndo166@gmail.com";
                string senderPassword = "173DD4654A53C89CDEAAF570E74033A23D98";
                string recipientEmail = "munndo166@gmail.com";  // Email to receive the logs

                // Create a MailMessage object
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(senderEmail)
                };
                mail.To.Add(recipientEmail);
                mail.Subject = "[HDDT] Lỗi phát sinh";
                mail.Body = "Xem chi tiết trong tệp tin đính kèm nhé.";

                // Attach the log file
                Attachment attachment = new Attachment(logFilePath);
                mail.Attachments.Add(attachment);

                // Set up the SMTP client
                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                // Send the email
                await smtpClient.SendMailAsync(mail);
                using (new CenterWinDialog(parent)) MessageBox.Show("Báo lỗi thành công.", "Xong", MessageBoxButtons.OK, MessageBoxIcon.Information);

                File.Delete(_filePath);
            }
            catch (Exception ex)
            {
                using (new CenterWinDialog(parent)) MessageBox.Show($"Báo lỗi thất bại, vui lòng liên hệ nhà phát triển", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Error(ex.ToString());
            }
        }
    }
}
