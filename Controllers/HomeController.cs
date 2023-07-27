using Microsoft.AspNetCore.Http;
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
            if (nguoithaotac == null)
            {
                throw new Exception();
            }
            string? tenOP = nguoithaotac?.Name;
            return Json(tenOP);
        }

        public async Task<IActionResult> CheckingVitri(string vitri)
        {
            try
            {
                string[] text = vitri.Split(';');
                string model = text[0].Trim();
                string cell = text[1].Trim();
                string station = text[2].Trim();
                string ten = text[3].Trim();
                var dulieuvitri = await db.Layouts.Where(x => (x.Model == model && x.Cell == cell && x.Station == station && x.PhanLoai == ten)).ToListAsync();
                return Json(dulieuvitri);
            }
            catch
            {
                throw new Exception();
            }
        }

        public async Task<IActionResult> CheckingVattuthuve(string vattuthuve)
        {
            try
            {
                string[] text = vattuthuve.Split(';');
                string tenvattuthuve = text[0].Trim();
                TempData["tenvattuthuve"] = tenvattuthuve;
                var dulieu = await db.DataRules.Where(x => x.BarcodeTen == tenvattuthuve).FirstOrDefaultAsync();
                if (dulieu != null)
                {
                    return Json(dulieu);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                throw new Exception();
            }
        }

        [HttpPost]
        public IActionResult updateStatus(string status, string thoigianbaoloi, string model, string cell, string station, string phanloai)
        {
            try
            {
                var rowToUpdate = db.Layouts.FirstOrDefault(s => (s.Model == model && s.Cell == cell && s.Station == station && s.PhanLoai == phanloai));
                if (rowToUpdate != null)
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
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                throw new Exception();
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
                return Json(new { success = lichsu });
            }
            catch
            {
                return Json(new { error = "Thêm dữ liệu NG vào bảng lịch sử không thành công!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckingVattuphatra(string mavattuphatra, string status, string model, string cell, string station,
            string phanloai, string thoigianbaoloi, string nguoixuat, string vitri, string result)
        {
            try
            {
                string[] text = mavattuphatra.Split(';');
                string tenvattuphatra = text[0].Trim();
                string tenvattuthuve = TempData["tenvattuthuve"].ToString();
                if (tenvattuthuve != tenvattuphatra) //TRƯỜNG HỢP NG
                {
                    //Cập nhật bảng layout
                    var rowToUpdate = db.Layouts.FirstOrDefault(s => (s.Model == model && s.Cell == cell && s.Station == station && s.PhanLoai == phanloai));
                    rowToUpdate.TrangThai = status;
                    rowToUpdate.ThoiGianBao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    db.Entry(rowToUpdate).State = EntityState.Modified;
                    //Thêm dữ liệu NG vào bàng Líchuxuatline
                    Lichsuxuatline lichsuNG = new Lichsuxuatline();
                    lichsuNG.Tenvattuthuve = tenvattuthuve;
                    lichsuNG.Tenvattuphatra = tenvattuphatra;
                    lichsuNG.Soluongxuatline = Convert.ToDecimal(0);
                    lichsuNG.Ngayxuat = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    lichsuNG.Nguoixuat = nguoixuat;
                    lichsuNG.Donvi = "";
                    lichsuNG.Tontu = Convert.ToDecimal(0);
                    lichsuNG.Vitri = vitri;
                    lichsuNG.Result = result;
                    lichsuNG.Lotno = "";
                    lichsuNG.Ghichu = "";
                    await db.AddAsync(lichsuNG);
                    await db.SaveChangesAsync();
                    var ketqua = new { mode = "NG", updatelayout = rowToUpdate, addLichsuxuatline = lichsuNG };
                    return Json(ketqua);
                }
                else //TRƯỜNG HỢP OK
                {
                    return Json(null);
                }
            }
            catch
            {
                throw new Exception();
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