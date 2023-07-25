﻿using System;
using System.Collections.Generic;

namespace WebScanBarcode.Models
{
    public partial class Lichsu
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;
        public string PhanLoai { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Cell { get; set; } = null!;
        public string Station { get; set; } = null!;
        public string? TrangThai { get; set; }
        public string? ThoiGianBao { get; set; }
        public string? HienTuongLoi { get; set; }
        public decimal? SoLuong { get; set; }
        public string? ThoiGianXacNhan { get; set; }
    }
}
