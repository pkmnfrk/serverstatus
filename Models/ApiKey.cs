using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Dapper;
using System.Threading.Tasks;

namespace ServerStatus.Models
{
	public class ApiKey
	{
		public int ApiKeyId { get; set; }
		public int ServerId { get; set; }
		public string Key { get; set; }

		public Server Server { get; set; }

		public static async Task<ApiKey> FetchKeyRecord(string key)
		{
			using (var cn = Sql.GetConnection())
			{
				return (await cn.QueryAsync<ApiKey, Server, ApiKey>(@"
SELECT a.*, s.*
FROM ApiKey a
JOIN Server s ON a.ServerId = s.ServerId
WHERE a.`Key` = @key", (a, s) => { a.Server = s; return a; }, new { key }, splitOn: "ServerId")).FirstOrDefault();
			}
		}
	}
}