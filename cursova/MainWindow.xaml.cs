using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace cursova
{
    public partial class MainWindow : Window
    {
        ObservableCollection<Voucher> voucher = new ObservableCollection<Voucher>();
        string connectionString = AppSettings.GetConnectionString();
        const string EmptyText = "|  Пошук за назвою ...";
        CurrentUser cur_employee = null;

        public MainWindow(CurrentUser employee)
        {
            InitializeComponent();
            LoadVoucher();
            LoadCountries();
            insidebtn.Content = employee.Name;
            cur_employee = employee;
            insidebtn.IsEnabled = false;
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadVoucher();
            LoadCountries();
            insidebtn.IsEnabled = true;
        }

        private void LoadVoucher()
        {
            voucher.Clear();
            string base_query = GetBaseQuery(string.Empty);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(base_query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        voucher.Add(new Voucher
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            VoucherName = reader["name_vaucher"].ToString(),
                            Country = reader["country_name"].ToString(),
                            TourType = reader["type"].ToString(),
                            Duration = Convert.ToInt32(reader["duration"]),
                            Price = Convert.ToSingle(reader["Price"]),
                        });
                    }
                    connection.Close();
                }

                ToursDataGrid.ItemsSource = voucher;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні туристів: " + ex.Message);
            }
        }

        private void LoadCountries()
        {
            string query = "SELECT country_name FROM country;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string countryName = reader["country_name"].ToString();

                        CheckBox checkBox = new CheckBox
                        {
                            Content = countryName,
                            Margin = new Thickness(15, 5, 5, 5),
                            IsEnabled = true
                        };

                        CountryCheckBoxesPanel.Children.Add(checkBox);
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні країн: " + ex.Message);
            }
        }

        private void Info_click(object sender, RoutedEventArgs e)
        {
            var selectedVoucher = ToursDataGrid.SelectedItem as Voucher;

            if (selectedVoucher != null)
            {
                vaucher_info window = new vaucher_info(selectedVoucher.Id, cur_employee);
                window.Show();
            }
            else
            {
                MessageBox.Show("Виберіть тур зі списку для перегляду", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Enter_click(object sender, RoutedEventArgs e)
        {
            autorization window = new autorization();
            window.Show();
            this.Close();
        }

        private void CheckAllCou_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var child in CountryCheckBoxesPanel.Children)
            {
                if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = true;
                }
            }
        }

        private void CheckAllCou_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var child in CountryCheckBoxesPanel.Children)
            {
                if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = false;
                }
            }
        }

        private void CheckAllTypes_Checked(object sender, RoutedEventArgs e)
        {
            Type1.IsChecked = true;
            Type2.IsChecked = true;
            Type3.IsChecked = true;
        }

        private void CheckAllTypes_Unchecked(object sender, RoutedEventArgs e)
        {
            Type1.IsChecked = false;
            Type2.IsChecked = false;
            Type3.IsChecked = false;
        }

        private void turistsbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TouristPage(cur_employee));
            vaucherbtn.BorderThickness = new Thickness(0);
            turistsbtn.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFEEA537");
            turistsbtn.BorderThickness = new Thickness(0, 0, 0, 3);
            allbtn.BorderThickness = new Thickness(0);
            Panel.SetZIndex(MainFrame, 3);
        }

        private void vaucherbtn_Click(object sender, RoutedEventArgs e)
        {
            turistsbtn.BorderThickness = new Thickness(0);
            vaucherbtn.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFEEA537");
            vaucherbtn.BorderThickness = new Thickness(0, 0, 0, 3);
            allbtn.BorderThickness = new Thickness(0);
            MainFrame.Content = null;
            Panel.SetZIndex(MainFrame, 0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vaucherbtn.BorderThickness = new Thickness(0, 0, 0, 3);
            vaucherbtn.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFEEA537");
            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Panel.SetZIndex(MainFrame, 10);
            MainFrame.Content = null;
            Panel.SetZIndex(findbox, 2);
            Panel.SetZIndex(cross, 2);
            Panel.SetZIndex(MainFrame, 0);
            Panel.SetZIndex(ToursDataGrid, 2);
            Panel.SetZIndex(finder, 2);
        }

        private void allbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BookingPage(cur_employee));
            vaucherbtn.BorderThickness = new Thickness(0);
            turistsbtn.BorderThickness = new Thickness(0);
            allbtn.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFEEA537");
            allbtn.BorderThickness = new Thickness(0,0,0,3);
            Panel.SetZIndex(MainFrame, 3);
        }

        private void deletetext_Click(object sender, RoutedEventArgs e)
        {
            findbox.Text = EmptyText;
            ApplyFiltersAndSearch(string.Empty);
        }

        private List<string> GetSelectedCountries()
        {
            List<string> selectedCountries = new List<string>();

            foreach (var child in CountryCheckBoxesPanel.Children)
            {
                if (child is CheckBox cb && cb.IsChecked == true)
                {
                    selectedCountries.Add(cb.Content.ToString());
                }
            }

            return selectedCountries;
        }

        private List<string> GetSelectedTypes()
        {
            List<string> selectedTypes = new List<string>();

            if (Type1.IsChecked == true) selectedTypes.Add("Пляжні");
            if (Type2.IsChecked == true) selectedTypes.Add("Екскурсійні");
            if (Type3.IsChecked == true) selectedTypes.Add("Оздоровчі");

            return selectedTypes;
        }

        private void Okbtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = findbox.Text.Trim();
            if (searchText == EmptyText)
            {
                searchText = string.Empty;
            }
            ApplyFiltersAndSearch(searchText);
        }

        private void ApplyFiltersAndSearch(string searchText)
        {
            List<string> selectedCountries = GetSelectedCountries();
            List<string> selectedTypes = GetSelectedTypes();

            List<string> whereConditions = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (selectedCountries.Count > 0)
            {
                string[] countryParams = selectedCountries.Select((c, i) => $"@country{i}").ToArray();
                whereConditions.Add($"ct.country_name IN ({string.Join(", ", countryParams)})");
                for (int i = 0; i < selectedCountries.Count; i++)
                {
                    parameters.Add($"@country{i}", selectedCountries[i]);
                }
            }

            if (selectedTypes.Count > 0)
            {
                string[] typeParams = selectedTypes.Select((t, i) => $"@type{i}").ToArray();
                whereConditions.Add($"t.`type` IN ({string.Join(", ", typeParams)})");
                for (int i = 0; i < selectedTypes.Count; i++)
                {
                    parameters.Add($"@type{i}", selectedTypes[i]);
                }
            }

            if (decimal.TryParse(MinPriceBox.Text, out decimal minPrice) &&
                decimal.TryParse(MaxPriceBox.Text, out decimal maxPrice))
            {
                if (maxPrice < minPrice)
                {
                    MessageBox.Show("Максимальна ціна не може бути меншою за мінімальну.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                whereConditions.Add("v2.Price >= @minPrice");
                parameters.Add("@minPrice", minPrice);

                whereConditions.Add("v2.Price <= @maxPrice");
                parameters.Add("@maxPrice", maxPrice);
            }
            else
            {
                if (decimal.TryParse(MinPriceBox.Text, out minPrice))
                {
                    whereConditions.Add("v2.Price >= @minPrice");
                    parameters.Add("@minPrice", minPrice);
                }

                if (decimal.TryParse(MaxPriceBox.Text, out maxPrice))
                {
                    whereConditions.Add("v2.Price <= @maxPrice");
                    parameters.Add("@maxPrice", maxPrice);
                }
            }

            if (mindate.SelectedDate.HasValue && maxdate.SelectedDate.HasValue)
            {
                if (maxdate.SelectedDate < mindate.SelectedDate)
                {
                    MessageBox.Show("Кінцева дата не може бути раніше за початкову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                else { 
                    whereConditions.Add("v2.start_date >= @mindate");
                    parameters.Add("@mindate", mindate.SelectedDate.Value.Date);

                    whereConditions.Add("v2.end_date <= @maxdate");
                    parameters.Add("@maxdate", maxdate.SelectedDate.Value.Date);
                }
            }
            else
            {
                if (mindate.SelectedDate.HasValue)
                {
                    whereConditions.Add("v2.start_date >= @mindate");
                    parameters.Add("@mindate", mindate.SelectedDate.Value.Date);
                }

                if (maxdate.SelectedDate.HasValue)
                {
                    whereConditions.Add("v2.end_date <= @maxdate");
                    parameters.Add("@maxdate", maxdate.SelectedDate.Value.Date);
                }
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                whereConditions.Add("v1.name_vaucher LIKE @search");
                parameters.Add("@search", "%" + searchText + "%");
            }

            string whereClause = "AND " + string.Join(" AND ", whereConditions);
            string query = GetBaseQuery(whereClause);

            voucher.Clear();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        voucher.Add(new Voucher
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            VoucherName = reader["name_vaucher"].ToString(),
                            Country = reader["country_name"].ToString(),
                            TourType = reader["type"].ToString(),
                            Duration = Convert.ToInt32(reader["duration"]),
                            Price = Convert.ToSingle(reader["Price"]),
                        });
                    }
                    connection.Close();
                }

                ToursDataGrid.ItemsSource = voucher;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при фільтрації: " + ex.Message);
            }
        }

        private void finderinbox_Click(object sender, RoutedEventArgs e)
        {
            string searchText = findbox.Text.Trim();

            ApplyFiltersAndSearch(searchText);            
        }

        private string GetBaseQuery(string whereClause)
        {
            return $@"
                SELECT 
                v2.id,
                v1.name_vaucher, 
                ct.country_name, 
                t.`type`, 
                v1.duration, 
                v2.Price
            FROM voucher_1 v1
            JOIN hotel h ON v1.id_hotel = h.id
            JOIN city c ON h.id_city = c.id
            JOIN country ct ON c.id_country = ct.id
            JOIN tour_type t ON v1.id_tour_type = t.id
            JOIN voucher_2 v2 ON v2.id_trip = v1.id
            LEFT JOIN booking b ON b.id_voucher = v2.id
            WHERE v2.start_date > CURDATE()
                AND (
                    SELECT IFNULL(SUM(b2.people_count), 0)
                    FROM booking b2
                    WHERE b2.id_voucher = v2.id
                ) < v1.seats {whereClause}
            GROUP BY 
                v2.id,
                v1.name_vaucher, 
                ct.country_name, 
                t.`type`, 
                v1.duration,
                v2.Price;";
                    }

        private void deletemin_Click(object sender, RoutedEventArgs e)
        {
            MinPriceBox.Text = string.Empty;
        }
        private void deletemax_Click(object sender, RoutedEventArgs e)
        {
            MaxPriceBox.Text = string.Empty;
        }
    }       
    
}