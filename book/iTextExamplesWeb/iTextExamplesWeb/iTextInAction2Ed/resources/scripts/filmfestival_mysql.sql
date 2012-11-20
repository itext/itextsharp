CREATE TABLE IF NOT EXISTS `festival_category` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(40) COLLATE utf8_bin NOT NULL,
  `keyword` char(4) COLLATE utf8_bin NOT NULL,
  `parent` int(11) NOT NULL,
  `color` char(6) COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `festival_entry` (
  `film_id` int(11) NOT NULL,
  `year` smallint(4) NOT NULL DEFAULT '2011',
  `category_id` tinyint(1) NOT NULL,
  PRIMARY KEY (`film_id`,`year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `festival_screening` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `day` date NOT NULL,
  `time` time NOT NULL,
  `location` varchar(4) COLLATE utf8_bin NOT NULL,
  `film_id` int(11) NOT NULL DEFAULT '0',
  `press` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `film_country` (
  `id` char(2) COLLATE utf8_bin NOT NULL,
  `country` varchar(60) COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `film_director` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(60) COLLATE utf8_bin NOT NULL,
  `given_name` varchar(60) COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `film_movietitle` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(120) COLLATE utf8_bin NOT NULL,
  `original_title` varchar(120) COLLATE utf8_bin DEFAULT NULL,
  `imdb` varchar(7) COLLATE utf8_bin NOT NULL,
  `year` int(11) NOT NULL,
  `duration` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `film_movie_country` (
  `film_id` int(11) NOT NULL,
  `country_id` char(2) COLLATE utf8_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS `film_movie_director` (
  `film_id` int(11) NOT NULL,
  `director_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
