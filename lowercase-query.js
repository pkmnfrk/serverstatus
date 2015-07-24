module.exports = function(req, res, next) {
	var remap = {};
	for(var i in req.query) {
		if(i != i.toLowerCase()) {
			remap[i] = i.toLowerCase();
		}
	}
	
	for(var i in remap) {
		req.query[remap[i]] = req.query[i];
		delete req.query[i];
	}
	
	next();
}