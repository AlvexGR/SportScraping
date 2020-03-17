-- MySQL dump 10.13  Distrib 8.0.18, for Win64 (x86_64)
--
-- Host: localhost    Database: sports_scraping
-- ------------------------------------------------------
-- Server version	8.0.18

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
-- Table structure for table `match`
--

DROP TABLE IF EXISTS `match`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `match` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `game_code` varchar(45) NOT NULL,
  `start_time` datetime NOT NULL,
  `sport_code` varchar(145) NOT NULL,
  `home_team_id` int(11) NOT NULL,
  `away_team_id` int(11) NOT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `game_code_UNIQUE` (`game_code`),
  KEY `fk_match_sport_idx` (`sport_code`),
  KEY `fk_match_home_team_idx` (`home_team_id`),
  KEY `fk_match_away_team_idx` (`away_team_id`),
  CONSTRAINT `fk_match_away_team` FOREIGN KEY (`away_team_id`) REFERENCES `team` (`id`),
  CONSTRAINT `fk_match_home_team` FOREIGN KEY (`home_team_id`) REFERENCES `team` (`id`),
  CONSTRAINT `fk_match_sport` FOREIGN KEY (`sport_code`) REFERENCES `sport` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=150 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `metric`
--

DROP TABLE IF EXISTS `metric`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `metric` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `match_id` int(11) NOT NULL,
  `scraping_information_id` int(11) NOT NULL,
  `created_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_match_metric_idx` (`match_id`),
  KEY `fk_scraping_information_metric_idx` (`scraping_information_id`),
  CONSTRAINT `fk_match_metric` FOREIGN KEY (`match_id`) REFERENCES `match` (`id`),
  CONSTRAINT `fk_scraping_information_metric` FOREIGN KEY (`scraping_information_id`) REFERENCES `scraping_information` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10160 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `player`
--

DROP TABLE IF EXISTS `player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `source_id` varchar(45) NOT NULL COMMENT 'This could be duplicated',
  `name` varchar(200) NOT NULL,
  `team_id` int(11) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_player_team_idx` (`team_id`),
  CONSTRAINT `fk_player_team` FOREIGN KEY (`team_id`) REFERENCES `team` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1756 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `player_head_to_head`
--

DROP TABLE IF EXISTS `player_head_to_head`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_head_to_head` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `player_a_id` int(11) NOT NULL,
  `player_b_id` int(11) NOT NULL,
  `player_a_price` double DEFAULT NULL,
  `player_b_price` double DEFAULT NULL,
  `is_tie_included` tinyint(4) NOT NULL,
  `tie_price` double DEFAULT NULL,
  `metric_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_player_head_to_head_metric_idx` (`metric_id`),
  CONSTRAINT `fk_player_head_to_head_metric` FOREIGN KEY (`metric_id`) REFERENCES `metric` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `player_over_under`
--

DROP TABLE IF EXISTS `player_over_under`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_over_under` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `player_id` int(11) NOT NULL,
  `score_type` varchar(45) NOT NULL,
  `over` double DEFAULT NULL,
  `over_line` double DEFAULT NULL COMMENT 'Over vlaue',
  `under` double DEFAULT NULL,
  `under_line` double DEFAULT NULL COMMENT 'Under value',
  `metric_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_player_under_over_metric_idx` (`metric_id`),
  CONSTRAINT `fk_player_under_over_metric` FOREIGN KEY (`metric_id`) REFERENCES `metric` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10123 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `provider`
--

DROP TABLE IF EXISTS `provider`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `provider` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `code` varchar(200) NOT NULL,
  `name` varchar(200) NOT NULL,
  `scrape_type` int(11) NOT NULL COMMENT '0 - Competition\\n1 - PlayerOverUnder\\n2 - PlayerHeadToHead',
  `is_metric` tinyint(4) NOT NULL,
  `country_code` varchar(45) NOT NULL,
  `url` varchar(200) NOT NULL,
  `sport_code` varchar(145) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_provider_sport_idx` (`sport_code`),
  CONSTRAINT `fk_provider_sport` FOREIGN KEY (`sport_code`) REFERENCES `sport` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `scraping_information`
--

DROP TABLE IF EXISTS `scraping_information`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `scraping_information` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `provider_id` int(11) NOT NULL,
  `scrape_time` datetime NOT NULL,
  `scrape_status` int(11) NOT NULL COMMENT '-1 - Failed\n0 - Pending\n1 - Scraping\n2 - Done\n3 - Canceled\n',
  `progress` int(11) NOT NULL,
  `progress_explanation` varchar(5000) DEFAULT NULL,
  `created_at` datetime NOT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_scrape_information_provider_idx` (`provider_id`),
  CONSTRAINT `fk_scrape_information_provider` FOREIGN KEY (`provider_id`) REFERENCES `provider` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=347 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sport`
--

DROP TABLE IF EXISTS `sport`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sport` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `short_name` varchar(45) NOT NULL COMMENT 'Short name of the sport like NBA, C1,...',
  `long_name` varchar(100) NOT NULL COMMENT 'Full name of the sport',
  `code` varchar(145) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code_UNIQUE` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `team`
--

DROP TABLE IF EXISTS `team`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `team` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `long_name` varchar(45) NOT NULL,
  `short_name` varchar(45) NOT NULL,
  `sport_code` varchar(145) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_team_sport_idx` (`sport_code`),
  CONSTRAINT `fk_team_sport` FOREIGN KEY (`sport_code`) REFERENCES `sport` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `temp_table_to_test`
--

DROP TABLE IF EXISTS `temp_table_to_test`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `temp_table_to_test` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(500) DEFAULT NULL,
  `description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-03-01 10:25:39
