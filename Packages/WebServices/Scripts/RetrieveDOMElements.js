// Populates a dictionary of content elements
function RecurseRetrieveContent( _node, _elementsDictionary ) {
//console.log( "Examining node " + _node.path + " (ID = " + _node.id + " - Type = " + _node.nodeType + " - Tag = " + _node.tagName + " - XPath = " + getXPath( _node ) + ")" );

	if ( !IsValidNode( _node ) ) {
//console.log( "Node " + _node.path + " is invalid!" );
		return;	// The node is invalid... (e.g placeholder or invisible)
	}

	var	contentType = IsContentNode( _node );
	if ( contentType ) {
//console.log( "Node " + _node.path + " is content" );

		// Append a new content element
		var	contentNode = _node.getBoundingClientRect === undefined ? _node.parentNode	// Text nodes don't have a rectangle, only the parent has so use parent as content node...
																	: _node;

		if ( _elementsDictionary[contentNode.path] === undefined ) {
			var	clientRectangle = contentNode.getBoundingClientRect();

			// Convert to absolute position
			clientRectangle.left += window.scrollX;
			clientRectangle.top += window.scrollY;

//console.log( "Node " + contentNode.path + " - Type " + contentType + " (" + clientRectangle.left + ", " + clientRectangle.top + ", " + clientRectangle.width + ", " + clientRectangle.height + ")"
//		 + " Tag = " + contentNode.tagName + " HTML = " + contentNode.outerHTML );

			var	contentElementDescriptor = {
				path : contentNode.path,
				type : contentType,
				x : clientRectangle.left,
				y : clientRectangle.top,
				w : clientRectangle.width,
				h : clientRectangle.height,
			};

			if ( contentType == 1 ) {
				contentElementDescriptor.URL = contentNode.href;	// Keep URL for links
			}

			_elementsDictionary[contentNode.path] = contentElementDescriptor;
		}
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

	// Return only the values
	var	contentElementsArray = [];
	for ( var key in contentElements ) {
		contentElementsArray.push( contentElements[key] );
	}

	return JSON.stringify( contentElementsArray );

})();
