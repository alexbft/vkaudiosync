(function (context) {

    var _ = context._ || require('underscore');

    var Deferred = function () {
        if (!_.isEmpty(this)) {
            return new Deferred();
        } else {
            this._success_handlers = [];
            this._error_handlers = [];
            this.promise = new Promise(this);
        }
    }
    Deferred.prototype.constructor = Deferred;
    var DeferredPrivate = {};

    Deferred.isPromise = function (obj) {
        return Boolean(obj && obj.constructor === Promise);
    }

    Deferred.all = function (promiseArray) {
        var d = new Deferred(), reject = _.bind(d.reject, d);
        var num = 0, l = promiseArray.length;
        var res = [];
        _.each(promiseArray, function (promise, index) {
            promise.then(function (v) {
                res[index] = v;
                num += 1;
                if (num >= l) {
                    d.resolve(res);
                }
            }, reject);
        });
        return d.promise;
    }

    Deferred.prototype.resolve = function (value) {
        var self = this;
        if (this._success_handlers.length) {
            if (Deferred.isPromise(value)) {
                value.then(_.bind(self.resolve, self), _.bind(self.reject, self));
            } else {
                _.each(this._success_handlers, function (handler) {
                    handler(value);
                });
            }
        } else {
            this._result_type = 'success';
            this._result = value;
        }
        return this.promise;
    }

    Deferred.prototype.reject = function (err) {
        var self = this;
        if (this._error_handlers.length) {
            _.each(this._error_handlers, function (handler) {
                handler(err);
            });
        } else {
            this._result_type = 'error';
            this._result = err;
        }
        return this.promise;
    }

    Deferred.prototype.result = function () {
        return this._result;
    }

    Deferred.prototype.resultType = function () {
        return this._result_type || false;
    }

    DeferredPrivate.then = function (callback, errback) {
        var after = new Deferred();

        function makeHandler(handler) {
            return function (value) {
                try {
                    after.resolve(handler(value));
                } catch (err) {
                    after.reject(err);
                }
            }
        }

        if (callback) {
            this._success_handlers.push(makeHandler(callback));
        }
        if (errback) {
            this._error_handlers.push(makeHandler(errback));
        } else {
            this._error_handlers.push(function (err) {
                after.reject(err);
            });
        }
        if (this._result_type) {
            if (this._result_type === 'success') {
                this.resolve(this._result);
            } else {
                this.reject(this._result);
            }
        }
        return after.promise;
    }

    DeferredPrivate.end = function () {
        this._error_handlers.push(function (err) { throw err; });
    }

    var Promise = function (deferred) {
        this.then = _.bind(DeferredPrivate.then, deferred);
        this.end = _.bind(DeferredPrivate.end, deferred);
    }
    Promise.prototype.constructor = Promise;

    Promise.prototype.then = function () {
        throw new Error('Should be overridden!');
    }

    Promise.prototype.error = function (errback) {
        return this.then(null, errback);
    }

    Promise.prototype.end = function () {
        throw new Error('Should be overridden!');
    }

    if (typeof module !== "undefined") {
        module.exports = Deferred;
    } else {
        context.Deferred = Deferred;
    }

})(this);