// Returns the FIRST top-most element that contains ALL elements that have content (i.e. image or text)
function RecurseRetrieveTopMostContentContainer( _node, _path ) {
//console.log( "Examining node " + _path + " (ID = " + _node.id + " - Type = " + _node.nodeType + " - Tag = " + _node.tagName + " - Value = " + _node.nodeValue + " - XPath = " + getXPath( _node ) + ")" );
//if ( _node.outerHTML !== undefined )
//	console.log( "Outer HTML = " + (_node.outerHTML.length < 100 ? _node.outerHTML : _node.outerHTML.substr( 0, 100 )) );

	if ( !IsValidNode( _node ) ) {
//console.log( "Node " + _path + " is invalid" );
		return null;	// The node is invalid... (e.g placeholder or invisible)
	}
	if ( IsContentNode( _node ) ) {
//console.log( "Node " + _path + " is content" );
		if ( !IsVisibleElement( _node ) ) {
//console.log( "Node " + _path + " is invisible" );
			return null;
		}

		return _node;	// A content-containing node is its own container...
	}

	// Check children
	var	topContainer = null;
	var	childNodes = _node.childNodes;
	for ( var i = 0; i < childNodes.length; i++ ) {
		var	childPath = (_path !== undefined ? _path + "." : "") + i.toString();
		var	childContainer = RecurseRetrieveTopMostContentContainer( childNodes[i], childPath );
		if ( childContainer == null )
			continue;	// Child doesn't have any content...

		if ( topContainer == null ) {
			topContainer = childContainer;	// First child with content...
		} else if ( childContainer != topContainer ) {
			// Found another child with content... This means WE are the container for these 2 children...
			topContainer = _node;
			break;
		}
	}

	return topContainer;
}

function RemoveFixedNodes( _root ) {
	if ( _root == null )
		_root = document.body;

	RecurseGetSpecificNodes( _root, IsFixedElement ).forEach( _element => _element.remove() );
}

//function IsInvisibleElement( _element ) {
//	return !IsVisibleElement( _element );
//}
//function RemoveInvisibleNodes(_root) {
//	if ( _root == null )
//		_root = document.body;
//
//	RecurseGetSpecificNodes( _root, IsInvisibleElement ).forEach( _element => _element.remove() );
//}

(function() {
	// Remove fixed and invisible DOM elements
	RemoveFixedNodes( document.body );
//	RemoveInvisibleNodes( document.body );

	// Enumerate all non empty nodes that contain either text or an image
	var	topMostContainer = RecurseRetrieveTopMostContentContainer( document.body );
	if ( topMostContainer == null )
		return null;

	var	containerRect = topMostContainer.getBoundingClientRect();
	if ( topMostContainer == document.body ) {
		// Patch rectangle for body element
		containerRect.height = GetPageHeight();
		containerRect.bottom = containerRect.top + containerRect.height;
	}
//return topMostContainer;
	return JSON.stringify( containerRect );

})();
