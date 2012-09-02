(function (context) {
	var slice = [].slice,
		logCont = null;

	if (!context.console) {
		context.console = {
			log: function () {
				var str = slice.call(arguments).join(' ');
				if (logCont === null) {
					var body = document.getElementsByTagName('body')[0];
					logCont = document.createElement('div');
					logCont.style.fontFamily = 'monospace';
					body.appendChild(logCont);
				}
				var newEntry = document.createElement('div');
				var textNode = document.createTextNode(str);
				newEntry.appendChild(textNode);
				logCont.appendChild(newEntry);
			}
		}
	}
})(this);