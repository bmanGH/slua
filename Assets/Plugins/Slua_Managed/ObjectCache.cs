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
	using System.Runtime.InteropServices;
	using System.Collections.Generic;
	using LuaInterface;
	using System.Runtime.CompilerServices;

	public class ObjectCache
	{
		static Dictionary<IntPtr, ObjectCache> multiState = new Dictionary<IntPtr, ObjectCache>();

		static IntPtr oldl = IntPtr.Zero;
		static internal ObjectCache oldoc = null;

#if LUA_OBJECT_CACHE_TRACE
		private IntPtr _l;
		private Dictionary<int, string> _cacheTrace = new Dictionary<int, string> ();
		private Dictionary<int, string> _cacheTypeName = new Dictionary<int, string> ();

		public Dictionary<int, string> cacheTrace {
			get { return _cacheTrace; }
		}
		public Dictionary<int, string> cacheTypeName {
			get { return _cacheTypeName; }
		}

		public object getCachedObject (int index) {
			return get (_l, index);
		}
#endif

		public static ObjectCache get(IntPtr l)
		{
			if (oldl == l)
				return oldoc;
			ObjectCache oc;
			if (multiState.TryGetValue(l, out oc))
			{
				oldl = l;
				oldoc = oc;
				return oc;
			}

			LuaDLL.lua_getglobal(l, "__main_state");
			if (LuaDLL.lua_isnil(l, -1))
			{
				LuaDLL.lua_pop(l, 1);
				return null;
			}

			IntPtr nl = LuaDLL.lua_touserdata(l, -1);
			LuaDLL.lua_pop(l, 1);
			if (nl != l)
				return get(nl);
			return null;
		}

		public class ObjSlot
		{
			public int freeslot;
			public object v;
			public ObjSlot(int slot, object o)
			{
				freeslot = slot;
				v = o;
			}
		}

		public class ObjEqualityComparer : IEqualityComparer<object>
		{
			public new bool Equals(object x, object y)
			{
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

//#if SPEED_FREELIST
		public class FreeList : List<ObjSlot>
		{
			public FreeList()
			{
				this.Add(new ObjSlot(0, null));
			}

			public int add(object o)
			{
				ObjSlot free = this[0];
				if (free.freeslot == 0)
				{
					Add(new ObjSlot(this.Count, o));
					return this.Count - 1;
				}
				else
				{
					int slot = free.freeslot;
					free.freeslot = this[slot].freeslot;
					this[slot].v = o;
					this[slot].freeslot = slot;
					return slot;
				}
			}

			public void del(int i)
			{
				ObjSlot free = this[0];
				this[i].freeslot = free.freeslot;
				this[i].v = null;
				free.freeslot = i;
			}

			public bool get(int i, out object o)
			{
				if (i < 1 || i > this.Count)
				{
					throw new ArgumentOutOfRangeException();
				}

				ObjSlot slot = this[i];
				o = slot.v;
				return o != null;
			}

			public object get(int i)
			{
				object o;
				if (get(i, out o))
					return o;
				return null;
			}

			public void set(int i, object o)
			{
				this[i].v = o;
			}
		}
//#else
//		public class FreeList : Dictionary<int, object>
//		{
//			private int id = 1;
//			public int add(object o)
//			{
//				Add(id, o);
//				return id++;
//			}
//
//			public void del(int i)
//			{
//				this.Remove(i);
//			}
//
//			public bool get(int i, out object o)
//			{
//				return TryGetValue(i, out o);
//			}
//
//			public object get(int i)
//			{
//				object o;
//				if (TryGetValue(i, out o))
//					return o;
//				return null;
//			}
//
//			public void set(int i, object o)
//			{
//				this[i] = o;
//			}
//		}
//#endif

		public FreeList cache = new FreeList();
		public Dictionary<object, int> objMap = new Dictionary<object, int>(new ObjEqualityComparer());
		internal int udCacheRef = 0;

		public ObjectCache(IntPtr l)
		{
			LuaDLL.lua_newtable(l);
			LuaDLL.lua_newtable(l);
			LuaDLL.lua_pushstring(l, "v");
			LuaDLL.lua_setfield(l, -2, "__mode");
			LuaDLL.lua_setmetatable(l, -2);
			udCacheRef = LuaDLL.luaL_ref(l, LuaIndexes.LUA_REGISTRYINDEX);

#if LUA_OBJECT_CACHE_TRACE
			_l = l;
#endif
		}

		static public void clear()
		{
			oldl = IntPtr.Zero;
			oldoc = null;
		}

		internal static void del(IntPtr l)
		{
			multiState.Remove(l);
		}

		internal static void make(IntPtr l)
		{
			ObjectCache oc = new ObjectCache(l);
			multiState[l] = oc;
			oldl = l;
			oldoc = oc;
		}

		internal void gc(int index)
		{
			object o;
			if (cache.get(index, out o))
			{
				int oldindex;
				if (isGcObject(o) && objMap.TryGetValue(o,out oldindex) && oldindex==index)
				{
					objMap.Remove(o);
				}
				cache.del(index);

#if LUA_OBJECT_CACHE_TRACE
				_cacheTrace.Remove(index);
				_cacheTypeName.Remove(index);
#endif
			}
		}
#if !SLUA_STANDALONE
        internal void gc(UnityEngine.Object o)
        {
            int index;
            if(objMap.TryGetValue(o, out index))
            {
                objMap.Remove(o);
                cache.del(index);

	#if LUA_OBJECT_CACHE_TRACE
				_cacheTrace.Remove(index);
				_cacheTypeName.Remove(index);
	#endif
            }
        }
		public void releaseUnityObject (UnityEngine.Object o)
		{
			gc (o);
		}
#endif

		internal int add(object o)
		{
			int objIndex = cache.add(o);
			if (isGcObject(o))
			{
				objMap[o] = objIndex;
			}

#if LUA_OBJECT_CACHE_TRACE
			LuaDLL.lua_getglobal(_l, "debug");
			LuaDLL.lua_getfield(_l, -1, "traceback");
			LuaDLL.lua_call(_l, 0, 1);
			LuaDLL.lua_remove(_l, -2);
			string s = LuaDLL.lua_tostring(_l, -1);
			LuaDLL.lua_pop(_l, 1);

			_cacheTrace.Add(objIndex, s);
			_cacheTypeName.Add(objIndex, o.GetType().Name);
#endif

			return objIndex;
		}

		internal object get(IntPtr l, int p)
		{
			int index = LuaDLL.luaS_rawnetobj(l, p);
			object o;
			if (index != -1 && cache.get(index, out o))
			{
				return o;
			}
			return null;
		}

		internal void setBack(IntPtr l, int p, object o)
		{
			int index = LuaDLL.luaS_rawnetobj(l, p);
			if (index != -1)
			{
				cache.set(index, o);
			}
		}

		internal void push(IntPtr l, object o)
		{
			push(l, o, true);
		}

		internal void push(IntPtr l, Array o)
		{
			push(l, o, true, true);
		}

		internal void push(IntPtr l, object o, bool checkReflect, bool isArray=false)
		{
			if (o == null)
			{
				LuaDLL.lua_pushnil(l);
				return;
			}

			int index = -1;

			bool gco = isGcObject(o);
			bool found = gco && objMap.TryGetValue(o, out index);
			if (found)
			{
				if (LuaDLL.luaS_getcacheud(l, index, udCacheRef) == 1)
					return;
			}

			index = add(o);
//#if SLUA_CHECK_REFLECTION
			int isReflect = LuaDLL.luaS_pushobject(l, index, isArray ? "LuaArray" : getAQName(o), gco, udCacheRef);
			if (isReflect != 0 && checkReflect && !isArray)
			{
				Logger.LogWarning(string.Format("{0} not exported, using reflection instead", o.ToString()));
			}
//#else
//			LuaDLL.luaS_pushobject(l, index, isArray?"LuaArray":getAQName(o), gco, udCacheRef);
//#endif

			LuaDLL.lua_pushvalue(l, LuaObject.TOLUA_NOPEER);
#if LUA_5_3
			LuaDLL.lua_setuservalue(l, -2);
#else
			LuaDLL.lua_setfenv(l, -2);
#endif
		}

		public bool isGcObject(object obj)
		{
			return obj.GetType().IsValueType == false;
		}

		static Dictionary<Type, string> aqnameMap = new Dictionary<Type, string>();

		public static string getAQName(object o)
		{
			Type t = o.GetType();
			return getAQName(t);
		}

		public static string getAQName(Type t)
		{
			string name;
			if (aqnameMap.TryGetValue(t, out name))
			{
				return name;
			}
			name = t.AssemblyQualifiedName;
			aqnameMap[t] = name;
			return name;
		}

	}
}

