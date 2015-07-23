var db = require("../db");

var express = require('express');
var router = express.Router();
var bodyParser = require('body-parser');

var apiCache = {};

function failAuth(res) {
	res.set("WWW-Authenticate", "ApiKey");
	res.sendStatus(401);
}

router.use(function(req, res, next) {
	var auth = req.get('Authorization');
	
	if(auth) {
		
		auth = /^ApiKey (.*)$/i.exec(auth);
		
		if(auth) {
			auth = auth[1];
			
			if(auth) {
				auth = auth.toLowerCase();
				
				if(apiCache[auth]) {
					var c = apiCache[auth];
					if(c.revalidate < new Date()) {
						req.serverId = c.serverId;
						next();
						return;
					}
				}
				
				db.getConnection(function(err, cn) {
					cn.query("SELECT * FROM ApiKey WHERE `Key` = ?", [auth], function(err, rows) {
						db.returnConnection(cn);
						if(rows.length) {
							//console.log(rows[0]);
							var rev = new Date();
							rev.setMinutes(rev.getMinutes() + 15);
							
							apiCache[auth] = {
								revalidate: rev,
								serverId: rows[0].ServerId
							};
							
							req.serverId = rows[0].ServerId;
							next();
						} else {
							failAuth(res);
						}
					});
				});
				
			}
			return;
			
		}
	}
	
	failAuth(res);
});

router.get('/', function(req, res, next) {
	db.getConnection(function(err, cn) {
		if(err) {
			next(err);
			return;
		}
		
		cn.query("SELECT `Mod`, Id, Meta, Qty FROM StockItem", function(err, rows) {
			if(err) {
				next(err);
				return;
			}
			
			db.returnConnection(cn);
			
			res.send(rows);
			
		});
	});
});

router.get('/:mod', function(req, res, next) {
	var mod = req.params.mod;
	
	db.getConnection(function(err, cn) {
		if(err) {
			next(err);
			return;
		}
		
		cn.query("SELECT `Mod`, Id, Meta, Qty FROM StockItem Where `Mod` = ?", [mod], function(err, rows) {
			if(err) {
				next(err);
				return;
			}
			
			db.returnConnection(cn);
			
			res.send(rows);
			
		});
	});
});

router.get('/:mod/:id', function(req, res, next) {
	var mod = req.params.mod;
	var id = req.params.id;
	
	db.getConnection(function(err, cn) {
		if(err) {
			next(err);
			return;
		}
		
		cn.query("SELECT `Mod`, Id, Meta, Qty FROM StockItem Where `Mod` = ? AND Id = ?", [mod, id], function(err, rows) {
			if(err) {
				next(err);
				return;
			}
			
			db.returnConnection(cn);
			
			res.send(rows);
			
		});
	});
});

router.get('/:mod/:id/:meta', function(req, res, next) {
	var mod = req.params.mod;
	var id = req.params.id;
	var meta = req.params.meta;
	
	db.getConnection(function(err, cn) {
		if(err) {
			next(err);
			return;
		}
		
		cn.query("SELECT `Mod`, Id, Meta, Qty FROM StockItem Where `Mod` = ? AND Id = ? And Meta = ?", [mod, id, meta], function(err, rows) {
			if(err) {
				next(err);
				return;
			}
			
			db.returnConnection(cn);
			
			res.send(rows);
			
		});
	});
});

var jsonBody = bodyParser.json();

router.post('/:mod/:id/:meta?', jsonBody ,function(req, res, next) {
	var mod = req.params.mod;
	var id = req.params.id;
	var meta = req.params.meta;
	
	if(!meta) meta = 0;
	
	ret = {
		mod: mod,
		id: id,
		meta: meta,
		qty: req.body.qty
	};
	
	db.getConnection(function(err, cn) {
		cn.query("INSERT INTO StockItem(ServerId, `Mod`, Id, Meta, Qty) VALUES(?, ?, ?, ?, ?) ON DUPLICATE KEY UPDATE Qty = VALUES(Qty)", [req.serverId, mod, id, meta, req.body.qty], function(err, rows) {
			db.returnConnection(cn);
			
			if(err) {
				next(err);
				return;
			}
			
			res.send(ret);
		});
	});
	
	
});

module.exports = router;
