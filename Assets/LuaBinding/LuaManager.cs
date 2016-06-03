using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using LuaInterface;
using SLua;


public class LuaManager : MonoSingleton<LuaManager> {

	public static string scriptRootPath = "Scripts/Lua/";

	public LuaState luaState {
		get;
		protected set;
	}

	public bool isReady {
		get;
		protected set;
	}

	protected int errorReported = 0;

	public LuaManager () {
		isReady = false;
	}

	protected Action<IntPtr>[] getBindList(Assembly assembly, string ns) {
		Type t = assembly.GetType(ns);
		if(t != null)
			return (Action<IntPtr>[]) t.GetMethod("GetBindList").Invoke(null, null);
		return new Action<IntPtr>[0];
	}

	protected IEnumerator doBind (LuaManager manager) {
		IntPtr L = manager.luaState.L;

		LuaObject.init(L);

		yield return null;

		List<Action<IntPtr>> bindList = new List<Action<IntPtr>>();
		Assembly assembly = Assembly.Load("Assembly-CSharp");
		bindList.AddRange(getBindList(assembly, "SLua.BindUnity"));
		bindList.AddRange(getBindList(assembly, "SLua.BindUnityUI"));
		bindList.AddRange(getBindList(assembly, "SLua.BindDll"));
		bindList.AddRange(getBindList(assembly, "SLua.BindCustom"));

		yield return null;

		int n = 0;
		while (n < bindList.Count) {
			int i = n;
			for (; i < n + 20 && i < bindList.Count; i++) {
				Action<IntPtr> bindAction = bindList[i];
				bindAction(L);
			}
			n = i;

			yield return null;
		}
		Debug.Log("<LuaManager> lua state bind " + n + " items");

//		LuaTimer.reg(L);
//		LuaCoroutine.reg(L, manager);
		Helper.reg(L);
//		LuaValueType.reg(L);
		LuaDLL.luaS_openextlibs(L);

		yield return null;

		Lua3rdDLL.open(L);

		yield return null;

		if (LuaDLL.lua_gettop (luaState.L) != errorReported) {
			Debug.LogError ("<LuaManager> Some function not remove temp value from lua stack, You should fix it");
			errorReported = LuaDLL.lua_gettop (luaState.L);
		} else {
			LuaState.loaderDelegate += LoaderHandle;

			Debug.Log("<LuaManager> lua state get ready");
			isReady = true;
		}
	}

	protected byte[] LoaderHandle (string fn) {
		fn = fn.Replace(".", "/");
		TextAsset asset = Resources.Load(Path.HasExtension (fn) ? scriptRootPath + fn : scriptRootPath + fn + ".lua") as TextAsset;
		if (asset == null) {
			// fallback to ?/init.lua
			asset = Resources.Load(scriptRootPath + fn + "/init.lua") as TextAsset;
			if (asset == null)
				return null;
		} 
		return asset.bytes;
	}

	public void setup () {
		shutdown ();

		luaState = new LuaState();

		StartCoroutine (doBind (this));
	}

	public void shutdown () {
		if (luaState != null && isReady == true) {
			LuaState.loaderDelegate -= LoaderHandle;

			luaState.Dispose ();
			luaState = null;

			Debug.Log("<LuaManager> lua state shut down");
			isReady = false;
		}
	}

	protected override void OnDestroy () {
		base.OnDestroy ();

		if (Application.isEditor == false) {
			shutdown ();
		}
	}

	void LateUpdate () {
		if (isReady == false)
			return;

		if (LuaDLL.lua_gettop(luaState.L) != errorReported)
		{
			errorReported = LuaDLL.lua_gettop(luaState.L);
			Debug.LogError(string.Format("<LuaManager> Some function not remove temp value({0}) from lua stack, You should fix it", LuaDLL.luaL_typename(luaState.L, errorReported)));
		}

		luaState.checkRef();

//		LuaTimer.tick(Time.deltaTime);
	}

}
