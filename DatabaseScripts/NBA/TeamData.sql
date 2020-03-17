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
-- Dumping data for table `team`
--

LOCK TABLES `team` WRITE;
/*!40000 ALTER TABLE `team` DISABLE KEYS */;
INSERT INTO `team` VALUES (1,'Boston Celtics','BOS','NBA_NationalBasketballAssociation'),(2,'Brooklyn Nets','BKN','NBA_NationalBasketballAssociation'),(3,'New York Knicks','NY','NBA_NationalBasketballAssociation'),(4,'Philadelphia 76ers','PHI','NBA_NationalBasketballAssociation'),(5,'Toronto Raptors','TOR','NBA_NationalBasketballAssociation'),(6,'Golden State Warriors','GS','NBA_NationalBasketballAssociation'),(7,'LA Clippers','LAC','NBA_NationalBasketballAssociation'),(8,'Los Angeles Lakers','LAL','NBA_NationalBasketballAssociation'),(9,'Phoenix Suns','PHX','NBA_NationalBasketballAssociation'),(10,'Sacramento Kings','SAC','NBA_NationalBasketballAssociation'),(11,'Chicago Bulls','CHI','NBA_NationalBasketballAssociation'),(12,'Cleveland Cavaliers','CLE','NBA_NationalBasketballAssociation'),(13,'Detroit Pistons','DET','NBA_NationalBasketballAssociation'),(14,'Indiana Pacers','IND','NBA_NationalBasketballAssociation'),(15,'Milwaukee Bucks','MIL','NBA_NationalBasketballAssociation'),(16,'Atlanta Hawks','ATL','NBA_NationalBasketballAssociation'),(17,'Charlotte Hornets','CHA','NBA_NationalBasketballAssociation'),(18,'Miami Heat','MIA','NBA_NationalBasketballAssociation'),(19,'Orlando Magic','ORL','NBA_NationalBasketballAssociation'),(20,'Washington Wizards','WSH','NBA_NationalBasketballAssociation'),(21,'Denver Nuggets','DEN','NBA_NationalBasketballAssociation'),(22,'Minnesota Timberwolves','MIN','NBA_NationalBasketballAssociation'),(23,'Oklahoma City Thunder','OKC','NBA_NationalBasketballAssociation'),(24,'Portland Trail Blazers','POR','NBA_NationalBasketballAssociation'),(25,'Utah Jazz','UTAH','NBA_NationalBasketballAssociation'),(26,'Dallas Mavericks','DAL','NBA_NationalBasketballAssociation'),(27,'Houston Rockets','HOU','NBA_NationalBasketballAssociation'),(28,'Memphis Grizzlies','MEM','NBA_NationalBasketballAssociation'),(29,'New Orleans Pelicans','NO','NBA_NationalBasketballAssociation'),(30,'San Antonio Spurs','SA','NBA_NationalBasketballAssociation');
/*!40000 ALTER TABLE `team` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-03-01 10:30:18
