function getXPath(node, path) {
	path = path || [];
	if(node.parentNode) {
		path = getXPath(node.parentNode, path);
	}

	if(node.previousSibling) {
		var count = 1;
		var sibling = node.previousSibling
		do {
		if(sibling.nodeType == 1 && sibling.nodeName == node.nodeName) {count++;}
		sibling = sibling.previousSibling;
		} while(sibling);
		if(count == 1) {count = null;}
	} else if(node.nextSibling) {
		var sibling = node.nextSibling;
		do {
		if(sibling.nodeType == 1 && sibling.nodeName == node.nodeName) {
			var count = 1;
			sibling = null;
		} else {
			var count = null;
			sibling = sibling.previousSibling;
		}
		} while(sibling);
	}

	if(node.nodeType == 1) {
		path.push(node.nodeName.toLowerCase() + (node.id ? "[@id='"+node.id+"']" : count > 0 ? "["+count+"]" : ''));
	}
	return path;
}

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

	// Check style for invisibility
	if ( _element.getComputedStyle !== undefined ) {
		var style = window.getComputedStyle( _element );
		if ( style.display == "none" || style.visibility == "hidden" ) {
console.log( "Element " + _element.id + " removed because display style is either display:none or visibility:hidden..." );
			return true;
		}
	}

	if ( _element.getBoundingClientRect === undefined )
		return false;

	var	rectangle = _element.getBoundingClientRect();
//	var	r = window.scrollX + rectangle.right;
//	var	b = window.scrollY + rectangle.bottom;
	var	r = rectangle.right;
	var	b = rectangle.bottom;
console.log( "Rectangle = (" + rectangle.left + ", " + rectangle.top + ", " + rectangle.width + ", " + rectangle.height + ") with scroll offset, bottom = " + (rectangle.bottom + window.scrollY) );


	if ( r <= 0 || b <= 0 ) {
console.log( "Outside top-left screen..." );
		return true;	// Outside of screen
	}

//	var	l = window.scrollX + rectangle.left;
//	var	t = window.scrollY + rectangle.top;
	var	l = rectangle.left;
	if ( l >= window.width ) {
console.log( "Outside right screen..." );
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
	return nodeText.length > 0;
}

// Returns the top parent node that contains only this child node (meaning we stop going up to the parent if the parent has more than one child)
function GetParentWithSingleChild( _element ) {
	var parent = _element.parentNode;
	if ( parent == null || parent.children.length > 1 )
		return _element;	// Stop at this element...

	return GetParentWithSingleChild( parent );
}

// Returns the FIRST top-most element that contains ALL elements that have content (i.e. image or text)
function RecurseRetrieveTopMostContentContainer( _node, _path ) {
console.log( "Examining node " + _path + " (ID = " + _node.id + " - Type = " + _node.nodeType + " - Tag = " + _node.tagName + " - Value = " + _node.nodeValue + " - XPath = " + getXPath( _node ) + ")" );
if ( _node.outerHTML !== undefined )
	console.log( "Outer HTML = " + (_node.outerHTML.length < 100 ? _node.outerHTML : _node.outerHTML.substr( 0, 100 )) );

	if ( !IsValidNode( _node ) ) {
console.log( "Node " + _path + " is invalid" );
		return null;	// The node is invalid... (e.g placeholder or invisible)
	}
	if ( IsInvisibleElement( _node ) ) {
console.log( "Node " + _path + " is invisible" );
		return null;
	}
	if ( IsContentNode( _node ) ) {
console.log( "Node " + _path + " is content" );
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

(function () {
	// Remove fixed and invisible DOM elements
	RemoveFixedNodes( document.body );
//	RemoveInvisibleNodes( document.body );

	// Enumerate all non empty nodes that contain either text or an image
	var	topMostContainer = RecurseRetrieveTopMostContentContainer( document.body );
return topMostContainer;
	return topMostContainer != null ? JSON.stringify( topMostContainer.getBoundingClientRect() ) : null;

})();
