using System;
using System.Collections.Generic;

namespace WebScanBarcode.Models
{
    public partial class Lichsusanchiet
    {
        public int Id { get; set; }
        public string? Vitri { get; set; }
        public string? Nguoithuchien { get; set; }
        public string? Ngaythuchien { get; set; }
        public string? Giothuchien { get; set; }
        public string? Result { get; set; }
    }
}
