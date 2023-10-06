using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebScanBarcode.Models;
using static System.Collections.Specialized.BitVector32;

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

        public static class MyStringStorage
        {
            public static string? ten_vattuthuve { get; set; }
            public static string? ma_vattuthuve { get; set; }
            public static string? ten_vattuphatra { get; set; }
            public static string? ma_vattuphatra { get; set; }
            public static string? st_model { get; set; }
            public static string? st_cell { get; set; }
            public static string? st_station { get; set; }
            public static string? st_thoigianphatsinhloi { get; set; }
            public static string? st_thoigiancaitien { get; set; }
            public static string? st_tinhtrangloi { get; set; }
            public static string? st_nguyennhanloi { get; set; }
            public static string? st_caitien { get; set; }
            public static string? st_damnhiem { get; set; }
            public static string? st_ghichu { get; set; }
            public static decimal st_soluong { get; set; }
            public static string? st_donvi { get; set; }
        }

        public async Task<IActionResult> CheckingOP(string opId)
        {
            MyStringStorage.ten_vattuthuve = "";
            MyStringStorage.ma_vattuthuve = "";
            MyStringStorage.ten_vattuphatra = "";
            MyStringStorage.ma_vattuphatra = "";

            MyStringStorage.st_model = "";
            MyStringStorage.st_cell = "";
            MyStringStorage.st_station = "";

            MyStringStorage.st_thoigianphatsinhloi = "";
            MyStringStorage.st_thoigiancaitien = "";
            MyStringStorage.st_tinhtrangloi = "";
            MyStringStorage.st_nguyennhanloi = "";
            MyStringStorage.st_caitien = "";
            MyStringStorage.st_damnhiem = "";
            MyStringStorage.st_ghichu = "";
            MyStringStorage.st_soluong = 0;
            MyStringStorage.st_donvi = "";
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
                if (cell == null) { cell = ""; }
                if (station == null) { station = ""; }
                var dulieuvitri = await db.Layouts.Where(x => (x.Model == model && x.Cell == cell && x.Station == station && x.PhanLoai == ten)).ToListAsync();
                if (dulieuvitri.Count == 0)
                {
                    throw new Exception();
                }
                else
                {
                    MyStringStorage.st_model = model;
                    MyStringStorage.st_cell = cell;
                    MyStringStorage.st_station = station;

                    return Json(dulieuvitri);
                }
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
                var dulieu = await db.DataRules.Where(x => x.BarcodeTen == tenvattuthuve).FirstOrDefaultAsync();
                MyStringStorage.ten_vattuthuve = dulieu.Ten.ToString();
                MyStringStorage.ma_vattuthuve = tenvattuthuve;
                var dulieu2 = await db.TonKhoPas.Where(x => (x.Ten == dulieu.Ten && x.GhiChu == "2")).FirstOrDefaultAsync();
                var laylotno = await db.Lichsunhaptus
                                            .Where(l => l.Tenvattu == dulieu.Ten)
                                            .OrderByDescending(l => l.Id)
                                            .Take(1)
                                            .FirstOrDefaultAsync();
                if (dulieu != null && dulieu2 != null && laylotno != null)
                {
                    var data = new { dulieu = dulieu, dulieu2 = dulieu2, dulieu3 = laylotno };
                    return Json(data);
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
                if (cell == null) { cell = ""; }
                if (station == null) { station = ""; }

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
            string phanloai, string thoigianbaoloi, string nguoixuat, string vitri, decimal soluongxuatline,
            string donvi, decimal tontu, string lotno)
        {
            try
            {
                if (cell == null) { cell = ""; }
                if (station == null) { station = ""; }
                string[] text = mavattuphatra.Split(';');
                string ma_vattuphatra = text[0].Trim();

                string ten_vattuthuve = MyStringStorage.ten_vattuthuve;
                string ma_vattuthuve = MyStringStorage.ma_vattuthuve;

                if (ma_vattuthuve != ma_vattuphatra) //TRƯỜNG HỢP NG
                {
                    //Cập nhật bảng layout
                    var rowToUpdate = db.Layouts.FirstOrDefault(s => (s.Model == model && s.Cell == cell && s.Station == station && s.PhanLoai == phanloai));
                    rowToUpdate.TrangThai = status;
                    rowToUpdate.ThoiGianBao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    db.Entry(rowToUpdate).State = EntityState.Modified;
                    //Thêm dữ liệu NG vào bàng Líchuxuatline
                    Lichsuxuatline lichsuNG = new Lichsuxuatline();
                    lichsuNG.Tenvattuthuve = ten_vattuthuve;
                    lichsuNG.Tenvattuphatra = ma_vattuphatra;
                    lichsuNG.Soluongxuatline = Convert.ToDecimal(0);
                    lichsuNG.Ngayxuat = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    lichsuNG.Nguoixuat = nguoixuat;
                    lichsuNG.Donvi = "";
                    lichsuNG.Tontu = Convert.ToDecimal(0);
                    lichsuNG.Vitri = vitri;
                    lichsuNG.Result = "NG";
                    lichsuNG.Lotno = "";
                    lichsuNG.Ghichu = "";
                    await db.AddAsync(lichsuNG);
                    await db.SaveChangesAsync();
                    var ketqua = new { mode = "NG", updatelayout = rowToUpdate, addLichsuxuatline = lichsuNG };
                    return Json(ketqua);
                }
                else //TRƯỜNG HỢP OK
                {
                    MyStringStorage.st_damnhiem = nguoixuat;
                    MyStringStorage.st_soluong = soluongxuatline;
                    MyStringStorage.st_donvi = donvi;
                    var dulieu = await db.DataRules.Where(x => x.BarcodeTen == ma_vattuphatra).FirstOrDefaultAsync();
                    MyStringStorage.ten_vattuphatra = dulieu.Ten.ToString();
                    MyStringStorage.ma_vattuphatra = ma_vattuphatra;
                    if (dulieu != null)
                    {
                        Lichsuxuatline lichsuOK = new Lichsuxuatline();
                        lichsuOK.Tenvattuthuve = ten_vattuthuve;
                        lichsuOK.Tenvattuphatra = MyStringStorage.ten_vattuphatra;
                        lichsuOK.Soluongxuatline = soluongxuatline;
                        lichsuOK.Ngayxuat = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        lichsuOK.Nguoixuat = nguoixuat;
                        lichsuOK.Donvi = donvi;
                        lichsuOK.Tontu = tontu;
                        lichsuOK.Vitri = vitri;
                        lichsuOK.Result = "OK";
                        lichsuOK.Lotno = lotno;
                        lichsuOK.Ghichu = "";
                        await db.AddAsync(lichsuOK);
                        await db.SaveChangesAsync();
                        var ketqua = new { mode = "OK", dulieu = dulieu, dulieuluuOK = lichsuOK };
                        return Json(ketqua);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch
            {
                throw new Exception();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Final()
        {
            try
            {
                string tenThietBiThuVe = MyStringStorage.ten_vattuthuve;
                string model = MyStringStorage.st_model;
                string cell = MyStringStorage.st_cell;
                string station = MyStringStorage.st_station;
                string thoigianphatsinhloi = MyStringStorage.st_thoigianphatsinhloi;
                string tenthietbiphatra = MyStringStorage.ten_vattuphatra;
                string tinhtrangloi = MyStringStorage.st_tinhtrangloi;
                string nguyennhanloi = MyStringStorage.st_nguyennhanloi;
                string caitien = MyStringStorage.st_caitien;
                string damnhiem = MyStringStorage.st_damnhiem;
                string ghichu = MyStringStorage.st_ghichu;
                decimal soluong = MyStringStorage.st_soluong;
                string donvi = MyStringStorage.st_donvi;

                Lichsusuachualoi lichsu = new Lichsusuachualoi();
                lichsu.TenThietBiThuVe = tenThietBiThuVe;
                lichsu.Model = model;
                lichsu.Cell = cell;
                lichsu.Station = station;
                lichsu.Thoigianphatsinhloi = thoigianphatsinhloi;
                lichsu.Thoigiancaitien = DateTime.Now.ToString("dd/MM/yyyy");
                lichsu.Tenthietbiphatra = tenthietbiphatra;
                lichsu.Tinhtrangloi = tinhtrangloi;
                lichsu.Nguyennhanloi = nguyennhanloi;
                lichsu.Caitien = caitien;
                lichsu.Damnhiem = damnhiem;
                lichsu.Ghichu = ghichu;
                lichsu.SoLuong = soluong;
                lichsu.DonVi = donvi;

                await db.AddAsync(lichsu);
                await db.SaveChangesAsync();

                var tonKhoPA = db.TonKhoPas.FirstOrDefault(x => x.Ten == tenthietbiphatra);
                if (tonKhoPA != null)
                {
                    // Cập nhật các trường dữ liệu
                    //tonKhoPA.SoLuongXuatRaLine += soluong;
                    //tonKhoPA.TonKhoTong -= soluong;
                    //tonKhoPA.SoLuongTonTu -= soluong;

                    tonKhoPA.SoLuongXuatRaLine += 0;
                    tonKhoPA.TonKhoTong -= 0;
                    tonKhoPA.SoLuongTonTu -= 0;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.Entry(tonKhoPA).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                TempData["success"] = 1;
                return RedirectToAction("Scan");
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