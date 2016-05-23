// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


namespace SLua
{

	using System;
	using System.Collections.Generic;
	using System.Collections;
	using LuaInterface;
	using System.IO;
	using System.Text;
	using System.Runtime.InteropServices;
	#if !SLUA_STANDALONE
	using UnityEngine;
	#endif


	public class LuaFunction : LuaVar
	{
		public LuaFunction(LuaState l, int r)
			: base(l, r)
		{
		}

		public LuaFunction(IntPtr l, int r)
			: base(l, r)
		{
		}

		public bool pcall(int nArgs, int errfunc)
		{

			if (!state.isMainThread())
			{
				Logger.LogError("Can't call lua function in bg thread");
				return false;
			}

			LuaDLL.lua_getref(L, valueref);

			if (!LuaDLL.lua_isfunction(L, -1))
			{
				LuaDLL.lua_pop(L, 1);
				throw new Exception("Call invalid function.");
			}

			LuaDLL.lua_insert(L, -nArgs - 1);
			if (LuaDLL.lua_pcall(L, nArgs, -1, errfunc) != 0)
			{
				LuaDLL.lua_pop(L, 1);
				return false;
			}
			return true;
		}

		bool innerCall(int nArgs, int errfunc)
		{
			bool ret = pcall(nArgs, errfunc);
			LuaDLL.lua_remove(L, errfunc);
			return ret;
		}


		public object call()
		{
			int error = LuaObject.pushTry(state.L);
			if (innerCall(0, error))
			{
				return state.topObjects(error - 1);
			}
			return null;
		}

		public object call(params object[] args)
		{
			int error = LuaObject.pushTry(state.L);

			for (int n = 0; args != null && n < args.Length; n++)
			{
				LuaObject.pushVar(L, args[n]);
			}

			if (innerCall(args != null ? args.Length : 0, error))
			{
				return state.topObjects(error - 1);
			}

			return null;
		}

		public object call(object a1)
		{
			int error = LuaObject.pushTry(state.L);

			LuaObject.pushVar(state.L, a1);
			if (innerCall(1, error))
			{
				return state.topObjects(error - 1);
			}


			return null;
		}

		public object call(object a1, object a2)
		{
			int error = LuaObject.pushTry(state.L);

			LuaObject.pushVar(state.L, a1);
			LuaObject.pushVar(state.L, a2);
			if (innerCall(2, error))
			{
				return state.topObjects(error - 1);
			}
			return null;
		}

		public object call(object a1, object a2, object a3)
		{
			int error = LuaObject.pushTry(state.L);

			LuaObject.pushVar(state.L, a1);
			LuaObject.pushVar(state.L, a2);
			LuaObject.pushVar(state.L, a3);
			if (innerCall(3, error))
			{
				return state.topObjects(error - 1);
			}
			return null;
		}

		// you can add call method with specific type rather than object type to avoid gc alloc, like
		// public object call(int a1,float a2,string a3,object a4)

		// using specific type to avoid type boxing/unboxing

		public object call (float f1) {
			int error = LuaObject.pushTry(state.L);

			LuaDLL.lua_pushnumber(L, f1);
			if (innerCall (1, error)) {
				return state.topObjects (error - 1);
			}
			return null;
		}

		public object call (object a1, float f2) {
			int error = LuaObject.pushTry(state.L);

			LuaObject.pushVar (state.L, a1);
			LuaDLL.lua_pushnumber(L, f2);
			if (innerCall (2, error)) {
				return state.topObjects (error - 1);
			}
			return null;
		}
	}

}
