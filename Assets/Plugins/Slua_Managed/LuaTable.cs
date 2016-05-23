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


	public class LuaTable : LuaVar, IEnumerable<LuaTable.TablePair>
	{

		public struct TablePair
		{
			public object key;
			public object value;
		}
		public LuaTable(IntPtr l, int r)
			: base(l, r)
		{
		}

		public LuaTable(LuaState l, int r)
			: base(l, r)
		{
		}

		public LuaTable(LuaState state)
			: base(state, 0)
		{

			LuaDLL.lua_newtable(L);
			valueref = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
		}
		public object this[string key]
		{
			get
			{
				return state.getObject(valueref, key);
			}

			set
			{
				state.setObject(valueref, key, value);
			}
		}

		public object this[int index]
		{
			get
			{
				return state.getObject(valueref, index);
			}

			set
			{
				state.setObject(valueref, index, value);
			}
		}

		public object invoke(string func, params object[] args)
		{
			LuaFunction f = (LuaFunction)this[func];
			if (f != null)
			{
				return f.call(args);
			}
			throw new Exception(string.Format("Can't find {0} function", func));
		}

		public int length()
		{
			int n = LuaDLL.lua_gettop(L);
			push(L);
			int l = LuaDLL.lua_rawlen(L, -1);
			LuaDLL.lua_settop(L, n);
			return l;
		}

		public class Enumerator : IEnumerator<TablePair>, IDisposable
		{
			LuaTable t;
			int indext = -1;
			TablePair current = new TablePair();
			int iterPhase = 0;

			public Enumerator(LuaTable table)
			{
				t = table;
				Reset();
			}

			public bool MoveNext()
			{
				if (indext < 0)
					return false;

				if (iterPhase == 0)
				{
					LuaDLL.lua_pushnil(t.L);
					iterPhase = 1;
				}
				else
					LuaDLL.lua_pop(t.L, 1);

				bool ret = LuaDLL.lua_next(t.L, indext) > 0;
				if (!ret) iterPhase = 2;

				return ret;
			}

			public void Reset()
			{
				LuaDLL.lua_getref(t.L, t.Ref);
				indext = LuaDLL.lua_gettop(t.L);
			}

			public void Dispose()
			{
				if (iterPhase == 1)
					LuaDLL.lua_pop(t.L, 2);

				LuaDLL.lua_remove(t.L, indext);
			}

			public TablePair Current
			{
				get
				{
					current.key = LuaObject.checkVar(t.L, -2);
					current.value = LuaObject.checkVar(t.L, -1);
					return current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}
		}

		public IEnumerator<TablePair> GetEnumerator()
		{
			return new LuaTable.Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

	}

}
