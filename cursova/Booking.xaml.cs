using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace cursova
{
    public partial class Booking : Window
    {
        private int? selectedId;
        private int selectedTourId;
        private bool isEditMode;
        string connectionString = AppSettings.GetConnectionString();

        private int selectedTouristId;
        private int selectedEmployeeId;
        private int? selectedDiscountId;
        private int seats = 0;
        DateTime tourdate;

        public Booking(int tourid, CurrentUser cur_employee)
        {
            InitializeComponent();
            selectedId = null;
            selectedTourId = tourid;
            isEditMode = false;
            LoadBookingInfo();
            LoadComboBoxes(cur_employee);
            NowDatePicker.SelectedDate = DateTime.Today;
            EmployeeComboBox.IsEnabled = false;
        }

        public Booking(int selectid, int tourid, bool editMode)
        {
            InitializeComponent();
            selectedId = selectid;
            selectedTourId = tourid;
            isEditMode = editMode;

            if (isEditMode)
                LoadBookingForEdit();
            else
            {
                LoadBookingInfo();
                LoadComboBoxes(null);
            }
        }

        private void okbtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedTourist = TouristComboBox.SelectedItem as ComboBoxItem;
            var selectedEmployee = EmployeeComboBox.SelectedItem as ComboBoxItem;
            string selectedDiscount = DiscountText.Text;
            int? touristId = Convert.ToInt32(selectedTourist.Tag);
            int? employeeId = Convert.ToInt32(selectedEmployee.Tag);
            if (string.IsNullOrWhiteSpace(VoucherTxt.Text) ||  selectedTourist == null || selectedEmployee == null || !int.TryParse(PeopleCountTxt.Text, out int peopleCount) || peopleCount < 0)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля вірно.");
                return;
            }
            if (seats < peopleCount)
            {
                MessageBox.Show("Вибачте, нема достатньої кількості вільних місць");
                return;
            }
            

            DateTime bookingDate = isEditMode && NowDatePicker.SelectedDate.HasValue
                ? NowDatePicker.SelectedDate.Value
                : DateTime.Now;            

            string insertQuery = @"
                INSERT INTO booking (id_tourist, id_voucher, id_employee, booking_date, people_count, id_discount) 
                VALUES (@id_tourist, @id_voucher, @id_employee, @booking_date, @people_count, @id_discount)";

            string updateQuery = @"
                UPDATE booking 
                SET id_tourist = @id_tourist, 
                    id_employee = @id_employee, 
                    id_voucher = @id_voucher, 
                    booking_date = @booking_date, 
                    people_count = @people_count, 
                    id_discount = @id_discount 
                WHERE id = @booking_id";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if (!isEditMode)
                {
                    using (var command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id_tourist", touristId);
                        command.Parameters.AddWithValue("@id_voucher", selectedTourId);
                        command.Parameters.AddWithValue("@id_employee", employeeId);
                        command.Parameters.AddWithValue("@booking_date", bookingDate);
                        command.Parameters.AddWithValue("@people_count", peopleCount);
                        command.Parameters.AddWithValue("@id_discount", SetDiscount(peopleCount, selectedDiscount) != -1 ? SetDiscount(peopleCount, selectedDiscount) : DBNull.Value);
                        
                        command.ExecuteNonQuery();
                    }

                    using (var command = new MySqlCommand("SELECT MAX(id) FROM booking", connection))
                    {
                        selectedId = Convert.ToInt32(command.ExecuteScalar());
                        command.ExecuteNonQuery();
                    }                    
                }
                else
                {
                    using (var command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id_tourist", touristId);
                        command.Parameters.AddWithValue("@id_voucher", selectedTourId);
                        command.Parameters.AddWithValue("@id_employee", employeeId);
                        command.Parameters.AddWithValue("@booking_date", bookingDate);
                        command.Parameters.AddWithValue("@people_count", peopleCount);
                        command.Parameters.AddWithValue("@id_discount", SetDiscount(peopleCount, selectedDiscount) != -1 ? SetDiscount(peopleCount, selectedDiscount) : DBNull.Value);
                        command.Parameters.AddWithValue("@booking_id", selectedId);

                        command.ExecuteNonQuery();
                    }
                    EmployeeComboBox.IsEnabled = true;
                }

                using (var command = new MySqlCommand(@"
                    SELECT ROUND(
                            (v2.Price - IFNULL((v2.Price * IFNull(d.percentage,0) / 100), 0)) * b.people_count, 0
                        ) AS amount
                    FROM booking b
                    JOIN voucher_2 v2 ON v2.id = b.id_voucher
                    LEFT JOIN discount d ON d.id = b.id_discount where b.id = @bookingId", connection))
                {
                    command.Parameters.AddWithValue("@bookingId", selectedId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            decimal amount = reader.GetDecimal("amount");
                            MessageBox.Show($"До сплати: {amount} грн", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не вдалося знайти суму для оплати.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }            

                this.Close();
        }

        private void LoadBookingInfo()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new MySqlCommand(GetFirstQuery(), connection))
                {
                    cmd.Parameters.AddWithValue("@TourId", selectedTourId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            VoucherTxt.Text = reader["trip_name"].ToString();
                            seats = Convert.ToInt32(reader["available_seats"]);
                            tourdate = Convert.ToDateTime(reader["start_date"]);
                        }
                    }
                }

                connection.Close();
            }
        }

        private void LoadBookingForEdit()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new MySqlCommand(GetEditQuery($"WHERE b.id = {selectedId}"), connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        VoucherTxt.Text = reader["name_vaucher"].ToString();
                        PeopleCountTxt.Text = reader["people_count"].ToString();
                        NowDatePicker.SelectedDate = Convert.ToDateTime(reader["booking_date"]);
                        seats = Convert.ToInt32(reader["available_seats"]);
                        selectedTouristId = Convert.ToInt32(reader["id_tourist"]);
                        tourdate = Convert.ToDateTime(reader["start_date"]);
                        selectedDiscountId = reader["id_discount"] == DBNull.Value ? null : Convert.ToInt32(reader["id_discount"]);

                        var selectedEmployeeId = new CurrentUser
                        {
                            Name = reader["employee_name"].ToString(),
                            Id = Convert.ToInt32(reader["id_employee"])
                        };
                        LoadComboBoxes(selectedEmployeeId);
                    }
                }
                connection.Close();
            }
        }

        private void LoadComboBoxes(CurrentUser cur_employee)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Tourist
                using (var cmd1 = new MySqlCommand("SELECT id, full_name FROM tourist", connection))
                using (var reader1 = cmd1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        var item = new ComboBoxItem
                        {
                            Content = reader1["full_name"].ToString(),
                            Tag = Convert.ToInt32(reader1["id"])
                        };
                        TouristComboBox.Items.Add(item);
                    }
                }


                // Employee
                using (var cmd2 = new MySqlCommand("SELECT id, full_name FROM employee WHERE id > 1", connection))
                using (var reader2 = cmd2.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var item = new ComboBoxItem
                        {
                            Content = reader2["full_name"].ToString(),
                            Tag = Convert.ToInt32(reader2["id"])
                        };
                        EmployeeComboBox.Items.Add(item);
                        
                    }

                    SelectComboBoxItemById(EmployeeComboBox, cur_employee.Id);
                }

                DiscountText.Text = GetDiscount(selectedDiscountId);

                if (isEditMode)
                {
                    SelectComboBoxItemById(TouristComboBox, selectedTouristId);
                    SelectComboBoxItemById(EmployeeComboBox, selectedEmployeeId);
                        
                }
                connection.Close();
            }
        }

        private void SelectComboBoxItemById(ComboBox comboBox, int id)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if ((int)item.Tag == id)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private int? SetDiscount(int peopleCount, string selectedDiscount)
        {

            if (peopleCount < 2)
            {
                childrenbox.IsChecked = false;
            }

            DateTime selected = NowDatePicker.SelectedDate ?? DateTime.MinValue;

             if ((bool)childrenbox.IsChecked)
            {
                selectedDiscount = "Дитяча";
            }
            else if ((bool)oldbox.IsChecked)
            {
                selectedDiscount = "Пенсійна";
            }
            else if (peopleCount > 3)
            {
                selectedDiscount = "Оптова";
            }
            else if (selected <= tourdate.AddMonths(-6))
            {
                selectedDiscount = "Рання";
            }

            if (selectedDiscount == "Нема")
            {
                return null;
            }
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd1 = new MySqlCommand($"SELECT id, discount_name FROM discount where discount_name = '{selectedDiscount}';", connection))
                using (var readerz = cmd1.ExecuteReader())
                {
                    int? h = null;
                    if (readerz.Read())
                    {
                        h =  Convert.ToInt32(readerz["id"]);
                    }
                    return h;
                }
                connection.Close();
            }
        }

        private string GetDiscount(int? Id)
        {        
            
            
            
            if (Id == null)
            {
                return "Нема";
            }
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();


                using (var cmd1 = new MySqlCommand(GetDQuery($"id = {Id}"), connection))
                using (var readerz = cmd1.ExecuteReader())
                {
                    string res = "-";
                    if (readerz.Read())
                    {
                        res = readerz["discount_name"].ToString();
                    }
                    if(res == "Дитяча")
                    {
                        childrenbox.IsChecked = true;
                    }
                    if (res == "Пенсійна")
                    {
                        oldbox.IsChecked = true;
                    }
                    return res;
                }
                connection.Close();
            }

        }

        public string GetEditQuery(string whereClause)
        {
            return $@"
                SELECT  
                v1.name_vaucher,  
                t.full_name AS tourist_name,  
                e.full_name AS employee_name,  
                b.booking_date,  
                b.people_count,  
                COALESCE(d.discount_name, '-') AS discount_name,
                b.id_tourist,
                b.id_employee,
                b.id_discount, 
                v1.seats - (
                    SELECT IFNULL(SUM(b2.people_count), 0)
                    FROM booking b2
                    WHERE b2.id_voucher = v2.id
                ) AS available_seats, 
                v2.start_date
            FROM booking b  
            JOIN employee e ON b.id_employee = e.id  
            JOIN voucher_2 v2 ON b.id_voucher = v2.id  
            JOIN voucher_1 v1 ON v2.id_trip = v1.id  
            JOIN tourist t ON b.id_tourist = t.id  
            LEFT JOIN discount d ON b.id_discount = d.id {whereClause}";
        }

        public string GetFirstQuery()
        {
            return @"
            SELECT 
            v2.id, 
            v2.id_trip, 
            v2.end_date, 
            v2.start_date, 
            v2.Price,
            v1.seats - (
                SELECT IFNULL(SUM(b2.people_count), 0)
                FROM booking b2
                WHERE b2.id_voucher = v2.id
            ) AS available_seats,
            v1.name_vaucher AS trip_name
        FROM voucher_2 v2
        JOIN voucher_1 v1 ON v1.id = v2.id_trip
            WHERE v2.id = @TourId";
        }
        public string GetDQuery(string whereClause)
        {
            return $@"
                SELECT id, discount_name FROM discount where {whereClause};";
        }

        private void calcbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PeopleCountTxt.Text, out int peopleCount) || peopleCount <= 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну кількість людей.");
                return;
            }

            if (childrenbox.IsChecked == false && oldbox.IsChecked == false)
            {
                DiscountText.Text = "Нема";
            }

            string currentDiscountText = DiscountText.Text;
            int? discountId = SetDiscount(peopleCount, currentDiscountText);

            if (discountId != null)
            {
                DiscountText.Text = GetDiscount(discountId);
            }
            else
            {
                DiscountText.Text = "Нема";
            }
        }
    }
}