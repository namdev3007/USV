-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: clinic_booking
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `appointments`
--

DROP TABLE IF EXISTS `appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `appointments` (
  `id` int NOT NULL AUTO_INCREMENT,
  `patient_id` int NOT NULL,
  `doctor_id` int NOT NULL,
  `schedule_slot_id` int NOT NULL,
  `appointment_date` date NOT NULL,
  `start_time` time NOT NULL,
  `end_time` time NOT NULL,
  `symptoms` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `symptom_analysis` json DEFAULT NULL,
  `age_score` decimal(4,2) DEFAULT '0.00',
  `condition_score` decimal(4,2) DEFAULT '0.00',
  `symptom_score` decimal(4,2) DEFAULT '0.00',
  `priority_score` decimal(4,2) DEFAULT '0.00',
  `status` enum('BOOKED','CONFIRMED','IN_PROGRESS','COMPLETED','CANCELLED') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'BOOKED',
  `cancelled_at` timestamp NULL DEFAULT NULL,
  `cancel_reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_appt_slot` (`schedule_slot_id`),
  KEY `idx_patient` (`patient_id`),
  KEY `idx_doctor` (`doctor_id`),
  KEY `idx_date` (`appointment_date`),
  KEY `idx_status` (`status`),
  KEY `idx_priority` (`priority_score`),
  CONSTRAINT `fk_appt_doctor` FOREIGN KEY (`doctor_id`) REFERENCES `doctors` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_appt_patient` FOREIGN KEY (`patient_id`) REFERENCES `patients` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_appt_slot` FOREIGN KEY (`schedule_slot_id`) REFERENCES `schedule_slots` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `appointments`
--

LOCK TABLES `appointments` WRITE;
/*!40000 ALTER TABLE `appointments` DISABLE KEYS */;
INSERT INTO `appointments` VALUES (1,1,5,1,'2026-01-06','08:00:00','08:30:00','Mắt bị đỏ, ngứa, chảy nước mắt nhiều',NULL,30.00,25.00,20.00,75.00,'CANCELLED','2026-01-15 05:52:22','Tự động hủy - Đã quá giờ khám','2026-01-15 05:50:41','2026-01-15 05:52:22'),(2,2,1,11,'2026-01-07','08:00:00','08:30:00','Đau khớp gối, sưng, khó vận động',NULL,35.00,30.00,20.00,85.00,'CANCELLED','2026-01-15 05:52:22','Tự động hủy - Đã quá giờ khám','2026-01-15 05:50:41','2026-01-15 05:52:22'),(3,3,3,8,'2026-01-06','08:00:00','08:30:00','Nghẹt mũi, chảy nước mũi vàng, đau đầu',NULL,20.00,25.00,20.00,65.00,'CANCELLED','2026-01-15 05:52:22','Tự động hủy - Đã quá giờ khám','2026-01-15 05:50:41','2026-01-15 05:52:22'),(4,4,2,14,'2026-01-07','13:00:00','13:30:00','Khó thở, ho có đờm, tức ngực',NULL,40.00,30.00,20.00,90.00,'IN_PROGRESS',NULL,NULL,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(5,1,5,2,'2026-01-06','08:30:00','09:00:00','Kiểm tra thị lực định kỳ',NULL,30.00,10.00,10.00,50.00,'COMPLETED',NULL,NULL,'2026-01-15 05:50:41','2026-01-15 05:50:41');
/*!40000 ALTER TABLE `appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `clinics`
--

DROP TABLE IF EXISTS `clinics`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clinics` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Tên bệnh viện/phòng khám',
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Mã định danh clinic (VD: TC001, VN123)',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Mô tả về clinic',
  `address` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Địa chỉ chính',
  `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Số điện thoại liên hệ',
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Email liên hệ',
  `website` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Website',
  `logo_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Logo của clinic',
  `is_active` tinyint(1) DEFAULT '1' COMMENT 'Clinic có đang hoạt động không',
  `settings` json DEFAULT NULL COMMENT 'Cấu hình riêng của clinic (JSON)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`),
  KEY `idx_code` (`code`),
  KEY `idx_is_active` (`is_active`),
  KEY `idx_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng quản lý các bệnh viện/phòng khám';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clinics`
--

LOCK TABLES `clinics` WRITE;
/*!40000 ALTER TABLE `clinics` DISABLE KEYS */;
INSERT INTO `clinics` VALUES (1,'Bệnh viện Thu Cúc','TC001','Bệnh viện đa khoa Thu Cúc','286-294 Thụy Khuê, Tây Hồ, Hà Nội','024-7301-2468','info@thucuc.vn','https://thucuc.vn','https://icolor.vn/wp-content/uploads/2021/03/H%E1%BB%87-th%E1%BB%91ng-y-t%E1%BA%BF-Thu-C%C3%BAc-c%C3%B4ng-b%E1%BB%91-thay-%C4%91%E1%BB%95i-nh%E1%BA%ADn-di%E1%BB%87n-m%E1%BB%9Bi-TCI-1.png',1,NULL,'2026-01-16 02:40:52','2026-01-21 09:59:01'),(2,'Phòng khám Đa khoa Quốc tế','PK001','Phòng khám đa khoa quốc tế chất lượng cao','123 Nguyễn Huệ, Q1, TP.HCM','028-3829-0000','info@pkqt.vn','https://pkqt.vn','https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTumQIFf7C5ZQuXwUR-E_AkFwNa_DdZdaXOqw&s',1,NULL,'2026-01-16 02:40:52','2026-01-21 09:59:20');
/*!40000 ALTER TABLE `clinics` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `doctor_specialties`
--

DROP TABLE IF EXISTS `doctor_specialties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doctor_specialties` (
  `id` int NOT NULL AUTO_INCREMENT,
  `doctor_id` int NOT NULL,
  `specialty_id` int NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_doctor_specialty` (`doctor_id`,`specialty_id`),
  KEY `fk_docspec_specialty` (`specialty_id`),
  CONSTRAINT `fk_docspec_doctor` FOREIGN KEY (`doctor_id`) REFERENCES `doctors` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_docspec_specialty` FOREIGN KEY (`specialty_id`) REFERENCES `specialties` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doctor_specialties`
--

LOCK TABLES `doctor_specialties` WRITE;
/*!40000 ALTER TABLE `doctor_specialties` DISABLE KEYS */;
INSERT INTO `doctor_specialties` VALUES (1,1,3,'2026-01-15 05:50:41'),(2,2,4,'2026-01-15 05:50:41'),(3,3,2,'2026-01-15 05:50:41'),(4,4,5,'2026-01-15 05:50:41'),(5,5,1,'2026-01-15 05:50:41'),(6,6,1,'2026-01-15 05:50:41'),(7,7,2,'2026-01-15 05:50:41'),(8,8,3,'2026-01-15 05:50:41'),(9,9,4,'2026-01-15 05:50:41'),(10,10,5,'2026-01-15 05:50:41'),(11,11,6,'2026-01-15 05:50:41'),(12,12,6,'2026-01-15 05:50:41');
/*!40000 ALTER TABLE `doctor_specialties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `doctors`
--

DROP TABLE IF EXISTS `doctors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doctors` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `full_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `qualification` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `experience_years` int DEFAULT '0',
  `bio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `consultation_fee` decimal(10,2) DEFAULT '0.00' COMMENT 'Phí khám bệnh (VNĐ)',
  `avatar_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `doctor_type` enum('IN_HOUSE','EXTERNAL') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'IN_HOUSE' COMMENT 'IN_HOUSE: Bác sĩ của viện, EXTERNAL: Bác sĩ thuê ngoài (tự đăng ký lịch)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id` (`user_id`),
  KEY `idx_doctor_type` (`doctor_type`),
  CONSTRAINT `fk_doctors_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doctors`
--

LOCK TABLES `doctors` WRITE;
/*!40000 ALTER TABLE `doctors` DISABLE KEYS */;
INSERT INTO `doctors` VALUES (1,3,'BS. Lê Đức Anh','0911111111','CKII Cơ Xương Khớp',12,'Chuyên gia cơ xương khớp',300000.00,'/uploads/doctors/nam1.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(2,4,'BS. Nguyễn Minh Tâm','0922222222','TS Hô hấp',15,'Chuyên gia hô hấp',350000.00,'/uploads/doctors/nam2.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(3,5,'BS. Trần Hải Yến','0933333333','ThS TMH',10,'Chuyên khoa tai mũi họng',250000.00,'/uploads/doctors/nu1.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(4,6,'BS. Phạm Thị Mai','0944444444','TS Thần kinh',8,'Chuyên khoa thần kinh',350000.00,'/uploads/doctors/nu2.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(5,7,'BS. Hoàng Văn Nam','0955555555','ThS Mắt',7,'Chuyên khoa mắt',250000.00,'/uploads/doctors/nam3.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(6,8,'BS. Trương Thanh Hằng','0966666666','CKI Mắt',9,'Chuyên khoa mắt',200000.00,'/uploads/doctors/nu3.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(7,9,'BS. Võ Minh Quân','0977777777','ThS TMH',11,'Chuyên khoa TMH',250000.00,'/uploads/doctors/nam4.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(8,10,'BS. Đặng Thị Lan','0988888888','CKII Cơ Xương Khớp',13,'Chuyên gia cơ xương khớp',300000.00,'/uploads/doctors/nu4.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(9,11,'BS. Phan Văn Hùng','0999999999','TS Hô hấp',14,'Chuyên gia hô hấp',350000.00,'/uploads/doctors/nam5.jpg',1,'IN_HOUSE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(10,12,'BS. Mai Xuân Thảo','0900000000','PGS.TS Thần kinh',20,'Chuyên gia thần kinh',500000.00,'/uploads/doctors/nam6.jpg',1,'EXTERNAL','2026-01-15 05:50:41','2026-01-15 05:50:41'),(11,13,'BS. Lý Thanh Tùng','0901111111','ThS RHM',10,'Chuyên khoa răng hàm mặt',250000.00,'/uploads/doctors/nam7.jpg',1,'EXTERNAL','2026-01-15 05:50:41','2026-01-15 05:50:41'),(12,14,'BS. Ngô Thị Hoa','0902222222','CKI RHM',8,'Chuyên khoa răng hàm mặt',200000.00,'/uploads/doctors/nu5.jpg',1,'EXTERNAL','2026-01-15 05:50:41','2026-01-15 05:50:41');
/*!40000 ALTER TABLE `doctors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `medical_records`
--

DROP TABLE IF EXISTS `medical_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `medical_records` (
  `id` int NOT NULL AUTO_INCREMENT,
  `appointment_id` int NOT NULL,
  `diagnosis` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `prescription` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `notes` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_records_appt` (`appointment_id`),
  CONSTRAINT `fk_records_appt` FOREIGN KEY (`appointment_id`) REFERENCES `appointments` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `medical_records`
--

LOCK TABLES `medical_records` WRITE;
/*!40000 ALTER TABLE `medical_records` DISABLE KEYS */;
INSERT INTO `medical_records` VALUES (1,5,'Viêm kết mạc dị ứng','Thuốc nhỏ mắt Systane Ultra 10ml - Ngày 4 lần\nThuốc kháng histamin Cetirizine 10mg - Ngày 1 viên','Tránh tiếp xúc với bụi bẩn, khói thuốc. Tái khám sau 1 tuần nếu không giảm.','2026-01-15 05:50:41','2026-01-15 05:50:41');
/*!40000 ALTER TABLE `medical_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `patients`
--

DROP TABLE IF EXISTS `patients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `patients` (
  `id` int NOT NULL AUTO_INCREMENT,
  `patient_code` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `user_id` int NOT NULL,
  `full_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `age` int DEFAULT NULL,
  `gender` enum('MALE','FEMALE','OTHER') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `phone` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `address` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `underlying_conditions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `insurance_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Mã bảo hiểm y tế',
  `id_card` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Số CMND/CCCD',
  `ethnicity` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Dân tộc',
  `occupation` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Nghề nghiệp',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id` (`user_id`),
  UNIQUE KEY `patient_code` (`patient_code`),
  CONSTRAINT `fk_patients_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `patients`
--

LOCK TABLES `patients` WRITE;
/*!40000 ALTER TABLE `patients` DISABLE KEYS */;
INSERT INTO `patients` VALUES (1,'YMP123456789',15,'Nguyễn Văn An','1990-05-13',34,'MALE','0987654321','123 Nguyễn Huệ, Q1, TP.HCM','Không có',NULL,NULL,NULL,NULL,'2026-01-15 05:50:41','2026-01-21 14:29:13'),(2,'YMP246913578',16,'Trần Thị Bình','1985-08-20',39,'FEMALE','0987654322','456 Lê Lợi, Q3, TP.HCM','Tiểu đường',NULL,NULL,NULL,NULL,'2026-01-15 05:50:41','2026-01-21 12:47:09'),(3,'YMP370370367',17,'Lê Minh Cường','2000-12-10',24,'MALE','0987654323','789 Trần Hưng Đạo, Q5, TP.HCM','Không có',NULL,NULL,NULL,NULL,'2026-01-15 05:50:41','2026-01-21 12:47:09'),(4,'YMP493827156',18,'Phạm Thị Dung','1975-03-25',49,'MALE','0987654324','321 Hai Bà Trưng, Q1, TP.HCM','Huyết áp cao',NULL,NULL,NULL,NULL,'2026-01-15 05:50:41','2026-01-21 12:47:09'),(5,'YMP617283945',29,'Nguyễn Văn An','1990-05-15',34,'MALE','0373940511','123 Nguyễn Huệ, Q1, Hà Nội','Không có',NULL,NULL,NULL,NULL,'2026-01-16 02:40:53','2026-01-21 12:47:09'),(6,'YMP740740734',30,'Trần Thị Bình','1985-08-20',39,'FEMALE','0987654322','456 Lê Lợi, Cầu Giấy, Hà Nội','Tiểu đường',NULL,NULL,NULL,NULL,'2026-01-16 02:40:53','2026-01-21 12:47:09'),(7,'YMP864197523',31,'Lê Minh Cường','2000-12-10',24,'MALE','0987654323','789 Trần Hưng Đạo, Q1, TP.HCM','Không có',NULL,NULL,NULL,NULL,'2026-01-16 02:40:53','2026-01-21 12:47:09'),(8,'YMP987654312',32,'Phạm Thị Dung','1975-03-25',49,'FEMALE','0987654324','321 Hai Bà Trưng, Q3, TP.HCM','Huyết áp cao',NULL,NULL,NULL,NULL,'2026-01-16 02:40:53','2026-01-21 12:47:09');
/*!40000 ALTER TABLE `patients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ratings`
--

DROP TABLE IF EXISTS `ratings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ratings` (
  `id` int NOT NULL AUTO_INCREMENT,
  `appointment_id` int NOT NULL,
  `patient_id` int NOT NULL,
  `doctor_id` int NOT NULL,
  `rating` int NOT NULL,
  `comment` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_appointment_rating` (`appointment_id`),
  KEY `idx_doctor_rating` (`doctor_id`,`rating`),
  KEY `idx_patient_rating` (`patient_id`),
  CONSTRAINT `fk_ratings_appt` FOREIGN KEY (`appointment_id`) REFERENCES `appointments` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_ratings_doctor` FOREIGN KEY (`doctor_id`) REFERENCES `doctors` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_ratings_patient` FOREIGN KEY (`patient_id`) REFERENCES `patients` (`id`) ON DELETE CASCADE,
  CONSTRAINT `chk_rating_range` CHECK ((`rating` between 1 and 5))
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ratings`
--

LOCK TABLES `ratings` WRITE;
/*!40000 ALTER TABLE `ratings` DISABLE KEYS */;
INSERT INTO `ratings` VALUES (1,5,1,5,5,NULL,'2026-01-15 06:24:00','2026-01-15 06:24:00');
/*!40000 ALTER TABLE `ratings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rooms`
--

DROP TABLE IF EXISTS `rooms`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rooms` (
  `id` int NOT NULL AUTO_INCREMENT,
  `room_number` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Mã phòng khám (VD: P101, P102)',
  `specialty_id` int NOT NULL COMMENT 'Chuyên khoa sử dụng phòng này',
  `room_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Tên phòng khám',
  `floor` int DEFAULT NULL COMMENT 'Tầng (VD: 1, 2, 3)',
  `is_active` tinyint(1) DEFAULT '1' COMMENT 'Phòng có đang hoạt động không',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_room_number` (`room_number`),
  KEY `idx_specialty` (`specialty_id`),
  CONSTRAINT `fk_rooms_specialty` FOREIGN KEY (`specialty_id`) REFERENCES `specialties` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Quản lý phòng khám theo chuyên khoa';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rooms`
--

LOCK TABLES `rooms` WRITE;
/*!40000 ALTER TABLE `rooms` DISABLE KEYS */;
INSERT INTO `rooms` VALUES (1,'P101',1,'Phòng Khám Mắt 1',1,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(2,'P102',1,'Phòng Khám Mắt 2',1,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(3,'P103',2,'Phòng Khám TMH 1',1,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(4,'P104',2,'Phòng Khám TMH 2',1,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(5,'P201',3,'Phòng Khám Cơ Xương Khớp 1',2,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(6,'P202',3,'Phòng Khám Cơ Xương Khớp 2',2,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(7,'P203',3,'Phòng Khám Cơ Xương Khớp 3',2,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(8,'P204',4,'Phòng Khám Hô Hấp 1',2,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(9,'P205',4,'Phòng Khám Hô Hấp 2',2,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(10,'P301',5,'Phòng Khám Thần Kinh 1',3,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(11,'P302',5,'Phòng Khám Thần Kinh 2',3,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(12,'P303',6,'Phòng Khám RHM 1',3,1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(13,'P304',6,'Phòng Khám RHM 2',3,1,'2026-01-15 05:50:41','2026-01-15 05:50:41');
/*!40000 ALTER TABLE `rooms` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `schedule_slots`
--

DROP TABLE IF EXISTS `schedule_slots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `schedule_slots` (
  `id` int NOT NULL AUTO_INCREMENT,
  `schedule_id` int NOT NULL,
  `doctor_id` int NOT NULL,
  `date` date NOT NULL,
  `shift` enum('MORNING','AFTERNOON') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `room_number` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Phòng khám mà bác sĩ đăng ký',
  `start_time` time NOT NULL,
  `end_time` time NOT NULL,
  `max_patients` int DEFAULT '5',
  `current_patients` int DEFAULT '0',
  `is_full` tinyint(1) DEFAULT '0',
  `is_blocked` tinyint(1) DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_room_slot` (`room_number`,`date`,`start_time`,`end_time`),
  KEY `fk_slots_schedule` (`schedule_id`),
  KEY `fk_slots_doctor` (`doctor_id`),
  KEY `idx_room_date_time` (`room_number`,`date`,`start_time`,`end_time`),
  CONSTRAINT `fk_slots_doctor` FOREIGN KEY (`doctor_id`) REFERENCES `doctors` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_slots_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `schedules` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `schedule_slots`
--

LOCK TABLES `schedule_slots` WRITE;
/*!40000 ALTER TABLE `schedule_slots` DISABLE KEYS */;
INSERT INTO `schedule_slots` VALUES (1,1,5,'2026-01-06','MORNING','P101','08:00:00','08:30:00',3,1,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(2,1,5,'2026-01-06','MORNING','P101','08:30:00','09:00:00',3,1,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(3,1,5,'2026-01-06','MORNING','P101','09:00:00','09:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(4,1,5,'2026-01-06','MORNING','P101','09:30:00','10:00:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(5,2,6,'2026-01-06','AFTERNOON','P102','13:00:00','13:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(6,2,6,'2026-01-06','AFTERNOON','P102','13:30:00','14:00:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(7,2,6,'2026-01-06','AFTERNOON','P102','14:00:00','14:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(8,3,3,'2026-01-06','MORNING','P103','08:00:00','08:30:00',3,1,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(9,3,3,'2026-01-06','MORNING','P103','08:30:00','09:00:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(10,3,3,'2026-01-06','MORNING','P103','09:00:00','09:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(11,4,1,'2026-01-07','MORNING','P201','08:00:00','08:30:00',3,1,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(12,4,1,'2026-01-07','MORNING','P201','08:30:00','09:00:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(13,4,1,'2026-01-07','MORNING','P201','09:00:00','09:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(14,5,2,'2026-01-07','AFTERNOON','P204','13:00:00','13:30:00',3,1,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(15,5,2,'2026-01-07','AFTERNOON','P204','13:30:00','14:00:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(16,5,2,'2026-01-07','AFTERNOON','P204','14:00:00','14:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(17,1,5,'2026-01-08','MORNING','P101','08:00:00','08:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(18,1,5,'2026-01-08','MORNING','P101','08:30:00','09:00:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(19,1,5,'2026-01-08','MORNING','P101','09:00:00','09:30:00',3,0,0,0,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(20,6,1,'2026-01-16','MORNING','P201','08:00:00','08:30:00',3,0,0,0,'2026-01-15 15:13:58','2026-01-15 15:13:58'),(21,6,1,'2026-01-16','MORNING','P201','08:30:00','09:00:00',3,0,0,0,'2026-01-15 15:13:58','2026-01-15 15:13:58'),(22,6,1,'2026-01-16','MORNING','P201','09:00:00','09:30:00',3,0,0,0,'2026-01-15 15:13:58','2026-01-15 15:13:58'),(23,6,1,'2026-01-16','MORNING','P201','09:30:00','10:00:00',3,0,0,0,'2026-01-15 15:13:58','2026-01-15 15:13:58'),(24,6,1,'2026-01-16','MORNING','P201','10:00:00','10:30:00',3,0,0,0,'2026-01-15 15:13:58','2026-01-15 15:13:58'),(25,6,1,'2026-01-16','MORNING','P201','10:30:00','11:00:00',3,0,0,0,'2026-01-15 15:13:58','2026-01-15 15:13:58'),(32,7,1,'2026-01-17','MORNING','P201','08:00:00','08:30:00',3,0,0,0,'2026-01-15 15:14:22','2026-01-15 15:14:22'),(33,7,1,'2026-01-17','MORNING','P201','08:30:00','09:00:00',3,0,0,0,'2026-01-15 15:14:22','2026-01-15 15:14:22'),(34,7,1,'2026-01-17','MORNING','P201','09:00:00','09:30:00',3,0,0,0,'2026-01-15 15:14:22','2026-01-15 15:14:22'),(35,7,1,'2026-01-17','MORNING','P201','09:30:00','10:00:00',3,0,0,0,'2026-01-15 15:14:22','2026-01-15 15:14:22'),(36,7,1,'2026-01-17','MORNING','P201','10:00:00','10:30:00',3,0,0,0,'2026-01-15 15:14:22','2026-01-15 15:14:22'),(37,7,1,'2026-01-17','MORNING','P201','10:30:00','11:00:00',3,0,0,0,'2026-01-15 15:14:22','2026-01-15 15:14:22');
/*!40000 ALTER TABLE `schedule_slots` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `schedules`
--

DROP TABLE IF EXISTS `schedules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `schedules` (
  `id` int NOT NULL AUTO_INCREMENT,
  `doctor_id` int NOT NULL,
  `day_of_week` enum('MONDAY','TUESDAY','WEDNESDAY','THURSDAY','FRIDAY','SATURDAY','SUNDAY') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `start_time` time DEFAULT NULL,
  `end_time` time DEFAULT NULL,
  `slot_duration` int DEFAULT '30',
  `max_patients_per_slot` int DEFAULT '3',
  `week_start_date` date DEFAULT NULL,
  `week_end_date` date DEFAULT NULL,
  `status` enum('ACTIVE','COMPLETED','CANCELLED') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'ACTIVE',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_doctor_day` (`doctor_id`,`day_of_week`),
  CONSTRAINT `fk_schedules_doctor` FOREIGN KEY (`doctor_id`) REFERENCES `doctors` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `schedules`
--

LOCK TABLES `schedules` WRITE;
/*!40000 ALTER TABLE `schedules` DISABLE KEYS */;
INSERT INTO `schedules` VALUES (1,5,NULL,NULL,NULL,30,3,'2026-01-06','2026-01-12','ACTIVE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(2,6,NULL,NULL,NULL,30,3,'2026-01-06','2026-01-12','ACTIVE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(3,3,NULL,NULL,NULL,30,3,'2026-01-06','2026-01-12','ACTIVE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(4,1,NULL,NULL,NULL,30,3,'2026-01-06','2026-01-12','ACTIVE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(5,2,NULL,NULL,NULL,30,3,'2026-01-06','2026-01-12','ACTIVE','2026-01-15 05:50:41','2026-01-15 05:50:41'),(6,1,'FRIDAY',NULL,NULL,30,3,NULL,NULL,'ACTIVE','2026-01-15 15:13:58','2026-01-15 15:13:58'),(7,1,'SATURDAY',NULL,NULL,30,3,NULL,NULL,'ACTIVE','2026-01-15 15:14:22','2026-01-15 15:14:22');
/*!40000 ALTER TABLE `schedules` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `specialties`
--

DROP TABLE IF EXISTS `specialties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `specialties` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `icon` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `image_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `specialties`
--

LOCK TABLES `specialties` WRITE;
/*!40000 ALTER TABLE `specialties` DISABLE KEYS */;
INSERT INTO `specialties` VALUES (1,'Mắt','Chuyên khoa mắt - Khám và điều trị các bệnh về mắt','?️','/uploads/specialties/mat.png',1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(2,'Tai Mũi Họng','Chuyên khoa tai mũi họng - Điều trị bệnh lý tai, mũi, họng','?','/uploads/specialties/tmh.png',1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(3,'Cơ xương khớp','Chuyên khoa cơ xương khớp - Điều trị bệnh lý cơ, xương, khớp','?','/uploads/specialties/coxuongkhop.png',1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(4,'Hô hấp','Chuyên khoa hô hấp - Điều trị bệnh lý đường hô hấp','?','/uploads/specialties/hohap.png',1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(5,'Thần kinh','Chuyên khoa thần kinh - Điều trị bệnh lý hệ thần kinh','?','/uploads/specialties/thankinh.png',1,'2026-01-15 05:50:41','2026-01-15 05:50:41'),(6,'Răng Hàm Mặt','Chuyên khoa răng hàm mặt - Nha khoa tổng quát','?','/uploads/specialties/rhm.png',1,'2026-01-15 05:50:41','2026-01-15 05:50:41');
/*!40000 ALTER TABLE `specialties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `role` enum('PATIENT','DOCTOR','RECEPTIONIST','ADMIN','SUPER_ADMIN') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `clinic_id` int DEFAULT NULL COMMENT 'Clinic mà user thuộc về (NULL cho SUPER_ADMIN)',
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`),
  KEY `idx_email` (`email`),
  KEY `idx_role` (`role`),
  KEY `idx_clinic_id` (`clinic_id`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`clinic_id`) REFERENCES `clinics` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'admin@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','ADMIN',1,'2026-01-15 05:50:41','2026-01-16 07:07:05',1),(2,'receptionist@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','RECEPTIONIST',1,'2026-01-15 05:50:41','2026-01-16 07:07:31',1),(3,'doctor1@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:51',1),(4,'doctor2@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:15:08',1),(5,'doctor3@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:42',1),(6,'doctor4@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:19',1),(7,'doctor5@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:26',2),(8,'doctor6@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:14',2),(9,'doctor7@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:09',2),(10,'doctor8@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:14:05',2),(11,'doctor9@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:13:57',2),(12,'doctor10@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:13:52',1),(13,'doctor11@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:13:43',1),(14,'doctor12@clinic.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-15 05:50:41','2026-01-16 07:13:39',1),(15,'patient1@gmail.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-15 05:50:41','2026-01-16 07:13:33',1),(16,'patient2@gmail.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-15 05:50:41','2026-01-16 07:13:28',1),(17,'patient3@gmail.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-15 05:50:41','2026-01-16 07:07:48',1),(18,'patient4@gmail.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-15 05:50:41','2026-01-16 07:07:43',1),(19,'superadmin@system.com','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','SUPER_ADMIN',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',NULL),(20,'admin@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','ADMIN',1,'2026-01-16 02:40:53','2026-01-16 06:52:33',1),(21,'admin@pkqt.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','ADMIN',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',2),(22,'receptionist@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','RECEPTIONIST',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',1),(23,'receptionist@pkqt.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','RECEPTIONIST',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',2),(24,'doctor1@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',1),(25,'doctor2@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',1),(26,'doctor3@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',1),(27,'doctor1@pkqt.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',2),(28,'doctor2@pkqt.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','DOCTOR',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',2),(29,'patient1@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',1),(30,'patient2@thucuc.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',1),(31,'patient1@pkqt.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',2),(32,'patient2@pkqt.vn','$2a$10$cjhEa4wMT6b5/VLljTVxtu1PJ1vV7Vdlw6bJ4UIuwTQoawFHKCodq','PATIENT',1,'2026-01-16 02:40:53','2026-01-16 02:40:53',2);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-01-21 21:39:12
