using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows;


namespace cursova
{
    public partial class Turistedit : Window
    {
        public Tourist TouristResult { get; private set; }

        private bool isEditMode;

        public Turistedit()
        {
            InitializeComponent();
            isEditMode = false;
        }

        public Turistedit(Tourist tourist)
        {
            InitializeComponent();
            isEditMode = true;
            TouristResult = tourist;
            NameTextBox.Text = tourist.Name;
            PhoneTextBox.Text = tourist.Number;
            EmailTextBox.Text = tourist.Type;

            if (DateTime.TryParse(tourist.Date, out DateTime parsedDate))
                BirthdayDatePicker.SelectedDate = parsedDate;
        }

        private void okbtn_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string phone = PhoneTextBox.Text;
            string email = EmailTextBox.Text;
            string date = BirthdayDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Будь ласка, заповніть усі обов’язкові поля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                if (!Regex.IsMatch(phone, @"^0\d{9}$"))
                {
                    MessageBox.Show("Невірний формат телефону. Має бути 10 цифр і починатися з 0");
                    return;
                }                    
                else if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
                {
                    MessageBox.Show("Електронна адреса повинна бути формату name@gmail.com");
                    return;
                }
            }
            

            try
                {
                    string connectionString = AppSettings.GetConnectionString();

                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        if (!isEditMode)
                        {
                            string insertQuery = "INSERT INTO tourist (full_name, phone_number, email, date_of_birth) VALUES (@name, @number, @email, @date)";
                            using (var command = new MySqlCommand(insertQuery, connection))
                            {
                                command.Parameters.AddWithValue("@name", name);
                                command.Parameters.AddWithValue("@number", phone);
                                command.Parameters.AddWithValue("@email", email);
                                command.Parameters.AddWithValue("@date", date);
                                command.ExecuteNonQuery();
                            }

                        }
                        else
                        {
                            string updateQuery = "UPDATE tourist SET full_name = @name, phone_number = @number, email = @email, date_of_birth = @date_of_birth WHERE id = @id";
                            using (var command = new MySqlCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@name", name);
                                command.Parameters.AddWithValue("@number", phone);
                                command.Parameters.AddWithValue("@email", email);
                                command.Parameters.AddWithValue("@date_of_birth", date);
                                command.Parameters.AddWithValue("@id", TouristResult.Id);
                                command.ExecuteNonQuery();
                            }
                        }

                        connection.Close();
                    }

                    TouristResult = new Tourist
                    {
                        Id = TouristResult?.Id ?? 0,
                        Name = name,
                        Number = phone,
                        Type = email,
                        Date = date
                    };

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при збереженні: " + ex.Message);
                }
        }
    
    }
}