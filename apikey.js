var db = require("./db");

var apiCache = {};

function failAuth(res) {
	res.set("WWW-Authenticate", "ApiKey");
	res.sendStatus(401);
}

function parseApiKey(req, res, next) {
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
					if(err) {
						next(err);
						return;
					}
					cn.query("SELECT * FROM ApiKey WHERE `Key` = ?", [auth], function(err, rows) {
						db.returnConnection(cn);
						if(err) {
							next(err);
							return;
						}
						if(rows.length) {
							//console.log(rows[0]);
							var rev = new Date();
							rev.setMinutes(rev.getMinutes() + 15);
							
							apiCache[auth] = {
								revalidate: rev,
								serverId: rows[0].ServerId
							};
							
							req.serverId = rows[0].ServerId;
						}
						next();
					});
				});
				return;
			}
		}
	}
	next();
}

parseApiKey.demand = function(req, res, next) {
	if(!req.serverId) {
		failAuth(res);
	} else {
		next();
	}
};

module.exports = parseApiKey;
