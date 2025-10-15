using MySql.Data.MySqlClient;
using System.Windows;

namespace cursova
{
    public partial class vaucher_info : Window
    {
        int selectid;
        string connectionString = AppSettings.GetConnectionString();
        CurrentUser cur_employee = null;
        public vaucher_info(int selected, CurrentUser current)
        {
            InitializeComponent();
            LoadAllInfo(Getquerry($"WHERE v2.id = {selected}"));
            selectid = selected;
            cur_employee = current;
        }

        private string Getquerry(string whereclause)
        {
            return $@"WITH counts AS (
                    SELECT id_voucher, COUNT(*) AS count
                    FROM booking
                    GROUP BY id_voucher
                ),
                extremes AS (
                    SELECT MAX(count) AS max_count
                    FROM counts
                ),
                ranked AS (
                    SELECT 
                        id_voucher,
                        count,
                        CASE 
                            WHEN count = max_count THEN 5
                            WHEN count >= max_count / 1.5 THEN 4
                            WHEN count >= max_count / 2 THEN 3
                            WHEN count >= max_count / 3 THEN 2
                            WHEN count >= 1 THEN 1
                            ELSE 0
                        END AS `rank`
                    FROM counts
                    JOIN extremes
                )

                SELECT
                  v2.id ,v1.name_vaucher,
                  c.city_name,
                  ct.country_name, 
                  t.`type`, 
                  v1.duration,
                  v1.`description`,
                  ttr.type_transport,
                  f.type_food,
                  h.name_hotel,
                  v1.seats - (
                    SELECT IFNULL(SUM(b2.people_count), 0)
                    FROM booking b2
                    WHERE b2.id_voucher = v2.id
                ) AS available_seats,
                  v2.start_date,
                  v2.end_date,
                  v2.Price,
                  COALESCE(r.`rank`, 1) AS `rank` -- якщо бронювань не було, виставляємо ранг 1
                FROM voucher_1 v1  
                  JOIN hotel h ON v1.id_hotel = h.id
                  JOIN city c ON h.id_city = c.id
                  JOIN country ct ON c.id_country = ct.id
                  JOIN tour_type t ON v1.id_tour_type = t.id
                  JOIN voucher_2 v2 ON v2.id_trip = v1.id
                  JOIN food f ON v1.id_food = f.id
                  JOIN transport tr ON v1.id_transport = tr.id
                  JOIN type_transport ttr ON tr.id_type_transport = ttr.id
                  LEFT JOIN ranked r ON r.id_voucher = v2.id
                 {whereclause}
                    GROUP BY
                      v2.id, v1.name_vaucher, 
                      ct.country_name, 
                      t.`type`, 
                      v1.duration, 
                      v2.Price,
                      v1.`description`,
                      ttr.type_transport,
                      f.type_food,
                      h.name_hotel,
                      v1.seats,
                      v2.start_date,
                      v2.end_date,
                      r.`rank` ;";
        }

        private void LoadAllInfo(string query)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        txtName.Content = reader["name_vaucher"].ToString();
                        txtCity.Content = reader["city_name"].ToString();
                        txtCountry.Content = reader["country_name"].ToString();
                        txtDuration.Content = Convert.ToInt32(reader["duration"].ToString());
                        txtStart.Content = Convert.ToDateTime(reader["start_date"].ToString());
                        txtEnd.Content = Convert.ToDateTime(reader["end_date"].ToString());
                        txtRate.Content = Convert.ToInt32(reader["rank"].ToString());
                        txtType.Content = reader["type"].ToString();
                        txtHotel.Content = reader["name_hotel"].ToString();
                        txtTransport.Content = reader["type_transport"].ToString();
                        txtSeats.Content = Convert.ToInt32(reader["available_seats"].ToString());
                        txtPrice.Content = Convert.ToDecimal(reader["Price"].ToString());
                        txtDescription.Text = reader["description"].ToString();
                    }
                    connection.Close();
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(cur_employee == null)
            {
                MessageBox.Show("Ви не авторизовані!");
                this.Close();
                return;
            }
            Booking window = new Booking(selectid, cur_employee);
            window.Show();
            this.Close();
        }
    }
}
