using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace cursova
{
    public partial class autorization : Window
    {
        public autorization()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Login.Text) || string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show("Введіть логін та пароль");
                return;
            }
            string login = Login.Text.Trim();
            string password = Password.Password.Trim();
            CurrentUser cur_employee = GetEmployee(login, password);

            if (cur_employee != null)
            {
                MainWindow window = new MainWindow(cur_employee);
                window.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private CurrentUser GetEmployee(string login, string password)
        {
            CurrentUser cur_employee = null;

            string query = @$" select e.id, a.login, a.`password`, e.full_name 
                from authorization a 
                join employee e on e.id = a.id_employee 
                where a.login = '{login}' and a.`password` = '{password}';";

            try
            {
                string connectionString = AppSettings.GetConnectionString();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return cur_employee;
                    }

                    while (reader.Read())
                    {
                        cur_employee = new CurrentUser
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["full_name"].ToString(),
                            Login = reader["login"].ToString(),
                            Password = reader["password"].ToString()
                        };
                    }

                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час авторизації: " + ex.Message);
            }

            return cur_employee;
        }

    }
}
