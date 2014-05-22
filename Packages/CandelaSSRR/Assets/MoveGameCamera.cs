using UnityEngine;
using System.Collections;

/// <summary>
/// This is a NASTY HACK to be able to rotate the camera in the GAME tab even when not in PLAY mode.
/// Usage: Drop it on your camera, press ALT with mouse on screen to rotate the camera
/// </summary>
[ExecuteInEditMode]
public class MoveGameCamera : MonoBehaviour
{
	void	OnPreCull()
	{
		if ( enabled )
			Move();
	}

	void	OnEnable()
	{

	}

	protected void	Move()
	{
		if ( Event.current.modifiers != EventModifiers.Alt )
			return;

		Vector2	LastPosition = new Vector2( Screen.width / 2, Screen.height / 2 );
		Vector2	CurrentPosition = Event.current.mousePosition;
		transform.Rotate( Vector3.up, 0.01f * (CurrentPosition.x - LastPosition.x), Space.World );
		transform.Rotate( transform.right, 0.01f * (CurrentPosition.y - LastPosition.y), Space.World );
	}
}
