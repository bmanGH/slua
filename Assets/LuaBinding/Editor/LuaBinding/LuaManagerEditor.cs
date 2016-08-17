using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using SLua;
using LuaInterface;


[CustomEditor(typeof(LuaManager))]
public class LuaManagerEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		if (Application.isPlaying == true) {
			LuaManager manager = (LuaManager)target;

			if (manager.isReady == true) {
				IntPtr L = manager.luaState.L;

				var objectCache = ObjectCache.get (L);
				var objectList = objectCache.cache;
				int classObjCount = 0;
				int valueObjCount = 0;
				foreach (var item in objectList) {
					object o = item.v;
					if (o != null) {
						if (objectCache.isGcObject (o) == true) {
							classObjCount++;
						} else {
							valueObjCount++;
						}
					}
				}
				EditorGUILayout.LabelField ("Cache class object:", classObjCount.ToString());
				EditorGUILayout.LabelField ("Cache value object:", valueObjCount.ToString());

				EditorGUILayout.LabelField ("Memory(Kb)", manager.getMemoryUsed().ToString());

				if (GUILayout.Button ("GC Collect")) {
					manager.gcCollect ();
				}
			}
		}
	}

}
