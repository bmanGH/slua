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

	abstract public class LuaVar : IDisposable
	{
		protected LuaState state = null;
		protected int valueref = 0;

		public IntPtr L
		{
			get
			{
				return state.L;
			}
		}

		public int Ref
		{
			get
			{
				return valueref;
			}
		}

		public LuaVar()
		{
			state = null;
		}

		public LuaVar(LuaState l, int r)
		{
			state = l;
			valueref = r;
		}

		public LuaVar(IntPtr l, int r)
		{
			state = LuaState.get(l);
			valueref = r;
		}

		~LuaVar()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Dispose(bool disposeManagedResources)
		{
			if (valueref != 0)
			{
				//HACK: 在WebGL平台下，切换场景后state.refQueue可能会出现未知的长度越界问题，导致后续调用出现空指针错误
				//		并且WebGL没有多线程的环境，故临时在此处避免使用到state.refQueue来避免问题的发生
#if UNITY_WEBGL && !UNITY_EDITOR
				LuaDLL.lua_unref(state.L, valueref);
				valueref = 0;
#else
				LuaState.UnRefAction act = (IntPtr l, int r) =>
				{
					LuaDLL.lua_unref(l, r);
				};
				state.gcRef(act, valueref);
				valueref = 0;
#endif
			}
		}

		public void push(IntPtr l)
		{
			LuaDLL.lua_getref(l, valueref);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is LuaVar)
			{
				return this == (LuaVar)obj;
			}
			return false;
		}

		public static bool operator ==(LuaVar x, LuaVar y)
		{
			if ((object)x == null || (object)y == null)
				return (object)x == (object)y;

			return Equals(x, y) == 1;
		}

		public static bool operator !=(LuaVar x, LuaVar y)
		{
			if ((object)x == null || (object)y == null)
				return (object)x != (object)y;

			return Equals(x, y) != 1;
		}

		static int Equals(LuaVar x, LuaVar y)
		{
			x.push(x.L);
			y.push(x.L);
			int ok = LuaDLL.lua_equal(x.L, -1, -2);
			LuaDLL.lua_pop(x.L, 2);
			return ok;
		}
	}

}
