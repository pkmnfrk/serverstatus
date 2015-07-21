using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data.MySqlClient;

namespace ServerStatus.Models
{
	public static class Sql
	{
		public static IDbConnection GetConnection()
		{
			
			var conn = new MySqlConnection();
			conn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			conn.Open();

			return conn;

		}
	}
}