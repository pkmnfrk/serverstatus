var db = require("../db");

var express = require('express');
var router = express.Router();
var bodyParser = require('body-parser');
var apiKey = require('../apikey');

router.use(apiKey);


router.get('/data/:mod?/:id?/:meta?', function(req, res, next) {
	var mod = req.params.mod;
	var id = req.params.id;
	var meta = req.params.meta;
	var serverId = req.serverId || req.query.serverid;
	
	if(!serverId) {
		next(new Error("No serverid"));
		return;
	}
	
	var query = "select `Mod`, Id, Meta, `Date`, Qty FROM StockItemSample";
	query += " WHERE ServerId = ?";
	var params = [serverId];
	
	if(mod) {
		query += " AND `Mod` = ?";
		params.push(mod);
		if(id) {
			query += " AND Id = ?";
			params.push(id);
			if(meta) {
				query += " AND Meta = ?";
				params.push(meta);
			}
		}
	}
	
	//query += " GROUP BY `Mod`, Id, Meta";
	
	db.getConnection(function(err, cn) {
		if(err) {
			next(err);
			return;
		}
		
		cn.query(query, params, function(err, rows) {
			if(err) {
				next(err);
				return;
			}
			
			res.send(rows);
		});
	});
});


router.get('/:mod?/:id?/:meta?', function(req, res, next) {
	var mod = req.params.mod;
	var id = req.params.id;
	var meta = req.params.meta;
	var serverId = req.serverId || req.query.serverid;
	
	if(!serverId) {
		next(new Error("No serverid"));
		return;
	}
	
	db.getConnection(function(err, cn) {
		if(err) {
			next(err);
			return;
		}
		
		var query = "SELECT `Mod`, Id, Meta, Qty FROM StockItem Where";
		var params = [];
		
		query += " ServerId = ?";
		params.push(serverId);
		
		if(mod) {
			query += " AND `Mod` = ?";
			params.push(mod);
			if(id) {
				query += " AND Id = ?";
				params.push(id);
				if(meta) {
					query += " AND Meta = ?";
					params.push(meta);
				}
			}
		}
		
		cn.query(query, params, function(err, rows) {
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

router.post('/:mod/:id/:meta?', apiKey.demand, jsonBody, function(req, res, next) {
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
