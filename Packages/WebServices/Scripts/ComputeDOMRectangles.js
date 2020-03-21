// Builds the absolute rectangles of content elements
function RecurseComputeContentRectangles( _node, _elementsDictionary ) {
//console.log( "Examining node " + _node.path + " (ID = " + _node.id + " - Type = " + _node.nodeType + " - Tag = " + _node.tagName + " - XPath = " + getXPath( _node ) + ")" );

	if ( !IsValidNode( _node ) ) {
//console.log( "Node " + _node.path + " is invalid!" );
		return;	// The node is invalid... (e.g placeholder or invisible)
	}

	var	contentType = IsContentNode( _node );
	if ( contentType ) {
		if ( !IsVisibleElement( _node ) ) {
			return;
		}

//console.log( "Node " + _node.path + " is content" );

		// Before leaving, compute absolute rectangle position of the content element
		var	contentNode = _node.getBoundingClientRect === undefined ? _node.parentNode	// Text nodes don't have a rectangle, only the parent has, so use parent as content node...
																	: _node;
		if ( contentNode.absoluteRectangle !== undefined )
			return;	// Already computed earlier!

		var	clientRectangle = contentNode.getBoundingClientRect();

		// Convert to absolute position & store as a custom object in the element
		contentNode.absoluteRectangle = {
			left : clientRectangle.left + window.scrollX,
			top : clientRectangle.top + window.scrollY,
			width : clientRectangle.width,
			height : clientRectangle.height
		};
	}

	// Check children
	var	childNodes = _node.childNodes;
	for ( var i = 0; i < childNodes.length; i++ ) {
		RecurseComputeContentRectangles( childNodes[i], _elementsDictionary );
	}
}

(function () {
	// Compute absolute rectangles of all content elements
	RecurseComputeContentRectangles( document.body );

})();
