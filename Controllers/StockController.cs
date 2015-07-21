using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Dapper;

using ServerStatus.Models;
using ServerStatus.Filters;

namespace ServerStatus.Controllers
{
	[RoutePrefix("stock")]
	[Authorize]
	[ApiKeyAuthentication]
	public class StockController : ApiController
	{
		private ApiKey ApiKey
		{
			get
			{
				return (RequestContext.Principal as ApiKeyPrincipal).ApiKey;
			}
		}

		// GET api/<controller>
		[Route("")]
		public IEnumerable<StockItem> Get()
		{
			using (var cn = Sql.GetConnection())
			{
				return cn.Query<StockItem>(@"
SELECT `Mod`, Id, Meta, Qty
FROM StockItem
WHERE ServerId = @ServerId
ORDER BY `Mod`, Id, Meta", new { ApiKey.ServerId } ).ToList();
			}
		}

		// GET api/<controller>/minecraft
		[Route("{mod}")]
		public IEnumerable<StockItem> Get(string mod)
		{
			using (var cn = Sql.GetConnection())
			{
				return cn.Query<StockItem>(@"
SELECT `Mod`, Id, Meta, Qty
FROM StockItem
WHERE ServerId = @ServerId AND `Mod` = @mod
ORDER BY `Mod`, Id, Meta", new { ApiKey.ServerId, mod }).ToList();
			}
		}

		// GET api/<controller>/minecraft/wool
		[Route("{mod}/{id}", Name = "ModId")]
		public IEnumerable<StockItem> Get(string mod, string id)
		{
			using (var cn = Sql.GetConnection())
			{
				return cn.Query<StockItem>(@"
SELECT `Mod`, Id, Meta, Qty
FROM StockItem
WHERE ServerId = @ServerId AND `Mod` = @mod AND Id = @id
ORDER BY `Mod`, Id, Meta", new { ApiKey.ServerId, mod, id }).ToList();
			}
		}

		// GET api/<controller>/minecraft/wool/5
		[Route("{mod}/{id}/{meta}", Name = "ModIdMeta")]
		public IEnumerable<StockItem> Get(string mod, string id, int meta)
		{
			using (var cn = Sql.GetConnection())
			{
				return cn.Query<StockItem>(@"
SELECT `Mod`, Id, Meta, Qty
FROM StockItem
WHERE ServerId = @ServerId AND `Mod` = @mod AND Id = @id AND Meta = @meta
ORDER BY `Mod`, Id, Meta", new { ApiKey.ServerId, mod, id, meta }).ToList();
			}
		}

		// PUT api/<controller>
		[Route("{mod}/{id}")]
		public IHttpActionResult Post([FromBody]StockItem value, string mod, string id)
		{
			return Post(value, mod, id, 0);
		}

		// POST api/<controller>
		[Route("{mod}/{id}/{meta}")]
		public IHttpActionResult Post([FromBody]StockItem value, string mod, string id, int meta)
		{
			using (var cn = Sql.GetConnection())
			{
				cn.Execute(@"
INSERT INTO StockItem (ServerId, `Mod`, Id, Meta, Qty)
VALUES(@ServerId, @mod, @id, @meta, @qty)
ON DUPLICATE KEY UPDATE Qty = @qty", new
								   {
									   ApiKey.ServerId,
									   mod,
									   id,
									   meta,
									   value.qty
								   });

				value = cn.Query<StockItem>(@"
SELECT *
FROM StockItem
WHERE ServerId = @ServerId AND `Mod` = @mod AND Id = @id AND Meta = @meta", new { ApiKey.ServerId, mod, id, meta }).FirstOrDefault();
			}

			if (meta != 0)
			{
				return CreatedAtRoute("ModIdMeta", new { mod, id, meta }, value);
			}
			else
			{
				return CreatedAtRoute("ModId", new { mod, id }, value);
			}

		}

	}
}