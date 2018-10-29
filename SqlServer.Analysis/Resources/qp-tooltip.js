$(document).ready(function () {
	$('div.qp-node').mouseover(function () {
		var qpNode = $(this);
		var tt = qpNode.find('div.qp-tt');

		//have to show the div before position as postion wont work on hidden items
		tt.show();

		tt.position({
			my: "left+" + (qpNode.width() / 2) + " bottom",
			at: "left center",
			of: qpNode,
			collision: "flip flip"
		});
	}).mouseout(function () {
		var tt = $(this).find('div.qp-tt');
		tt.hide();
	});
});

