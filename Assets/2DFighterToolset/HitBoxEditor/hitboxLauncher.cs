using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;

public class hitboxLauncher : EditorWindow {

	[MenuItem("Window/HitboxEditor")]

	public static void ShowWindow()
	{
		Process.Start (Environment.CurrentDirectory + @"\Assets\2DFighterToolset\HitBoxEditor\hitboxEditor.exe");
		//UnityEngine.Debug.Log(Environment.CurrentDirectory);
	}

}
