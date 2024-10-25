using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HDDT.App
{
    public static class Func
    {
        /// <summary>
        /// H -> 4 -> I
        /// "Tên hàng hóa, dịch vụ": "Vỏ hộp offset 8305",
        /// "Đơn vị tính": "Cái",
        /// "Số lượng": "1.850",
        /// "Đơn giá": "16.280",
        /// 
        /// I -> 1 -> J
        /// "Thuế suất": "8%",
        /// 
        /// "Đơn giá": -> "Thành tiền chưa có thuế GTGT": "30.118.000" / "Số lượng": "1.850",
        /// Tổng tiền thuế = "Thuế suất": "8%" * "Thành tiền chưa có thuế GTGT"
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <param name="progress"></param>
        public static void Open(FileInfo template, FileInfo[] data, IProgress<int> progress)
        {
            // Open the Excel file
            using (var workbook = new XLWorkbook(template.FullName))
            {
                var worksheet = workbook.Worksheet(1); // select first sheet

                // add Thuế suất
                worksheet.Column("J").InsertColumnsBefore(1);

                // add
                // "Tên hàng hóa, dịch vụ"
                // "Đơn vị tính"
                // "Số lượng"
                // "Đơn giá"
                worksheet.Column("I").InsertColumnsBefore(4);

                // summary: I,J,K,L,M,N,O
                // I: "Tên hàng hóa, dịch vụ"
                // J: "Đơn vị tính"
                // K: "Số lượng"
                // L: "Đơn giá"
                //
                // M: Tổng tiền chưa thuế
                // N: Thuế suất
                // O: Tổng tiền thuế
                worksheet.Cell("I6").Value = "Tên hàng hóa, dịch vụ";
                worksheet.Cell("J6").Value = "Đơn vị tính";
                worksheet.Cell("K6").Value = "Số lượng";
                worksheet.Cell("L6").Value = "Đơn giá";

                worksheet.Cell("N6").Value = "Thuế suất";

                // row 6 : header
                // row 7: first row
                // so hoa don: column D

                // so hoa don - danh sach 
                Dictionary<Invoice, List<HoaDon>> dic = GetHD(data);

                // Specify the row you want to copy
                int originalRowNumberStart = 7;
                int lastRowInserted = originalRowNumberStart;

                for (int x = 0; x < dic.Count; x++)
                {
                    if (x == dic.Count - 1)
                    {
                        progress.Report(100);
                    }
                    else
                    {
                        progress.Report(Convert.ToInt32(Math.Round((x + 1) * 100.0 / dic.Count)));
                    }

                    // c,d,e,f
                    //var symbol = worksheet.Cell($"C{originalRowNumberStart}").Value.ToString();
                    //var number = worksheet.Cell($"D{originalRowNumberStart}").Value.ToString();
                    //var date = worksheet.Cell($"E{originalRowNumberStart}").Value.ToString();
                    //var mst = worksheet.Cell($"F{originalRowNumberStart}").Value.ToString();

                    var invoice = new Invoice()
                    {
                        Symbol = worksheet.Cell($"C{originalRowNumberStart}").Value.ToString().Trim(),
                        Number = worksheet.Cell($"D{originalRowNumberStart}").Value.ToString().Trim(),
                        Date = worksheet.Cell($"E{originalRowNumberStart}").Value.ToString().Trim().Replace("/", ""),
                        Information = worksheet.Cell($"F{originalRowNumberStart}").Value.ToString().Trim(),
                    };

                    if (dic.TryGetValue(invoice, out var dshd))
                    {
                        for (int i = 0; i < dshd.Count; i++)
                        {
                            if (i != 0)
                            {
                                // Calculate the target row where the copied content will be inserted
                                int newRowNumber = originalRowNumberStart + i;

                                // Insert a new row at the position
                                worksheet.Row(newRowNumber).InsertRowsAbove(1);

                                // Copy the original row to the new row
                                worksheet.Row(originalRowNumberStart).CopyTo(worksheet.Row(newRowNumber));

                                lastRowInserted = newRowNumber;

                                worksheet.Cell($"I{lastRowInserted}").Value = dshd[i].TenHangHoaDichVu;
                                worksheet.Cell($"J{lastRowInserted}").Value = dshd[i].DonViTinh;
                                worksheet.Cell($"K{lastRowInserted}").Value = dshd[i].SoLuong;
                                worksheet.Cell($"L{lastRowInserted}").Value = dshd[i].DonGia;
                                worksheet.Cell($"M{lastRowInserted}").Value = dshd[i].TenHangHoaDichVu.Contains("Chiết khấu") ? "-" + dshd[i].ThanhTienChuaCoThue : dshd[i].ThanhTienChuaCoThue;
                                worksheet.Cell($"N{lastRowInserted}").Value = dshd[i].ThueSuat;
                            }
                            else
                            {
                                worksheet.Cell($"I{originalRowNumberStart}").Value = dshd[i].TenHangHoaDichVu;
                                worksheet.Cell($"J{originalRowNumberStart}").Value = dshd[i].DonViTinh;
                                worksheet.Cell($"K{originalRowNumberStart}").Value = dshd[i].SoLuong;
                                worksheet.Cell($"L{originalRowNumberStart}").Value = dshd[i].DonGia;
                                worksheet.Cell($"M{originalRowNumberStart}").Value = dshd[i].TenHangHoaDichVu.Contains("Chiết khấu") ? "-" + dshd[i].ThanhTienChuaCoThue : dshd[i].ThanhTienChuaCoThue;
                                worksheet.Cell($"N{originalRowNumberStart}").Value = dshd[i].ThueSuat;

                                lastRowInserted = originalRowNumberStart;
                            }
                        }

                        originalRowNumberStart = lastRowInserted + 1;
                    }
                    else
                    {
                        originalRowNumberStart++;
                        lastRowInserted = originalRowNumberStart;
                    }

                }

                // Save to the same folder
                workbook.SaveAs($"{template.Directory.FullName}\\output.xlsx");
            }
        }

        private static Dictionary<Invoice, List<HoaDon>> GetHD(FileInfo[] data)
        {
            Dictionary<Invoice, List<HoaDon>> dic = new Dictionary<Invoice, List<HoaDon>>();
            foreach (var item in data)
            {
                var temp = item.Name.Replace(item.Extension, "").Split('_');
                // data_null_null_null_null
                // data_symbol_number_date_info
                var invoice = new Invoice()
                {
                    Symbol = temp[1],
                    Number = temp[2],
                    Date = temp[3],
                    Information = temp[4]
                };

                var hd = HoaDon.GetListFromJson(File.ReadAllText(item.FullName));
                dic.Add(invoice, hd);
            }

            return dic;
        }

        public static async Task DownloadExtensionAsync()
        {

            try
            {
                // Show folder browser dialog
                using (var folderBrowser = new FolderBrowserDialog())
                {
                    folderBrowser.Description = "Select a folder to save the files:";
                    if (folderBrowser.ShowDialog() == DialogResult.OK)
                    {
                        string selectedPath = folderBrowser.SelectedPath;
                        string folderPath = Path.Combine(selectedPath, "HDDTDataExtension");

                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, true);
                        }

                        Directory.CreateDirectory(folderPath);

                        // Call the download method with the selected path
                        HttpClient client = new HttpClient();

                        var owner = "vrykolakas166";
                        var repo = "HDDT";
                        var path = "Build/HDDTData";
                        var branch = "master";

                        string url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}?ref={branch}";

                        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HDDT.App", Program.GetCurrentVersion()));

                        var response = await client.GetStringAsync(url);
                        var files = JArray.Parse(response);

                        foreach (var file in files)
                        {
                            var fileName = file["name"].ToString();
                            var downloadUrl = file["download_url"]?.ToString();
                            if (downloadUrl != null)
                            {
                                var fileContent = await client.GetStringAsync(downloadUrl);
                                File.WriteAllText(Path.Combine(folderPath, fileName), fileContent);
                            }
                            else
                            {
                                MyLogger.Error($"In DownloadExtensionAsync where fileName = {fileName} && downloadUrl = {downloadUrl}");
                            }
                        }

                        MessageBox.Show("Tải công cụ thành công.", "Xong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tải công cụ thất bại, vui lòng liên hệ nhà phát triển", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MyLogger.Error(ex.Message);
            }
        }
    }
}
