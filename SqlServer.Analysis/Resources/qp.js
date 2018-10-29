if (typeof (QP) == "undefined" || !QP) {
	var QP = {}
};

(function () {
	/* Draws the lines linking nodes in query plan diagram.
	root - The document element in which the diagram is contained. */
	QP.drawLines = function (root) {
		if (root === null || root === undefined) {
			// Try and find it ourselves
			root = $(".qp-root").parent();
		} else {
			// Make sure the object passed is jQuery wrapped
			root = $(root);
		}
		// ReSharper disable once FunctionsUsedBeforeDeclared
		internalDrawLines(root);
	};

	/* Internal implementaiton of drawLines. */
	function internalDrawLines(root) {
		//find the widest statement, so we can stretch objects to fit
		var width = 0;
		$('div.qp-root').children().each(function () {
			if ($(this).width() > width) {
				width = $(this).width();
			}
		});
		$('hr.qp-statement-break').width(width);

		var context = null;
		var offset = null;
		window.setTimeout(function () {
			$(".qp-tr", root).each(function () {
				var parent = $(this);
				var canvas = parent.find("canvas");
				if (canvas.length > 0) { //this should only be the first qp-tr
					context = canvas[0].getContext("2d");
					//have to set the top and left because IE is too stupid.
					canvas[0].style.top = parent[0].offsetTop + 'px';
					canvas[0].style.left = parent[0].offsetLeft + 'px';
					canvas[0].height = parent[0].clientHeight;
					canvas[0].width = parent[0].clientWidth;
					offset = canvas.offset();
				}
				var from = $("> * > .qp-node", parent);
				$("> * > .qp-tr > * > .qp-node", parent).each(function () {
					// ReSharper disable once FunctionsUsedBeforeDeclared
					drawLine(context, offset, from, $(this));
				});
			});
			context.stroke();
		}, 1);
	}

	/* Draws a line between two nodes.
	context - The canvas context with which to draw.
	offset - Canvas offset in the document.
	from - The document jQuery object from which to draw the line.
	to - The document jQuery object to which to draw the line. */
	function drawLine(context, offset, from, to) {
		var fromOffset = from.offset();
		fromOffset.top += from.outerHeight() / 2;
		fromOffset.left += from.outerWidth();

		var toOffset = to.offset();
		toOffset.top += to.outerHeight() / 2;

		var midOffsetLeft = fromOffset.left / 2 + toOffset.left / 2;

		var hasRows = true;
		var rows = to.find("label[propertyType='Estimated Complete Number of Rows']").text();
		if (rows === '' || rows === 'NaN') {
			hasRows = false;
			rows = '1';
		}
		//strip out the commas from formating as they wont parse.
		rows = parseFloat(rows.replace(/,/g, ""));

		context.beginPath();
		context.fillStyle = "black";

		//not sure these ranges make sense... need to figure out what makes sense
		if		(rows >= 1			&& rows < 6000)			{ context.lineWidth = 1; }
		else if (rows >= 6000		&& rows < 15000)		{ context.lineWidth = 2; }
		else if (rows >= 15000		&& rows < 40000)		{ context.lineWidth = 3; }
		else if (rows >= 40000		&& rows < 100000)		{ context.lineWidth = 4; }
		else if (rows >= 100000		&& rows < 245000)		{ context.lineWidth = 5; }
		else if (rows >= 245000		&& rows < 615000)		{ context.lineWidth = 6; }
		else if (rows >= 615000		&& rows < 1550000)		{ context.lineWidth = 7; }
		else if (rows >= 1550000	&& rows < 4000000)		{ context.lineWidth = 8; }
		else if (rows >= 4000000	&& rows < 10000000)		{ context.lineWidth = 9; }
		else if (rows >= 10000000	&& rows < 25000000)		{ context.lineWidth = 10; }
		else if (rows >= 25000000	&& rows < 65000000)		{ context.lineWidth = 11; }
		else if (rows >= 65000000	&& rows < 155000000)	{ context.lineWidth = 12; }
		else if (rows >= 150000000	&& rows < 400000000)	{ context.lineWidth = 13; }
		else if (rows >= 400000000	&& rows < 999999999)	{ context.lineWidth = 14; }
		else { context.lineWidth = 15; }


		context.moveTo(fromOffset.left - offset.left, fromOffset.top - offset.top);
		context.lineTo(midOffsetLeft - offset.left, fromOffset.top - offset.top);
		context.lineTo(midOffsetLeft - offset.left, toOffset.top - offset.top);
		context.lineTo(toOffset.left - offset.left, toOffset.top - offset.top);

		context.stroke();
		context.closePath();
		context.beginPath();

		if (hasRows) {
			context.fillStyle = '#FF6347';
			context.font = 'bold 7pt Arial';
			context.textAlign = 'end';

			if (rows >= 1000000000000000) {
				rows = Math.round(rows / 1000000000000000) + 'Q'; //insanity??? NO, THIS IS SPARTA! /wtf... quadrillion
			} else if (rows >= 1000000000000) {
				rows = Math.round(rows / 1000000000000) + 'T'; //hopefully NOT??? /sigh... //trillion
			} else if (rows >= 1000000000) {
				rows = Math.round(rows / 1000000000) + 'B'; //billion
			} else if (rows >= 1000000) {
				rows = Math.round(rows / 1000000) + 'M'; //million 
			} else if (rows >= 1000) {
				rows = Math.round(rows / 1000) + 'K'; //thousands
			} else {
				rows = Math.round(rows);
			}

			context.fillText(rows, toOffset.left - offset.left - 2, to.offset().top - offset.top + 9);
		}
		context.closePath();
	}
})();
