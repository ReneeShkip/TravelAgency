CREATE DATABASE travel_agency;
use travel_agency;

CREATE TABLE country (
id TINYINT PRIMARY KEY,
country_name VARCHAR(50) NOT NULL UNIQUE,
continent VARCHAR(50) NOT NULL
);

CREATE TABLE  city(
id TINYINT AUTO_INCREMENT PRIMARY KEY,
city_name VARCHAR(50) NOT NULL,
id_country TINYINT NOT NULL, FOREIGN KEY (id_country) REFERENCES country(id)
);

CREATE TABLE tour_type (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
`type` VARCHAR(50) NOT NULL,
restriction text NOT NULL
);

CREATE TABLE tourist (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
full_name VARCHAR(100) NOT NULL,
phone_number VARCHAR(15) NOT NULL,
email VARCHAR(50) NOT NULL UNIQUE,
date_of_birth DATE NOT NULL
);

CREATE TABLE job_position (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
name_position VARCHAR(50) NOT NULL
);

CREATE TABLE employee (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
full_name VARCHAR(200) NOT NULL,
id_position TINYINT NOT NULL, FOREIGN KEY (id_position) REFERENCES job_position(id),
phone_number VARCHAR(15) NOT NULL
);

CREATE TABLE hotel (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
name_hotel VARCHAR(100) NOT NULL,
id_city TINYINT NOT NULL, FOREIGN KEY (id_city) REFERENCES city(id),
address TEXT NOT NULL,
rating DECIMAL(3,2) CHECK(Rating >= 0 AND Rating <= 5),
contact_info VARCHAR(255) NOT NULL
);

CREATE TABLE type_transport (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
type_transport VARCHAR(20) NOT NULL
);

CREATE TABLE transport (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
id_type_transport TINYINT NOT NULL, FOREIGN KEY (id_type_transport) REFERENCES type_transport(id),
departure_time DATETIME NOT NULL,
arrival_time DATETIME NOT NULL
);

CREATE TABLE food (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
type_food VARCHAR(100) NOT NULL,
price_per_meal DECIMAL(10,2) NOT NULL
);

CREATE TABLE voucher_1 (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
name_vaucher VARCHAR(100) NOT NULL,
id_hotel TINYINT NOT NULL, FOREIGN KEY (id_hotel) REFERENCES hotel(id),
duration INT NOT NULL,
id_tour_type TINYINT NOT NULL, FOREIGN KEY (id_tour_type) REFERENCES tour_type(id),
id_transport TINYINT NOT NULL, FOREIGN KEY (id_transport) REFERENCES transport(id),
`description` TEXT NOT NULL,
id_food TINYINT NOT NULL, FOREIGN KEY (id_food) REFERENCES food(id),
seats INT NOT NULL
);

CREATE TABLE voucher_2 (
id TINYINT AUTO_INCREMENT PRIMARY KEY,
id_trip TINYINT NOT NULL, FOREIGN KEY (id_trip) REFERENCES voucher_1(id),
start_date DATE NOT NULL,
end_date DATE NOT NULL,
Price DECIMAL(10,2) NOT NULL
);

CREATE TABLE discount (
  id TINYINT AUTO_INCREMENT PRIMARY KEY,
  discount_name VARCHAR(100) NOT NULL,
  percentage DECIMAL(5,2) NOT NULL CHECK (percentage >= 0 AND percentage <= 100)
);


CREATE TABLE booking (
  id TINYINT AUTO_INCREMENT PRIMARY KEY,
  id_tourist TINYINT NOT NULL, FOREIGN KEY (id_tourist) REFERENCES tourist(id),
  id_voucher TINYINT NOT NULL, FOREIGN KEY (id_voucher) REFERENCES voucher_2(id),
  id_employee TINYINT NOT NULL, FOREIGN KEY (id_employee) REFERENCES employee(id),
  booking_date DATETIME NOT NULL,
  people_count INT NOT NULL,
  id_discount TINYINT, FOREIGN KEY (id_discount) REFERENCES discount(id)
);

CREATE TABLE authorization (
	id TINYINT AUTO_INCREMENT PRIMARY KEY,
    id_employee TINYINT NOT NULL, FOREIGN KEY (id_employee) REFERENCES employee(id),
    login varchar(20) UNIQUE NOT NULL,
    `password` varchar(20) UNIQUE NOT NULL
);