using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cursova
{
    public class HistoryInfo
    {
        public string VoucherName { get; set; }
        public string EmployeeFullName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime BookingDate { get; set; }
        public int PeopleCount { get; set; }
        public string DiscountName { get; set; }

        public HistoryInfo() { }

        public HistoryInfo(string voucherName, string touristFullName, string employeeFullName,
                            DateTime bookingDate, int peopleCount, string discountName, DateTime startdate, DateTime enddate)
        {
            VoucherName = voucherName;
            EmployeeFullName = employeeFullName;
            BookingDate = bookingDate;
            PeopleCount = peopleCount;
            DiscountName = discountName;
            StartDate = startdate;
            EndDate = enddate;
        }
        
    }
}
