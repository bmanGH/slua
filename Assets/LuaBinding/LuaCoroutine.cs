using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;
using SLua;


public class LuaCoroutineEnumerator : IEnumerator, IDisposable {

	public LuaThread luaThread {
		get;
		protected set;
	}

	public LuaCoroutineEnumerator (LuaThread luaThread) {
		this.luaThread = luaThread;
	}

	public void Reset () {
	}

	public bool MoveNext () {
		var status = luaThread.Resume ();
		return status == LuaThreadStatus.LUA_YIELD;
	}

	public object Current {
		get
		{
			var result = luaThread.GetYieldResult ();
			if (result is object[]) {
				var o = (object[])result;
				return o [0];
			} else {
				return result;
			}
		}
	}

	public void Dispose ()
	{
		luaThread.Dispose (true);
		luaThread = null;
	}

}
