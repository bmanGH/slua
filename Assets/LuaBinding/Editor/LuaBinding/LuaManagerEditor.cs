using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.IO;
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
					if (System.Object.ReferenceEquals(o, null) == false) {
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

				if (GUILayout.Button ("Dump Cache Objects Info")) {
					dumpCacheObjectsInfo (objectCache);
				}

#if LUA_OBJECT_CACHE_TRACE
				if (GUILayout.Button ("Dump Cache Trace")) {
					dumpCacheTrace (objectCache);
				}
#endif
			}
		}
	}

	protected void dumpCacheObjectsInfo (ObjectCache objectCache) {
		StringBuilder sb = new StringBuilder ();

		var objectList = objectCache.cache;
		foreach (var item in objectList) {
			object o = item.v;
			if (System.Object.ReferenceEquals(o, null) == false) {
				if (objectCache.isGcObject (o) == true) {
					if (typeof(UnityEngine.Object).IsAssignableFrom(o.GetType())) {
						var unityObj = (UnityEngine.Object)o;
						if (unityObj != null)
							sb.AppendLine ("[U] " + unityObj.GetType().Name + " : " + unityObj.GetInstanceID());
						else
							sb.AppendLine ("[U] " + unityObj.GetType().Name + " : " + unityObj.GetInstanceID() + " [X]");
					} else {
						sb.AppendLine ("[O] " + o.GetType().Name);
					}
				} else { // value type
					sb.AppendLine ("[V] " + o.GetType().Name);
				}
			}
		}

		string path = Path.Combine (Environment.CurrentDirectory, "lua_cache_objects_info.txt");
		using (StreamWriter fs = new StreamWriter(path, false, new System.Text.UTF8Encoding(false))) {
			fs.Write (sb.ToString ());
		}
	}

#if LUA_OBJECT_CACHE_TRACE
	protected void dumpCacheTrace (ObjectCache objectCache) {
		var cacheTrace = objectCache.cacheTrace;
		var cacheTypeName = objectCache.cacheTypeName;
		StringBuilder sb = new StringBuilder ();

		foreach (var traceItem in cacheTrace) {
			sb.AppendFormat ("[{0}] : {1}\n", cacheTypeName[traceItem.Key], traceItem.Key);
			sb.AppendLine (traceItem.Value);
			sb.AppendLine ();
		}

		string path = Path.Combine (Environment.CurrentDirectory, "lua_cache_trace.txt");
		using (StreamWriter fs = new StreamWriter(path, false, new System.Text.UTF8Encoding(false))) {
			fs.Write (sb.ToString ());
		}
	}
#endif

}
