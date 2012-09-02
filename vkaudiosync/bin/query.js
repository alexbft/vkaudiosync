/// <reference path="underscore-min.js" />
/// <reference path="jquery-1.8.1.min.js" />
/// <reference path="deferred.js" />
/// <reference path="external.js" />

(function (context) {
	var apiId = context.apiId = '3004830';
	var userId = null;
	var accessToken = null;

	var ajaxGet = ext.wrapCallback('ajaxGet');
	var auth = ext.wrapCallback('auth');

	var _query = function (method, args) {
		var d = new Deferred();
		args || (args = {});
		args['access_token'] = accessToken;
		ajaxGet('https://api.vk.com/method/' + method + '?' + $.param(args), function (err, res) {
			try {
				if (err) d.reject(new Error(err));
				else d.resolve(JSON.parse(res));
			} catch (e) {
				d.reject(e);
			}
		});
		return d.promise;
	}

	context.query = function (method, args) {
		function logerror(err) {
			console.log('Error in ' + method + ': ' + err);
		}

		return _query(method, args).then(function (res) {
			if (res.error) {
				logerror(res.error.error_msg);
				throw new Error(res.error.error_msg);
			} else {
				return res.response;
			}
		}, function (err) {
			logerror(err.message);
		})
	}

	context.auth = function () {
		auth('http://oauth.vk.com/authorize?' + $.param({
			client_id: apiId,
			scope: 8,
			redirect_uri: 'http://oauth.vk.com/blank.html',
			display: 'popup',
			response_type: 'token'
		}), function (err, res) {
			if (err) {
				console.log('Error: ' + err);
			}
			else {
				var parts = res.split(':');
				userId = parts[0];
				accessToken = parts[1];
				console.log('Authentication success, user id = ' + userId);
			}
		});
	}

})(this);