using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace cursova
{
    public partial class History : Window
    {
        private int _touristId;

        public History(int touristId, string touristName)
        {
            InitializeComponent();

            _touristId = touristId;
            SelectedTourist.Content = touristName;

            string query = GetQuery(touristId.ToString());
            LoadBooking(query);
        }

        ObservableCollection<HistoryInfo> history = new ObservableCollection<HistoryInfo>();


        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public string GetQuery(string whereClause)
        {
            return $@"select v1.name_vaucher,
	                SUBSTRING_INDEX(SUBSTRING_INDEX(e.full_name, ' ', 2), ' ', -1) AS employee_name,
                    b.booking_date,
                    v2.end_date,
                    v2.start_date, 
                    b.people_count, 
                    COALESCE(d.discount_name, '-') AS discount_name, v2.id
                    FROM booking b 
                JOIN employee e ON b.id_employee = e.id                 
                JOIN voucher_2 v2 ON b.id_voucher = v2.id 
                JOIN voucher_1 v1 ON v1.id = v2.id_trip
                JOIN tourist t ON b.id_tourist = t.id 
                LEFT JOIN discount d ON b.id_discount = d.id where t.id = {whereClause};";

        }

        private void LoadBooking(string query)
        {
            string connectionString = AppSettings.GetConnectionString();
            history.Clear();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        history.Add(new HistoryInfo
                        {
                                VoucherName = reader["name_vaucher"].ToString(),
                                EmployeeFullName = reader["employee_name"].ToString(),
                                BookingDate = (DateTime)reader["start_date"],
                                PeopleCount = Convert.ToInt32(reader["people_count"]),
                                DiscountName = reader["discount_name"].ToString(),
                                StartDate = (DateTime)reader["start_date"],
                                EndDate = (DateTime)reader["start_date"]
                        });
                    }
                    connection.Close();
                }

                HistoryDataGrid.ItemsSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні туристів: " + ex.Message);
            }
        }
    }
}
