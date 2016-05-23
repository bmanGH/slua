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

	public class LuaThread : LuaVar
	{
		protected object yieldResult;

		public LuaThread(IntPtr l, int r)
			: base(l, r)
		{ }

		public LuaThreadStatus Resume ()
		{
			if (!state.isMainThread())
			{
				Logger.LogError("Can't resume lua thread in bg thread");
				return LuaThreadStatus.LUA_ERRERR;
			}

			yieldResult = null;

			IntPtr L = state.L;
			push (L);
			IntPtr threadL = LuaDLL.lua_tothread(L, -1);
			LuaDLL.lua_pop (L, 1);

			int ret = LuaDLL.lua_resume(threadL, 0);

			if (ret > (int)LuaThreadStatus.LUA_YIELD) {
				LuaObject.pushTry(threadL);
				LuaDLL.lua_pushthread(threadL);
				LuaDLL.lua_pushvalue(threadL, -3); // error message
				LuaDLL.lua_call(threadL, 2, 0);

				LuaDLL.lua_settop(threadL, 0);
			} else {
				int top = LuaDLL.lua_gettop (threadL);
				if (top > 0) {
					if (top == 1) {
						yieldResult = LuaObject.checkVar (threadL, top);
						LuaDLL.lua_pop (threadL, 1);
					} else {
						object[] o = new object[top];
						for (int n = 1; n <= top; n++) {
							o [n - 1] = LuaObject.checkVar (threadL, n);
						}
						LuaDLL.lua_settop (L, 0);
						yieldResult = o;
					}
				}
			}

			return (LuaThreadStatus)ret;
		}

		public LuaThreadStatus Resume (params object[] args)
		{
			if (!state.isMainThread())
			{
				Logger.LogError("Can't resume lua thread in bg thread");
				return LuaThreadStatus.LUA_ERRERR;
			}

			yieldResult = null;

			IntPtr L = state.L;
			push (L);
			IntPtr threadL = LuaDLL.lua_tothread(L, -1);
			LuaDLL.lua_pop (L, 1);

			int nArgs = 0;
			if (args != null)
			{
				nArgs = args.Length;
				for (int i = 0; i < args.Length; i++)
				{
					LuaObject.pushVar (threadL, args[i]);
				}
			}

			int ret = LuaDLL.lua_resume(threadL, nArgs);

			if (ret > (int)LuaThreadStatus.LUA_YIELD) {
				LuaObject.pushTry(threadL);
				LuaDLL.lua_pushthread(threadL);
				LuaDLL.lua_pushvalue(threadL, -3); // error message
				LuaDLL.lua_call(threadL, 2, 0);

				LuaDLL.lua_settop(threadL, 0);
			} else {
				int top = LuaDLL.lua_gettop (threadL);
				if (top > 0) {
					if (top == 1) {
						yieldResult = LuaObject.checkVar (threadL, top);
						LuaDLL.lua_pop (threadL, 1);
					} else {
						object[] o = new object[top];
						for (int n = 1; n <= top; n++) {
							o [n - 1] = LuaObject.checkVar (threadL, n);
						}
						LuaDLL.lua_settop (L, 0);
						yieldResult = o;
					}
				}
			}

			return (LuaThreadStatus)ret;
		}

		public LuaThreadStatus getStatus ()
		{
			IntPtr L = state.L;
			push (L);
			IntPtr threadL = LuaDLL.lua_tothread(L, -1);
			LuaDLL.lua_pop (L, 1);

			return (LuaThreadStatus)LuaDLL.lua_status (threadL);
		}

		public object GetYieldResult ()
		{
			return yieldResult;
		}
	}

}
