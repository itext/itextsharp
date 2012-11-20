/*
** SQLite is public domain software (http://www.sqlite.org/copyright.html)
** from System.Data.SQLite.dll (http://sqlite.phxsoftware.com/)
   "There are zero licensing restrictions for private or commercial use."
*/
CREATE TABLE film_movietitle(
  id INTEGER PRIMARY KEY,
  title varchar(120) NOT NULL,
  original_title varchar(120) DEFAULT NULL,
  imdb varchar(7) NOT NULL,
  year INTEGER NOT NULL,
  duration INTEGER NOT NULL
);
CREATE TABLE film_director(
  id INTEGER PRIMARY KEY,
  name varchar(60) NOT NULL,
  given_name varchar(60) NOT NULL
);
CREATE TABLE film_movie_director(
  film_id INTEGER NOT NULL,
  director_id INTEGER NOT NULL
);
CREATE TABLE film_country(
  id CHAR(2) NOT NULL PRIMARY KEY,
  country varchar(60) NOT NULL
);
CREATE TABLE film_movie_country(
  film_id INTEGER NOT NULL,
  country_id CHAR(2) NOT NULL
);
CREATE TABLE festival_category(
  id INTEGER INTEGER PRIMARY KEY,
  name varchar(40) NOT NULL,
  keyword CHAR(4) NOT NULL,
  parent INTEGER NOT NULL,
  color CHAR(6) NOT NULL
);
CREATE TABLE festival_entry(
  film_id INTEGER PRIMARY KEY,
  year INTEGER NOT NULL,
  category_id INTEGER NOT NULL
);
CREATE TABLE festival_screening(
  id INTEGER PRIMARY KEY,
  day CHAR(10) NOT NULL,
  time char(8) NOT NULL,
  location varchar(4) NOT NULL,
  film_id INTEGER NOT NULL,
  press INTEGER NOT NULL
);
