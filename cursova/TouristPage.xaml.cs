using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace cursova
{

    public partial class TouristPage : Page
    {
        ObservableCollection<Tourist> tourists = new ObservableCollection<Tourist>();
        CurrentUser employee = null;
        public TouristPage(CurrentUser cur_employee)
        {
            InitializeComponent();
            LoadTourists(GetQuery("", ""));
            employee = cur_employee;
        }


        private const string EmptyText = "|  Пошук за ім'ям ...";
        public string GetQuery(string condition, string whereClause)
        {
            return $@"
            SELECT 
                t.id,
                t.full_name, 
                t.phone_number, 
                t.email, 
                t.date_of_birth,
                COUNT(b.id) AS booking_count
            FROM tourist t
            LEFT JOIN booking b ON t.id = b.id_tourist
            {whereClause}
            GROUP BY t.id
            {condition};";
        }

        private void LoadTourists(string query)
        {
            string connectionString = AppSettings.GetConnectionString();
            tourists.Clear();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        tourists.Add(new Tourist
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["full_name"].ToString(),
                            Number = reader["phone_number"].ToString(),
                            Type = reader["email"].ToString(),
                            Date = Convert.ToDateTime(reader["date_of_birth"]).ToShortDateString()
                        });
                    }
                    connection.Close();
                }

                TouristsDataGrid.ItemsSource = tourists;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні туристів: " + ex.Message);
            }
        }
        

        private void edittouristbtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedTourist = TouristsDataGrid.SelectedItem as Tourist;
            if (employee == null)
            {
                MessageBox.Show("Ви не авторизовані.");
                return;
            }                
            if (selectedTourist != null)
            {
                var editWindow = new Turistedit(selectedTourist);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTourists(GetQuery("", ""));
                }
            }
            else
            {
                MessageBox.Show("Виберіть туриста для редагування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void addbtn_Click(object sender, RoutedEventArgs e)
        {
            if (employee == null)
            {
                MessageBox.Show("Ви не авторизовані.");
                return;
            }
               
            var window = new Turistedit();
            if (window.ShowDialog() == true)
            {
                var newTourist = window.TouristResult;
                if (newTourist != null)
                {
                    tourists.Add(newTourist);
                }
            }
        }


        private void deletetext_Click(object sender, RoutedEventArgs e)
        {
            findbox.Text = EmptyText;
            LoadTourists(GetQuery(GetCondition(), Filterscheck(string.Empty)));
            ;
        }

        private void historytbtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedTourist = TouristsDataGrid.SelectedItem as Tourist;

            if (selectedTourist == null)
            {
                MessageBox.Show("Будь ласка, виберіть туриста.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int touristId = selectedTourist.Id;
            string touristName = selectedTourist.Name;

            History historyWindow = new History(touristId, touristName);
            historyWindow.ShowDialog();
        }

        private bool isProgrammatic = false;

        private void CheckAll_Checked(object sender, RoutedEventArgs e)
        {
            isProgrammatic = true;
            Option1.IsChecked = true;
            Option2.IsChecked = true;
            isProgrammatic = false;
        }

        private void CheckAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isProgrammatic)
            {
                isProgrammatic = true;
                Option1.IsChecked = false;
                Option2.IsChecked = false;
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

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (Option1.IsChecked == true && Option2.IsChecked == true)
            {
                isProgrammatic = true;
                CheckAll.IsChecked = true;
                isProgrammatic = false;
            }
        }

        private void Okbtn_Click(object sender, RoutedEventArgs e)
        {
            LoadTourists(GetQuery(GetCondition(), Filterscheck(findbox.Text.Trim())));
        }

        private string GetCondition()
        {
            string condition = "";

            if (Option1.IsChecked == true && Option2.IsChecked == false)
            {
                condition = "HAVING booking_count <= 1";
            }
            else if (Option2.IsChecked == true && Option1.IsChecked == false)
            {
                condition = "HAVING booking_count > 1";
            }

            return condition;
        }

        private string Filterscheck(string findertext)
        {
            List<string> whereConditions = new List<string>();
            string number = PhoneNum.Text.Trim();

            if (!string.IsNullOrEmpty(number) && number != EmptyText)
            {
                whereConditions.Add($"t.phone_number LIKE '%{number}%'");
            }

            if (!string.IsNullOrEmpty(findertext) && findertext != EmptyText)
            {
                whereConditions.Add($"t.full_name LIKE '%{findertext}%'");
            }

                return whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        }

        private void finderinbox_Click(object sender, RoutedEventArgs e)
        {
            LoadTourists(GetQuery(GetCondition(), Filterscheck(findbox.Text.Trim())));

        }

        private void deletephone_Click(object sender, RoutedEventArgs e)
        {
            PhoneNum.Text = string.Empty;
        }
    }
}