/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

/**
 * A factory that makes it easy to query the database using
 * a series of static methods using System.Data.SQLite:
 * http://sqlite.phxsoftware.com/
 * ###########################################################################
 * System.Data.SQLite is the ADO.NET 2.0/3.5 provider for the
 * SQLite database engine:
 * http://www.sqlite.org/
 * ###########################################################################
 * 
 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 * before July 2010, ADO.NET data provider **DEPENDENT** code was used.
 * this has been fixed. if you want to use a different provider see the 
 * README.txt file. the change requires minimumal effort.
 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 */
namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class PojoFactory {
// =========================================================================== 
    /**
     * Fills a Movie POJO using a DbDataReader.
     * @param rs a DbDataReader with records from table film_movietitle
     * @return a Movie POJO
     */
    public static Movie GetMovie(DbDataReader r) {
      return new Movie() {
        Title = r["title"].ToString(), 
        OriginalTitle = r["original_title"] != null 
            ? r["original_title"].ToString() : "",
        Imdb = r["imdb"].ToString(), 
        Year = Convert.ToInt32(r["year"]),
        Duration = Convert.ToInt32(r["duration"])
      };
    }
// ---------------------------------------------------------------------------
    /**
     * Fills a Director POJO using a DbDataReader.
     * @param rs a DbDataReader with records from table file_director
     * @return a Director POJO
     */
    public static Director GetDirector(DbDataReader r) {
      return new Director() {
        Name = r["name"].ToString(), 
        GivenName = r["given_name"].ToString()
      };
    }
// ---------------------------------------------------------------------------
    /**
     * Fills a Country POJO using a DbDataReader.
     * @param rs a DbDataReader with records from table file_director
     * @return a Country POJO
     */
    public static Country GetCountry(DbDataReader r) {
      return new Country() {
        Name = r["country"].ToString()
      };
    }
// ---------------------------------------------------------------------------
    /**
     * Fills an Entry POJO using a DbDataReader.
     * @param rs a DbDataReader with records from table festival_entry
     * @return an Entry POJO
     */
    public static Entry GetEntry(DbDataReader r) {
      return new Entry() {
        Year = Convert.ToInt32(r["year"])
      };
    }
// ---------------------------------------------------------------------------
    /**
     * Fills a Category POJO using a DbDataReader.
     * @param rs a DbDataReader with records from table festival_category
     * @return a Category POJO
     */
    public static Category GetCategory(DbDataReader r) {
      return new Category() {
        Name = r["name"].ToString(), 
        Keyword = r["keyword"].ToString(),
        color = r["color"].ToString()
      };
    }
// ---------------------------------------------------------------------------
    /**
     * Fills a Screening POJO using a DbDataReader.
     * @param rs a DbDataReader with records from table festival_screening
     * @return a Screening POJO
     */
    public static Screening GetScreening(DbDataReader r)  {
      return new Screening() {
        Date = r["day"].ToString(), 
        Time = r["time"].ToString(),
        Location = r["location"].ToString(), 
        Press = Convert.ToInt32(r["press"]) == 1
      };
    }
// ---------------------------------------------------------------------------
    /**
     * Returns a list with Screening objects
     * @param film_id a movie id
     * @return a List of Screening POJOs
     */
    public static List<Screening> GetScreenings(int film_id) {
      List<Screening> list = new List<Screening>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.MOVIESCREENINGS;
          cmd.Parameters.Add(cmd.CreateParameter());
          cmd.Parameters[0].ParameterName = "@film_id";
          cmd.Parameters[0].Value = film_id;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              list.Add(GetScreening(r));
            }
          }
        }
      }
      return list;
    }    
// ---------------------------------------------------------------------------
    /**
     * Returns a list with Screening objects, if you pass
     * a stringified date.
     * @param day stringified date "yyyy-MM-dd"
     * @return a List of Screening POJOs
     */
    public static List<Screening> GetScreenings(string day)  {
      List<Screening> list = new List<Screening>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.SCREENINGS;
          cmd.Parameters.Add(cmd.CreateParameter());
          cmd.Parameters[0].ParameterName = "@day";
          cmd.Parameters[0].Value = day;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              Screening screening = GetScreening(r);
              Movie movie = GetMovie(r);
              foreach (var d in GetDirectors(Convert.ToInt32(r["id"]))) {
                movie.AddDirector(d);
              }
              foreach (var cn in GetCountries(Convert.ToInt32(r["id"]))) {
                movie.AddCountry(cn);
              }
              Entry entry = GetEntry(r);
              Category category = GetCategory(r);
              entry.category = category;
              entry.movie = movie;
              movie.entry = entry;
              screening.movie = movie;
              list.Add(screening);
            }
          }
        }
      }
      return list;
    }
// ---------------------------------------------------------------------------    
    /**
     * Returns a list with Movie objects.
     */
    public static IEnumerable<Movie> GetMovies() {
      return GetMovies(false);
    }
    /*
     * @param sort_by_year => LINQ sort by movie year
     */
    public static IEnumerable<Movie> GetMovies(bool sort_by_year) {
      List<Movie> list = new List<Movie>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.MOVIES;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              Movie movie = GetMovie(r);
              Entry entry = GetEntry(r);
              Category category = GetCategory(r);
              entry.category = category;
              int film_id = Convert.ToInt32(r["id"]);
              foreach ( Screening screening in GetScreenings(film_id) ) {
                entry.AddScreening(screening);
              }
              movie.entry = entry;
              foreach ( Director director in GetDirectors(film_id) ) {
                movie.AddDirector(director);
              }
              foreach ( Country country in GetCountries(film_id) ) {
                movie.AddCountry(country);
              }
              list.Add(movie);
            }
          }
        }
      }
      if (!sort_by_year) {
        return list;
      }
      else {
        return from m in list orderby m.Year, m.Title select m;
      }
    }    
// ---------------------------------------------------------------------------
    /**
     * Returns a list with Movie objects.
     * @param director_id the id of a director
     * @return a List of Screening POJOs
     */
    // default => return collection
    public static IEnumerable<Movie> GetMovies(int director_id) 
    { return GetMovies(director_id, false);  }
    /*
     * @param sort_by_year => LINQ sort by movie year
     */
    public static IEnumerable<Movie> GetMovies(
        int director_id, bool sort_by_year) 
    {
      List<Movie> list = new List<Movie>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.MOVIEDIRECTORS;
          cmd.Parameters.Add(cmd.CreateParameter());
          cmd.Parameters[0].ParameterName = "@director_id";
          cmd.Parameters[0].Value = director_id;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              list.Add(GetMovie(r));
            }
          }
        }
      }
      if (!sort_by_year) {
        return list;
      }
      else {
        return from m in list orderby m.Year, m.Title select m;      
      }      
    } 
// ---------------------------------------------------------------------------    
    /**
     * Returns a list with Movie objects.
     * @param country_id the id of a country
     * @return a List of Screening POJOs
     */
    // default => return collection
    public static IEnumerable<Movie> GetMovies(string country_id)
    { return GetMovies(country_id, false); }
    /*
     * @param sort_by_year => LINQ sort by movie year
     */
    public static IEnumerable<Movie> GetMovies(
        string country_id, bool sort_by_year
    ) {
      List<Movie> list = new List<Movie>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.MOVIECOUNTRIES;
          cmd.Parameters.Add(cmd.CreateParameter());
          cmd.Parameters[0].ParameterName = "@country_id";
          cmd.Parameters[0].Value = country_id;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              Movie movie = GetMovie(r);
              foreach ( Director d in GetDirectors(Convert.ToInt32(r["id"])) ) {
                movie.AddDirector(d);
              }            
              list.Add(movie);
            }
          }
        }
      }    
      if (!sort_by_year) {
        return list;
      }
      else {
        return from m in list orderby m.Year, m.Title select m;     
      }
    }       
// ---------------------------------------------------------------------------
    /**
     * Returns a list with Country objects.
     * @param connection a connection to the filmfestival database
     * @param movie_id the id of a movie
     * @return a List of Screening POJOs
     */
    public static List<Country> GetCountries(int film_id) {
      List<Country> list = new List<Country>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.COUNTRIES;
          cmd.Parameters.Add(cmd.CreateParameter());
          cmd.Parameters[0].ParameterName = "@film_id";
          cmd.Parameters[0].Value = film_id;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              list.Add(GetCountry(r));
            }
          }
        }
      }
      return list;
    }
// ---------------------------------------------------------------------------
    /**
     * Returns a list with Screening objects
     * @return a List of Screening POJOs
     */
    public static List<Screening> GetPressPreviews() {
      List<Screening> list = new List<Screening>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.PRESS;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              Screening screening = GetScreening(r);
              Movie movie = GetMovie(r);
              int film_id = Convert.ToInt32(r["id"]);
              foreach (Director d in GetDirectors(film_id) ) {
                movie.AddDirector(d);
              }
              foreach ( Country country in GetCountries(film_id) ) {
                movie.AddCountry(country);
              }
              Entry entry = GetEntry(r);
              Category category = GetCategory(r);
              entry.category = category;
              entry.movie = movie;
              movie.entry = entry;
              screening.movie = movie;
              list.Add(screening);            
            }
          }
        }
      }      
      return list;
    }
// ---------------------------------------------------------------------------    
    /**
     * Returns a list with Director objects.
     * @param film_id the id of a movie
     */
    public static List<Director> GetDirectors(int film_id) {
      List<Director> list = new List<Director>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.DIRECTORS;
          cmd.Parameters.Add(cmd.CreateParameter());
          cmd.Parameters[0].ParameterName = "@film_id";
          cmd.Parameters[0].Value = film_id;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              list.Add(GetDirector(r));
            }
          }
        }
      }
      return list;
    }
// ---------------------------------------------------------------------------        
    /**
     * Returns an List containing all the filmfestival days.
     * @return a list containing days.
     */
    public static List<string> GetDays() {
      List<string> list = new List<string>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.DAYS;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              list.Add(r.GetString(0));
            }
          }
        }
      }        
      return list;
    }
// ---------------------------------------------------------------------------    
//    /**
//     * Returns an List containing all the screening locations.
//     * @return a list containing location codes.
//     */
    public static List<string> GetLocations() {
      List<string> list = new List<string>();
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = AdoDB.LOCATIONS;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              list.Add(r.GetString(0));
            }
          }
        }
      }        
      return list;
    }
// =========================================================================== 
  }
}