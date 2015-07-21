using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ServerStatus.Models
{
	public class StockItem
	{
		[JsonIgnore]
		public int ServerId { get; set; }
		public string mod { get; set; }
		public string id { get; set; }
		public int meta { get; set; }
		public int qty { get; set; }
	}
}