(function (context) {
	var slice = [].slice;
	var uid = 0;
	var callbacks = {};

	context.ext = {
		wrapCallback: function (extFuncName) {
			return function () {
				var args = slice.call(arguments);
				if (args.length < 1) throw new Error('No callback provided');
				var cb = args.pop();
				if (typeof cb !== 'function') throw new Error('No callback provided');
				var cbId = ++uid;
				args.push(cbId);
				callbacks[cbId] = function (err, res) {
					delete callbacks[cbId];
					cb(err, res);
				}
				var argNames = [];
				for (var i = 0, l = args.length; i < l; ++i) {
					argNames.push("args[" + i + "]");
				}
				var doCall = new Function("args", "return external." + extFuncName + "(" + argNames.join(', ') + ");");
				return doCall(args);
			}
		},

		json: function (extFuncName, arg) {
			return JSON.parse(external[extFuncName](JSON.stringify(arg)));
		}
	}

	context.$resolveCallback = function (cbId, err, res) {
		if (cbId in callbacks) {
			callbacks[cbId](err, res);
		}
	}
})(this);