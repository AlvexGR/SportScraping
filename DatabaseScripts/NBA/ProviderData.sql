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
-- Dumping data for table `provider`
--

LOCK TABLES `provider` WRITE;
/*!40000 ALTER TABLE `provider` DISABLE KEYS */;
INSERT INTO `provider` VALUES (5,'EspnCompetition','Espn',0,0,'AU','https://www.espn.com.au','NBA_NationalBasketballAssociation'),(6,'BetEasyPlayerOverUnder','BetEasy',1,1,'AU','https://beteasy.com.au','NBA_NationalBasketballAssociation'),(7,'TopSportPlayerOverUnder','TopSport',1,1,'AU','https://www.topsport.com.au','NBA_NationalBasketballAssociation'),(8,'Bet365PlayerOverUnder','Bet365',1,1,'AU','https://www.bet365.com.au','NBA_NationalBasketballAssociation'),(9,'NedsPlayerOverUnder','Neds',1,1,'AU','https://www.neds.com.au','NBA_NationalBasketballAssociation'),(10,'TabPlayerOverUnder','Tab',1,1,'AU','https://www.tab.com.au','NBA_NationalBasketballAssociation'),(11,'UbetPlayerOverUnder','Ubet',1,1,'AU','https://tab.ubet.com','NBA_NationalBasketballAssociation'),(12,'PalmerbetPlayerOverUnder','Palmerbet',1,1,'AU','https://www.palmerbet.com','NBA_NationalBasketballAssociation'),(13,'SportsBetPlayerOverUnder','SportsBet',1,1,'AU','https://www.sportsbet.com.au','NBA_NationalBasketballAssociation'),(14,'PointsBetPlayerOverUnder','PointsBet',1,1,'AU','https://pointsbet.com.au','NBA_NationalBasketballAssociation'),(15,'KambiBePlayerOverUnder','KambiBe',1,1,'AU','https://www.888sport.com','NBA_NationalBasketballAssociation'),(16,'BorgataonlinePlayerOverUnder','Borgataonline',1,1,'US','https://sports.borgataonline.com','NBA_NationalBasketballAssociation'),(17,'BetAmericaPlayerOverUnder','BetAmerica',1,1,'US','https://nj.betamerica.com','NBA_NationalBasketballAssociation'),(18,'BetVictorPlayerOverUnder','BetVictor',1,1,'US','https://www.betvictor.com','NBA_NationalBasketballAssociation'),(19,'BovadaPlayerOverUnder','Bovada',1,1,'US','https://www.bovada.lv','NBA_NationalBasketballAssociation'),(20,'TwoTwoBetPlayerOverUnder','TwoTwoBet',1,1,'US','https://22bet.co.uk','NBA_NationalBasketballAssociation'),(21,'BetUsPlayerOverUnder','BetUs',1,1,'US','https://www.betus.com.pa','NBA_NationalBasketballAssociation'),(22,'NextBetPlayerOverUnder','NextBet',1,1,'US','https://www.nextbet.com','NBA_NationalBasketballAssociation'),(23,'FiveDimesPlayerOverUnder','FiveDimes',1,1,'US','https://www.5dimes.eu','NBA_NationalBasketballAssociation'),(25,'EspnFutureCompetition','Espn Future',0,0,'AU','https://www.espn.com.au','NBA_NationalBasketballAssociation'),(26,'TabPlayerHeadToHead','Tab',2,1,'AU','https://www.tab.com.au','NBA_NationalBasketballAssociation');
/*!40000 ALTER TABLE `provider` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-03-01 10:30:37
