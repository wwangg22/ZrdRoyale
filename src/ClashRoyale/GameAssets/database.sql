-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Generation Time: Sep 15, 2023 at 09:50 PM
-- Server version: 8.0.34-0ubuntu0.20.04.1
-- PHP Version: 7.4.3-4ubuntu2.19

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `rrdb2`
--

-- --------------------------------------------------------

--
-- Table structure for table `clan`
--

CREATE TABLE `clan` (
  `Id` bigint NOT NULL,
  `Trophies` bigint NOT NULL,
  `RequiredTrophies` bigint NOT NULL,
  `Type` bigint NOT NULL,
  `Region` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Data` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

--
-- Dumping data for table `clan`
--

INSERT INTO `clan` (`Id`, `Trophies`, `RequiredTrophies`, `Type`, `Region`, `Data`) VALUES
(1, 0, 0, 1, '6', '{\"members\":[{\"lowId\":1,\"role\":2,\"score\":434,\"name\":\"Draword\"},{\"lowId\":2,\"role\":1,\"score\":248,\"name\":\"Jeanly\"}],\"stream\":[{\"$type\":\"ClashRoyale.Logic.Clan.StreamEntry.Entries.ChatStreamEntry, ClashRoyale\",\"creation\":\"2019-04-12T00:35:54.1073255Z\",\"id\":1555029354,\"msg\":\"/upgrade\",\"type\":2,\"lowId\":1,\"sender_name\":\"Draword\",\"sender_role\":2},{\"$type\":\"ClashRoyale.Logic.Clan.StreamEntry.Entries.ChatStreamEntry, ClashRoyale\",\"creation\":\"2019-04-12T00:36:05.2681537Z\",\"id\":1555029365,\"msg\":\"\\\\upgrade\",\"type\":2,\"lowId\":1,\"sender_name\":\"Draword\",\"sender_role\":2},{\"$type\":\"ClashRoyale.Logic.Clan.StreamEntry.Entries.ChatStreamEntry, ClashRoyale\",\"creation\":\"2019-04-12T00:55:48.8158332Z\",\"id\":1555030548,\"msg\":\"Hello World\",\"type\":2,\"lowId\":1,\"sender_name\":\"Draword\",\"sender_role\":2},{\"$type\":\"ClashRoyale.Logic.Clan.StreamEntry.Entries.AllianceEventStreamEntry, ClashRoyale\",\"creation\":\"2019-04-17T12:40:49.1609269Z\",\"id\":1555504849,\"eventType\":3,\"targetLowId\":2,\"targetName\":\"Jeanly\",\"type\":4,\"lowId\":2,\"sender_name\":\"Jeanly\",\"sender_role\":1},{\"$type\":\"ClashRoyale.Logic.Clan.StreamEntry.Entries.ChatStreamEntry, ClashRoyale\",\"creation\":\"2019-04-17T12:43:15.4979741Z\",\"id\":1555504995,\"msg\":\"ccd\",\"type\":2,\"lowId\":2,\"sender_name\":\"Jeanly\",\"sender_role\":1},{\"$type\":\"ClashRoyale.Logic.Clan.StreamEntry.Entries.ChatStreamEntry, ClashRoyale\",\"creation\":\"2019-04-21T02:22:20.2924206Z\",\"id\":1555813340,\"msg\":\"test\",\"type\":2,\"lowId\":2,\"sender_name\":\"Jeanly\",\"sender_role\":1}],\"name\":\"GM\",\"description\":\"Only GM\",\"lowId\":1,\"badge\":12,\"region\":6,\"type\":1}');

-- --------------------------------------------------------

--
-- Table structure for table `player`
--

CREATE TABLE `player` (
  `Id` bigint NOT NULL,
  `Trophies` bigint NOT NULL,
  `Language` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `FacebookId` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  `Home` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Sessions` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

--
-- Dumping data for table `player`
--

INSERT INTO `player` (`Id`, `Trophies`, `Language`, `FacebookId`, `Home`, `Sessions`) VALUES
(4, 0, 'GB', NULL, '{\"Home\":{\"clan_info\":{},\"arena\":{\"arena\":1},\"chests\":{},\"crownChestCooldown\":\"2023-09-15T21:48:10.9876204Z\",\"deck\":[{\"ClassId\":26,\"BattleSpell\":{\"d\":26000000,\"l\":-1}},{\"InstanceId\":1,\"ClassId\":26,\"BattleSpell\":{\"d\":26000001,\"l\":-1}},{\"InstanceId\":2,\"ClassId\":26,\"BattleSpell\":{\"d\":26000002,\"l\":-1}},{\"InstanceId\":3,\"ClassId\":26,\"BattleSpell\":{\"d\":26000003,\"l\":-1}},{\"InstanceId\":4,\"ClassId\":26,\"BattleSpell\":{\"d\":26000004,\"l\":-1}},{\"InstanceId\":5,\"ClassId\":26,\"BattleSpell\":{\"d\":26000005,\"l\":-1}},{\"InstanceId\":6,\"ClassId\":26,\"BattleSpell\":{\"d\":26000006,\"l\":-1}},{\"InstanceId\":7,\"ClassId\":26,\"BattleSpell\":{\"d\":26000007,\"l\":-1}}],\"freeChestTime\":\"2023-09-15T21:48:10.9876218Z\",\"shop\":[{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":26,\"InstanceId\":46,\"Rarity\":3,\"Type\":1},{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":26,\"InstanceId\":23,\"Rarity\":3,\"Type\":1,\"ShopIndex\":1},{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":28,\"InstanceId\":11,\"Rarity\":3,\"Type\":1,\"ShopIndex\":2},{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":28,\"InstanceId\":11,\"Rarity\":3,\"Type\":1,\"ShopIndex\":3}],\"stream\":[],\"DefaultLevel\":1,\"name\":\"NoName\",\"token\":\"moiopmnr7q3o0f3w3f3xpweulqq22e1gil2b5zwf\",\"created_ip\":\"78.11.247.198\",\"low_id\":4,\"language\":\"GB\",\"totalSessions\":1,\"totalPlayTimeSeconds\":4,\"shop_day\":5,\"diamonds\":1000,\"gold\":2500,\"exp_level\":1,\"decks\":[[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007]]}}', '[{\"location\":{\"country\":\"Poland\",\"countryCode\":\"PL\",\"city\":\"Krakow\"},\"ip\":\"78.11.247.198\",\"duration\":4,\"startDate\":\"09/15/2023 21:48:08\",\"deviceCode\":\"LG-K350\",\"gameVersion\":\"3.0\",\"sessionId\":\"4f2d2180-bdd5-4f31-8d47-ee2f11dc00d2\"}]'),
(5, 0, 'GB', NULL, '{\"Home\":{\"clan_info\":{},\"arena\":{\"arena\":1},\"chests\":{},\"crownChestCooldown\":\"2023-09-15T21:49:04.0307129Z\",\"deck\":[{\"ClassId\":26,\"BattleSpell\":{\"d\":26000000,\"l\":-1}},{\"InstanceId\":1,\"ClassId\":26,\"BattleSpell\":{\"d\":26000001,\"l\":-1}},{\"InstanceId\":2,\"ClassId\":26,\"BattleSpell\":{\"d\":26000002,\"l\":-1}},{\"InstanceId\":3,\"ClassId\":26,\"BattleSpell\":{\"d\":26000003,\"l\":-1}},{\"InstanceId\":4,\"ClassId\":26,\"BattleSpell\":{\"d\":26000004,\"l\":-1}},{\"InstanceId\":5,\"ClassId\":26,\"BattleSpell\":{\"d\":26000005,\"l\":-1}},{\"InstanceId\":6,\"ClassId\":26,\"BattleSpell\":{\"d\":26000006,\"l\":-1}},{\"InstanceId\":7,\"ClassId\":26,\"BattleSpell\":{\"d\":26000007,\"l\":-1}}],\"freeChestTime\":\"2023-09-15T21:49:04.0307139Z\",\"shop\":[{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":26,\"InstanceId\":32,\"Rarity\":3,\"Type\":1},{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":28,\"InstanceId\":11,\"Rarity\":3,\"Type\":1,\"ShopIndex\":1},{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":26,\"InstanceId\":37,\"Rarity\":3,\"Type\":1,\"ShopIndex\":2},{\"$type\":\"ClashRoyale.Logic.Home.Shop.Items.SpellShopItem, ClashRoyale\",\"ClassId\":28,\"InstanceId\":11,\"Rarity\":3,\"Type\":1,\"ShopIndex\":3}],\"stream\":[],\"DefaultLevel\":1,\"name\":\"Zordon\",\"token\":\"xbl8f4vrfj6bzy85w4pd22ouas2sdk8qxioz9nul\",\"name_set\":1,\"created_ip\":\"78.11.247.198\",\"low_id\":5,\"language\":\"GB\",\"totalSessions\":1,\"shop_day\":5,\"diamonds\":1000,\"gold\":2500,\"exp_level\":1,\"decks\":[[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007],[26000000,26000001,26000002,26000003,26000004,26000005,26000006,26000007]]}}', '[]');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `clan`
--
ALTER TABLE `clan`
  ADD PRIMARY KEY (`Id`) USING BTREE;

--
-- Indexes for table `player`
--
ALTER TABLE `player`
  ADD PRIMARY KEY (`Id`) USING BTREE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
