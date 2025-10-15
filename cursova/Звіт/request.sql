-- авторизація
select 
	a.login, 
	a.`password`, 
	p.name_position 
from authorization a 
join employee e on a.id_employee = e.id 
join job_position p on e.id_position = p.id;


-- країни
select country_name from country;

-- типи турів
select `type` from tour_type;
    
-- туристи
	select 
		full_name, 
		phone_number, 
		email, 
		date_of_birth 
	from tourist;

-- бронювання
SELECT 
  v1.name_vaucher, 
  t.full_name as tourist_name, 
  e.full_name as employee_name, 
  b.booking_date, 
  b.people_count, 
  COALESCE(d.discount_name, '-') AS discount_name
FROM booking b 
JOIN employee e ON b.id_employee = e.id 
JOIN voucher_1 v1 ON b.id_voucher = v1.id 
JOIN tourist t ON b.id_tourist = t.id 
LEFT JOIN discount d ON b.id_discount = d.id;
    
-- попередній перегляд путівки
SELECT 
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
GROUP BY 
  v1.name_vaucher, 
  c.city_name,
  ct.country_name, 
  t.`type`, 
  v1.duration,
  v2.Price;

-- детальний перегляд путівки
WITH counts AS (
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
  v1.seats,
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
  LEFT JOIN ranked r ON r.id_voucher = v2.id WHERE v2.id = 5
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
  r.`rank` ;
  