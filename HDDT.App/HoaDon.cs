using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace HDDT.App
{
    public class HoaDon
    {
        [JsonProperty("STT")]
        public string STT { get; set; }
        [JsonProperty("Tính chất")]
        public string TinhChat { get; set; }
        [JsonProperty("Tên hàng hóa, dịch vụ")]
        public string TenHangHoaDichVu { get; set; }
        [JsonProperty("Đơn vị tính")]
        public string DonViTinh { get; set; }
        [JsonProperty("Số lượng")]
        public string SoLuong { get; set; }
        [JsonProperty("Đơn giá")]
        public string DonGia { get; set; }
        [JsonProperty("Chiết khấu")]
        public string ChietKhau { get; set; }
        [JsonProperty("Thuế suất")]
        public string ThueSuat { get; set; }
        [JsonProperty("Thành tiền")]
        public string ThanhTien { get; set; }
        [JsonProperty("Thành tiền chưa có thuế GTGT")]
        public string ThanhTienChuaCoThue { get; set; }

        public static List<HoaDon> GetListFromJson(string json)
        {
            var list = JsonConvert.DeserializeObject<List<HoaDon>>(json);

            GetTTCT(list);

            // remove item with ThanhTienChuaCoThue == null
            list = list
                .Where(item => !string.IsNullOrEmpty(item.ThanhTienChuaCoThue) || item.ThanhTienChuaCoThue?.Trim() != "0")
                .ToList();

            FormatNumber(list);

            return list;
        }

        private static void GetTTCT(List<HoaDon> list)
        {
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.ThanhTienChuaCoThue) && !string.IsNullOrEmpty(item.ThanhTien))
                {
                    item.ThanhTienChuaCoThue = item.ThanhTien;
                }
            }
        }

        private static void FormatNumber(List<HoaDon> list)
        {
            foreach (var item in list)
            {
                item.SoLuong = item.SoLuong?.Trim().Replace(".", "").Replace(",", ".");
                item.DonGia = item.DonGia?.Trim().Replace(".", "");
                item.ThanhTienChuaCoThue = item.ThanhTienChuaCoThue?.Trim().Replace(".", "");

                // thue suat
                if (string.IsNullOrEmpty(item.ThueSuat) || (item.ThueSuat?.Trim().Equals("0") ?? false) || (item.ThueSuat?.Trim().Equals("KCT") ?? false))
                {
                    item.ThueSuat = "0%";
                }
            }
        }
    }
}
