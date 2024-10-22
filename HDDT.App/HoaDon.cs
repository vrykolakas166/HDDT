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
        [JsonProperty("Thành tiền chưa có thuế GTGT")]
        public string ThanhTienChuaCoThue { get; set; }

        public static List<HoaDon> GetListFromJson(string json)
        {
            var list = JsonConvert.DeserializeObject<List<HoaDon>>(json);

            RemoveDot(list);

            // remove item with ThanhTienChuaCoThue == null
            list = list.Where(item => !string.IsNullOrEmpty(item.ThanhTienChuaCoThue)).ToList();

            return list;
        }

        private static void RemoveDot(List<HoaDon> list)
        {
            foreach (var item in list)
            {
                item.SoLuong = item.SoLuong.Trim().Replace(".", "").Replace(",", ".");
                item.DonGia = item.DonGia.Trim().Replace(".", "");
                item.ThanhTienChuaCoThue = item.ThanhTienChuaCoThue.Trim().Replace(".", "");
            }
        }
    }
}
