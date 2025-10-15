using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cursova
{
    class Voucher
    {
        public int Id { get; set; }
        public string VoucherName { get; set; }
        public string Country { get; set; }
        public string TourType { get; set; }
        public int Duration { get; set; }
        public float Price { get; set; }

        public Voucher() { }

        public Voucher(int id, string voucherName, string country, string tourtype,
                          int duration, float price)
        {
            Id = id;
            VoucherName = voucherName;
            Country = country;
            TourType = tourtype;
            Duration = duration;
            Price = price;
        }
    }
}
