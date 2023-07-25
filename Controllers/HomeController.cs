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