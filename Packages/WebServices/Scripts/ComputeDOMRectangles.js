// Builds the absolute rectangles of content elements
function RecurseComputeContentRectangles( _node, _offsetX, _offsetY ) {
//console.log( "Examining node " + _node.path + " (ID = " + _node.id + " - Type = " + _node.nodeType + " - Tag = " + _node.tagName + " - XPath = " + getXPath( _node ) + ")" );

	if ( !IsValidNode( _node ) ) {
//console.log( "Node " + _node.path + " is invalid!" );
		return;	// The node is invalid... (e.g placeholder or invisible)
	}

	var	contentType = IsContentNode( _node );
	if ( contentType ) {
		// Compute absolute rectangle position of the content element
		var	contentNode = _node.getBoundingClientRect === undefined ? _node.parentNode	// Text nodes don't have a rectangle, only the parent has, so use parent as content node...
																	: _node;
		if ( contentNode.absoluteRectangle !== undefined )
			return;	// Rectangle has already been computed earlier!

		if ( !IsVisibleElement( contentNode ) ) {
			return;	// Not visible...
		}

//console.log( "Node " + _node.path + " is content" );
//if ( contentType == 3 && _node.nodeValue.trim() == "TENDANCES:" ) {
//	var	debugRect = _node.parentNode.getBoundingClientRect();
//	console.log( "POUIK!!! ==> (" + debugRect.left + ", " + debugRect.top + ", " + debugRect.width + ", " + debugRect.height + " ) (r,b) = ( " + debugRect.right + ", " + debugRect.bottom + " )" );
//}
//if ( contentType == 3 )
//	console.log( _node.nodeValue );

		var	clientRectangle = contentNode.getBoundingClientRect();

		// Convert to absolute position & store as a custom object in the element
		contentNode.absoluteRectangle = {
			left : clientRectangle.left + _offsetX,
			top : clientRectangle.top + _offsetY,
			width : clientRectangle.width,
			height : clientRectangle.height
		};
	}

	// Check children
	var	childNodes = _node.childNodes;
	for ( var i = 0; i < childNodes.length; i++ ) {
		RecurseComputeContentRectangles( childNodes[i], _offsetX, _offsetY );
	}
}

// Compute absolute rectangles of all content elements
function ComputeDOMRectangles( _offsetX, _offsetY ) {
	RecurseComputeContentRectangles( document.body, _offsetX, _offsetY );
}
