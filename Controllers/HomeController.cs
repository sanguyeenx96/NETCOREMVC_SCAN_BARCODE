using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebScanBarcode.Models;

namespace WebScanBarcode.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuanLyVatTuContext db;

        public HomeController(ILogger<HomeController> logger, QuanLyVatTuContext quanLyVatTuContext)
        {
            _logger = logger;
            db = quanLyVatTuContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Scan()
        {
            ViewData["page1"] = "Scan";
            return View();
        }

        public IActionResult XuatlinePD2()
        {
            ViewData["page1"] = "Scan";
            ViewData["page2"] = "Xuất line xưởng 2";

            return View();
        }

        public async Task<IActionResult> CheckingOP(string opId)
        {
            var nguoithaotac = await db.DataRuleTens.Where(x => x.Idname == opId).FirstOrDefaultAsync();
            string? tenOP = nguoithaotac?.Name;
            return Json(tenOP);
        }

        public async Task<IActionResult> CheckingVitri(string vitri)
        {
            string[] text = vitri.Split(';');
            string model = text[0].Trim();
            string cell = text[1].Trim();
            string station = text[2].Trim();
            string ten = text[3].Trim();
            var dulieuvitri = await db.Layouts.Where(x => (x.Model == model && x.Cell == cell && x.Station == station && x.PhanLoai == ten)).ToListAsync();
            return Json(dulieuvitri);
        }

        public async Task<IActionResult> CheckingVattuthuve(string vattuthuve)
        {
            string[] text = vattuthuve.Split(';');
            string tenvattuthuve = text[0].Trim();
            var dulieu = await db.DataRules.Where(x => x.BarcodeTen.Contains(tenvattuthuve)).FirstOrDefaultAsync();
            return Json(dulieu);
        }

        [HttpPost]
        public IActionResult updateStatus(string status, string thoigianbaoloi, string model, string cell, string station, string phanloai)
        {
            var rowToUpdate = db.Layouts.FirstOrDefault(s => (s.Model == model && s.Cell == cell && s.Station == station && s.PhanLoai == phanloai));
            if (rowToUpdate != null)
            {
                try
                {
                    rowToUpdate.TrangThai = status;
                    rowToUpdate.ThoiGianBao = thoigianbaoloi;
                    //Dòng mã "db.Entry(rowToUpdate).State = EntityState.Modified;"
                    //có nghĩa là bạn đang đánh dấu đối tượng "rowToUpdate" trong DbContext của bạn
                    //là đã được sửa đổi. Điều này cho phép DbContext biết rằng đối tượng này cần
                    //được cập nhật trong cơ sở dữ liệu khi SaveChanges() được gọi.
                    //EntityState.Modified là một giá trị của enum EntityState trong Entity Framework,
                    //nó chỉ định rằng đối tượng đã được sửa đổi và cần được cập nhật trong cơ sở dữ liệu.
                    db.Entry(rowToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(rowToUpdate);
                }
                catch
                {
                    return Json(new { error = "cập nhật trạng thái tại bảng Layout trong database không thành công!" });
                }
            }
            else
            {
                return Json(new { message = "Không tìm thấy dòng dữ liệu cần cập nhật" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Luulichsu_Xuatline(string tenvattuthuve, string tenvattuphatra, string soluongxuatline,
            string ngayxuat, string nguoixuat, string donvi, string tontu, string vitri, string result, string lotno,
            string ghichu)
        {
            try
            {
                Lichsuxuatline lichsu = new Lichsuxuatline();
                lichsu.Tenvattuthuve = tenvattuthuve;
                lichsu.Tenvattuphatra = tenvattuphatra;
                lichsu.Soluongxuatline = Convert.ToDecimal(soluongxuatline);
                lichsu.Ngayxuat = ngayxuat;
                lichsu.Nguoixuat = nguoixuat;
                lichsu.Donvi = donvi;
                lichsu.Tontu = Convert.ToDecimal(tontu);
                lichsu.Vitri = vitri;
                lichsu.Result = result;
                lichsu.Lotno = lotno;
                lichsu.Ghichu = ghichu;
                await db.AddAsync(lichsu);
                await db.SaveChangesAsync();
                return Json(new { success = "Thêm dữ liệu NG vào bảng lịch sử thành công!" });
            }
            catch
            {
                return Json(new { error = "Thêm dữ liệu NG vào bảng lịch sử không thành công!" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}