using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace cursova
{
    public partial class BookingPage : Page
    {
        CurrentUser cur_user = null;
        private int selected = -1;
        private int seats;
        
        ObservableCollection<BookingInfo> booking = new ObservableCollection<BookingInfo>();

        public BookingPage(CurrentUser current)
        {
            cur_user = current;
            InitializeComponent();
            LoadBooking(GetQuery(string.Empty));
        }

        private const string EmptyText = "|  Пошук за назвою ...";

        public string GetQuery(string whereClause)
        {
            return $@"
           SELECT 
              b.id AS booking_id,
              v1.name_vaucher,
              v1.seats - (
                SELECT IFNULL(SUM(b2.people_count), 0)
                FROM booking b2
                WHERE b2.id_voucher = b.id_voucher
              ) AS available_seats,
              t.full_name AS tourist_name, 
              SUBSTRING_INDEX(SUBSTRING_INDEX(e.full_name, ' ', 2), ' ', -1) AS employee_name, 
              b.booking_date, 
              b.people_count, 
              COALESCE(d.discount_name, '-') AS discount_name,
              COALESCE(d.percentage, 0) AS discount_perc,
              v2.Price,
              v2.id AS id_vaucher, v2.start_date
            FROM booking b 
            JOIN employee e ON b.id_employee = e.id 
            JOIN voucher_2 v2 ON b.id_voucher = v2.id 
            JOIN voucher_1 v1 ON v2.id_trip = v1.id 
            JOIN tourist t ON b.id_tourist = t.id 
            LEFT JOIN discount d ON b.id_discount = d.id
            {whereClause};";
        }
        private decimal balance = 0;
        private void LoadBooking(string query)
        {
            string connectionString = AppSettings.GetConnectionString();
            booking.Clear();
            balance = 0;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var d = reader["discount_perc"];
                        booking.Add(new BookingInfo
                        {
                            Id = Convert.ToInt32(reader["booking_id"]),
                            VoucherName = reader["name_vaucher"].ToString(),
                            TouristFullName = reader["tourist_name"].ToString(),
                            EmployeeFullName = reader["employee_name"].ToString(),
                            DiscountPerc = Convert.ToDecimal(reader["discount_perc"]),
                            Price = Convert.ToInt32(reader["Price"]),
                            PeopleCount = Convert.ToInt32(reader["people_count"]),
                            DiscountName = reader["discount_name"].ToString(),
                            VoucherId = Convert.ToInt32(reader["id_vaucher"]),
                            Start_date = Convert.ToDateTime(reader["start_date"])
                        });                        
                    }
                    connection.Close();
                }

                BookingDataGrid.ItemsSource = booking;
                foreach (var b in booking)
                {
                    balance += b.Amount;
                }
                Amountr.Text = balance.ToString() + "грн";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні бронюваня: " + ex.Message);
            }
        }

        private void infobtn_Copy1_Click(object sender, RoutedEventArgs e)
        {
            
            var selectedBooking = BookingDataGrid.SelectedItem as BookingInfo;
            if (selectedBooking != null)
            {
                if (cur_user == null)
                {
                    MessageBox.Show("Ви не авторизовані!");
                    return;
                }
                if (selectedBooking.Start_date.Date < DateTime.Today)
                {
                    MessageBox.Show("Бронювання вже не актуально");
                    return;
                }

                Booking bookingWindow = new Booking(selectedBooking.Id, selectedBooking.VoucherId, true);                
                bookingWindow.ShowDialog();
                if (!bookingWindow.IsActive)
                {
                    LoadBooking(GetQuery(string.Empty));
                } 
            }
            else
            {
                MessageBox.Show("Виберіть бронювання для редагування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void deletetext_Click(object sender, RoutedEventArgs e)
        {
            findbox.Text = EmptyText;
            LoadBooking(GetQuery(ApplyFilters(string.Empty)));
        }

        private bool isProgrammatic = false;

        private void CheckAll_Checked(object sender, RoutedEventArgs e)
        {
            isProgrammatic = true;
            Admin.IsChecked = true;
            Employee.IsChecked = true;
            Headmaster.IsChecked = true;
            isProgrammatic = false;
        }

        private void CheckAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isProgrammatic)
            {
                isProgrammatic = true;
                Admin.IsChecked = false;
                Employee.IsChecked = false;
                Headmaster.IsChecked = false;
                isProgrammatic = false;
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (Admin.IsChecked == true && Employee.IsChecked == true && Headmaster.IsChecked == true)
            {
                isProgrammatic = true;
                CheckAll.IsChecked = true;
                isProgrammatic = false;
            }
        }

        private void Option_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CheckAll.IsChecked == true)
            {
                isProgrammatic = true;
                CheckAll.IsChecked = false;
                isProgrammatic = false;
            }
        }

        private void Okbtn_Click(object sender, RoutedEventArgs e)
        {
            LoadBooking(GetQuery(ApplyFilters(findbox.Text.Trim())));
        }

        private string ApplyFilters(string searchtext)
        {
            List<string> whereConditions = new List<string>();
            List<string> positionConditions = new List<string>();

            if (Employee.IsChecked == true)
                positionConditions.Add("e.id_position = 2");

            if (Admin.IsChecked == true)
                positionConditions.Add("e.id_position = 4");

            if (Headmaster.IsChecked == true)
                positionConditions.Add("e.id_position = 3");

            if (positionConditions.Count > 0)
                whereConditions.Add("(" + string.Join(" OR ", positionConditions) + ")");

            if (!string.IsNullOrEmpty(searchtext) && searchtext != EmptyText)
                whereConditions.Add($"v1.name_vaucher LIKE '%{searchtext}%'");
            if(datefiltermax.SelectedDate.HasValue && (datefilter.SelectedDate.HasValue)) { 
                if (datefilter.SelectedDate > datefiltermax.SelectedDate)
                {
                    MessageBox.Show("Кінцева дата не може бути раніше за початкову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    whereConditions.Add($"v2.start_date >= '{datefilter.SelectedDate.Value:yyyy-MM-dd HH:mm:ss}'");                       
                    whereConditions.Add($"v2.end_date <= '{datefiltermax.SelectedDate.Value:yyyy-MM-dd HH:mm:ss}'");
                }
            }



            if (Alldisc.IsChecked == true && Nulldisc.IsChecked != true)
                whereConditions.Add("COALESCE(d.discount_name, '-') <> '-'");
            else if (Nulldisc.IsChecked == true && Alldisc.IsChecked != true)
                whereConditions.Add("COALESCE(d.discount_name, '-') = '-'");

            return whereConditions.Count > 0
                ? "WHERE " + string.Join(" AND ", whereConditions)
                : "";
        }

        private void finderinbox_Click(object sender, RoutedEventArgs e)
        {
            LoadBooking(GetQuery(ApplyFilters(findbox.Text.Trim())));
        }
    }
}