using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(Fighter))]
public class HitboxTest : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		Fighter myFighter = (Fighter)target;
		if (GUILayout.Button ("Add Hitboxes")) 
		{
			myFighter.AddHitboxes ();
		}
			
	}


}
