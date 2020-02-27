// This function is used to know if an element is set with a 'fixed' position, which is what we're looking for: fixed elements that may block the viewport
function IsFixedElement( _element ) {
	var position = window.getComputedStyle( _element ).position;
	return position == 'sticky' || position == 'fixed';
}

// Returns true if the element is not visible (e.g. simply hidden or out of screen)
function IsInvisibleElement( _element ) {
	if ( _element.hidden ) {
console.log( "Element " + _element.id + " removed because hidden..." );
		return true;	// Obvious...
	}

	var	rectangle = _element.getBoundingClientRect();
	var	r = window.scrollX + rectangle.right;
	var	b = window.scrollY + rectangle.bottom;
	if ( r <= 0 || b <= 0 ) {
console.log( "Element " + _element.id + " son of " + _element.parentNode.id + " removed because outside top-left screen..." );
		return true;	// Outside of screen
	}

	var	l = window.scrollX + rectangle.left;
//	var	t = window.scrollY + rectangle.top;
	if ( l >= window.width ) {
console.log( "Element " + _element.id + " removed because outside right screen..." );
		return true;	// Outside of screen
	}

	return false;	// Element is visible!
}

// Returns true if the element is valid
function IsValidNode( _node ) {
	if ( _node == null )
		return false;
	if ( _node.getBoundingClientRect === undefined )
		return true;	// Text nodes don't have this function...

	// Check for size => super small elements are usually placeholders
	var	rect = _node.getBoundingClientRect();
//console.log( "Examining node " + _node + " (ID = " + _node.id + ") => (" + rect.width + ", " + rect.height + ")" );
	return rect.width > 4 && rect.height > 4;
}

// Returns true if the node is a content node
function IsContentNode( _node ) {
	if ( _node == null )
		return false;

	if ( _node.nodeType != 3 ) {
		// Detect obvious nodes
		switch ( _node.tagName ) {
			case "A":
			case "LINK":
			case "IMG":
				return true;
		}

		return false;	// Not a content node...
	}

	var	nodeText = _node.nodeValue;
	if ( nodeText == null )
		return false;	// Curiously, a text node with no text...

	// Trim text of all whitespaces to make sure the text is significant and not a placeholder...
	nodeText = nodeText.trim();
	return nodeText.length > 4;
}

// Returns the top parent node that contains only this child node (meaning we stop going up to the parent if the parent has more than one child)
function GetParentWithSingleChild( _element ) {
	var parent = _element.parentNode;
	if ( parent == null || parent.children.length > 1 )
		return _element;	// Stop at this element...

	return GetParentWithSingleChild( parent );
}

// Recursively populates a list with the top-most unique parent of elements that pass the specified filter
function RecurseGetSpecificNodes( _element, _filter ) {
	// Check if the element itself passes the filter
	if ( _filter( _element ) )
		return [ GetParentWithSingleChild( _element ) ];

	// Otherwise, investigate each child...
	var childNodes = _element.children;
	var result = [];
	for ( var i = 0; i < childNodes.length; i++ ) {
		result = result.concat( RecurseGetSpecificNodes( childNodes[i], _filter ) );
	}

	return result;
}

function RemoveFixedNodes( _root ) {
	if ( _root == null )
		_root = document.body;

	RecurseGetSpecificNodes( _root, IsFixedElement ).forEach( _element => _element.remove() );
}

//function RemoveInvisibleNodes(_root) {
//	if ( _root == null )
//		_root = document.body;
//
//	RecurseGetSpecificNodes( _root, IsInvisibleElement ).forEach( _element => _element.remove() );
//}
