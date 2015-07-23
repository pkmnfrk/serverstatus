var mysql = require("mysql")

var config = require("./config");

var pool = mysql.createPool(config.mysql);

var ret = {
	getConnection: function(cb) {
		pool.getConnection(cb);
	},
	
	returnConnection: function(cn) {
		cn.release();
	}
};

module.exports = ret;