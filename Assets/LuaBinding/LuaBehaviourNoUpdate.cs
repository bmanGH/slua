using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;
using SLua;


public class LuaBehaviourNoUpdate : MonoBehaviour {

	[DoNotToLua]
	public TextAsset scriptAsset;

	public LuaTable bindLuaTable {
		get;
		protected set;
	}

	protected LuaFunction _luaAwakeFunc;
	protected LuaFunction _luaOnEnableFunc;
	protected LuaFunction _luaOnDisableFunc;
	protected LuaFunction _luaOnDestroyFunc;

	protected virtual void bind (IntPtr L) {
		_luaAwakeFunc = bindLuaTable ["Awake"] as LuaFunction;
		_luaOnEnableFunc = bindLuaTable ["OnEnable"] as LuaFunction;
		_luaOnDisableFunc = bindLuaTable ["OnDisable"] as LuaFunction;
		_luaOnDestroyFunc = bindLuaTable ["OnDestroy"] as LuaFunction;

//		bindLuaTable.push (L); // 1
//		LuaDLL.lua_newtable(L); // 2
//		LuaObject.pushVar (L, this); // 3
//		LuaDLL.lua_setfield(L, -2, "__index"); // 2
//		LuaObject.pushVar (L, this); // 3
//		LuaDLL.lua_setfield(L, -2, "__newindex"); // 2
//		LuaDLL.lua_setmetatable(L, -2); // 1
//		LuaDLL.lua_pop(L, 1); // 0

		bindLuaTable ["__bindBehaviour"] = this;
	}

	protected virtual void Awake () {
		if (LuaManager.getInstance ().isReady == true && scriptAsset != null) {
			LuaState luaState = LuaManager.getInstance ().luaState;

			bindLuaTable = luaState.doString (scriptAsset.text) as LuaTable;
			if (bindLuaTable == null) {
				throw new Exception ("<LuaBehaviour> no bind lua table");
			}

			bind (luaState.L);

			if (_luaAwakeFunc != null)
				_luaAwakeFunc.call (bindLuaTable);
		}
	}

	protected virtual void OnEnable () {
		if (_luaOnEnableFunc != null)
			_luaOnEnableFunc.call (bindLuaTable);
	}

	protected virtual void OnDisable () {
		if (_luaOnDisableFunc != null)
			_luaOnDisableFunc.call (bindLuaTable);
	}

	protected virtual void OnDestroy () {
		if (_luaOnDestroyFunc != null)
			_luaOnDestroyFunc.call (bindLuaTable);

		if (_luaAwakeFunc != null)
			_luaAwakeFunc.Dispose ();
		if (_luaOnEnableFunc != null)
			_luaOnEnableFunc.Dispose ();
		if (_luaOnDisableFunc != null)
			_luaOnDisableFunc.Dispose ();
		if (_luaOnDestroyFunc != null)
			_luaOnDestroyFunc.Dispose ();
		if (bindLuaTable != null)
			bindLuaTable.Dispose ();
	}

}
