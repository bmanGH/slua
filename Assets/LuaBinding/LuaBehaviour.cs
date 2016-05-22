using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;
using SLua;


public class LuaBehaviour : LuaBehaviourNoUpdate {

	protected LuaFunction _luaUpdateFunc;

	protected override void bind (IntPtr L) {
		base.bind (L);

		_luaUpdateFunc = bindLuaTable ["Update"] as LuaFunction;
	}

	protected virtual void Update () {
		if (_luaUpdateFunc != null)
			_luaUpdateFunc.call (bindLuaTable, Time.deltaTime);
	}

	protected override void OnDestroy () {
		base.OnDestroy ();

		if (_luaUpdateFunc != null)
			_luaUpdateFunc.Dispose ();
	}

}


public static class LuaBehaviourExtension {
	
	public static Coroutine StartCoroutine (this MonoBehaviour behaviour, LuaThread luaThread, out LuaCoroutineEnumerator luaCoroutine) {
		luaCoroutine = new LuaCoroutineEnumerator (luaThread);
		return behaviour.StartCoroutine (luaCoroutine);
	}

}
