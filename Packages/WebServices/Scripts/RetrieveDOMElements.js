// Populates a dictionary of content elements
function RecurseRetrieveContent( _node, _elementsDictionary ) {
//console.log( "Examining node " + _node.path + " (ID = " + _node.id + " - Type = " + _node.nodeType + " - Tag = " + _node.tagName + " - Value = " + _node.nodeValue + " - XPath = " + getXPath( _node ) + ")" );
//if ( _node.outerHTML !== undefined )
//	console.log( "Outer HTML = " + (_node.outerHTML.length < 100 ? _node.outerHTML : _node.outerHTML.substr( 0, 100 )) );

	if ( !IsValidNode( _node ) ) {
		return;	// The node is invalid... (e.g placeholder or invisible)
	}
	var	contentType = IsContentNode( _node );
	if ( contentType ) {
console.log( "Node " + _node.path + " is content" );
		if ( IsInvisibleElement( _node ) ) {
			return;
		}

		// Append a new content element
		var	contentNode = _node.getBoundingClientRect === undefined ? _node.parentNode	// Text nodes don't have a rectangle, only the parent has so use parent as content node...
																	: _node;

		var	clientRectangle = contentNode.getBoundingClientRect();
			clientRectangle.top += window.scrollY;

		_elementsDictionary[contentNode] = {
			path : contentNode.path,
			type : contentType,
			x : clientRectangle.left,
			y : clientRectangle.top,
			w : clientRectangle.width,
			h : clientRectangle.height,
		};
	}

	// Check children
	var	childNodes = _node.childNodes;
	for ( var i = 0; i < childNodes.length; i++ ) {
		var	childNode = childNodes[i];
			childNode.path = (_node.path !== undefined ? _node.path + "." : "") + i.toString();

		RecurseRetrieveContent( childNode, _elementsDictionary );
	}
}

(function () {

	var	contentElements = {};
	RecurseRetrieveContent( document.body, contentElements );

	return JSON.stringify( contentElements );

})();
