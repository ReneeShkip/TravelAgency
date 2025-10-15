namespace cursova
{
    public class BookingInfo
    {
        public int Id { get; set; }
        public string VoucherName { get; set; }
        public int VoucherId { get; set; }
        public string TouristFullName { get; set; }
        public string EmployeeFullName { get; set; }
        public int PeopleCount { get; set; }
        public DateTime Start_date { get; set; }
        public string DiscountName { get; set; }
        public decimal DiscountPerc { get; set; }
        public decimal Price { get; set; }
        public decimal Amount => (Price - Price * DiscountPerc/100) * PeopleCount;
      

        public BookingInfo() { }

        public BookingInfo(int id, string voucherName, string touristFullName, string employeeFullName,
             int peopleCount, string discountName, int voucherId, DateTime start_date)
        {
            Id = id;
            VoucherName = voucherName;
            VoucherId = voucherId;
            TouristFullName = touristFullName;
            EmployeeFullName = employeeFullName;
            PeopleCount = peopleCount;
            DiscountName = discountName;
            Start_date = start_date;
        }
    }
}
