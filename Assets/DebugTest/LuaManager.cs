using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

namespace Assets.DebugTest
{
	public class LuaManager : Singleton<LuaManager>
	{
		private readonly char[] pathSep = { '#', '.', '@' };
		private readonly Dictionary<string, string> luaFilePath = new Dictionary<string, string>();
		private LuaEnv luaEnv;
		private Dictionary<string, string[]> luaSourceFile;
		private readonly Dictionary<string, LuaTable> staticLuaTable = new Dictionary<string, LuaTable>();
		private readonly Dictionary<string, Dictionary<string, LuaFunction>> staticLuaFunc = new Dictionary<string, Dictionary<string, LuaFunction>>();
		private LuaFunction includeFunction;
		private string debugLibPath;


		private string LuaSourceFolder
		{
			get
			{
				return Path.GetFullPath("lua");
			}
		}

		public void Init()
		{
			luaEnv = new LuaEnv();
			luaEnv.AddLoader(LoadSource);
			Lua.lua_pushstdcallcfunction(luaEnv.L, Print);
			if (0 != Lua.xlua_setglobal(luaEnv.L, "print"))
			{
				throw new Exception("call xlua_setglobal fail!");
			}
			//object[] results = luaEnv.DoString("return require 'Debug'");
			//if (results != null && results.Length > 0 && results[0] is string)
			//{
			//	debugLibPath = results[0] as string;
			//	luaEnv.Global.SetInPath("debugMode", true);
			//	luaEnv.AddLoader(LoadDebug);
			//}

			luaEnv.DoString("require 'Entry'");
			includeFunction = luaEnv.Global.Get<LuaFunction>("include");
		}

		public object[] CallStaticLuaFunc(string clazz, string func, params object[] parameters)
		{
			LuaFunction f = GetStaticLuaTable(clazz, func);
			return f != null ? f.Call(parameters) : null;
		}

		private LuaFunction GetStaticLuaTable(string tableName, string func)
		{
			if (includeFunction == null) return null;

			LuaFunction result = null;
			if (!staticLuaTable.ContainsKey(tableName))
			{
				LuaTable table = includeFunction.Func<string, LuaTable>(tableName);
				if (table != null)
				{
					staticLuaTable[tableName] = table;
					staticLuaFunc[tableName] = new Dictionary<string, LuaFunction>();

					result = table.Get<LuaFunction>(func);
					if (result != null)
					{
						staticLuaFunc[tableName][func] = result;
					}
				}
			}
			else
			{
				var funcs = staticLuaFunc[tableName];
				if (funcs.ContainsKey(func))
					result = funcs[func];
				else
				{
					result = staticLuaTable[tableName].Get<LuaFunction>(func);
					if (result != null)
					{
						funcs[func] = result;
					}
				}
			}

			return result;
		}

		private byte[] LoadDebug(ref string filepath)
		{
			filepath = Path.Combine(debugLibPath, filepath.Replace(".", "/") + ".lua");
			if (filepath != null && File.Exists(filepath))
			{
				return File.ReadAllBytes(filepath);
			}
			return null;
		}


		private byte[] LoadSource(ref string filepath)
		{
			byte[] str = null;

			string path = luaFilePath.ContainsKey(filepath) ? luaFilePath[filepath] :
				luaFilePath[filepath] = GetLuaSourceFile(filepath);

			if (File.Exists(path))
			{
				str = File.ReadAllBytes(path);
				filepath = path;
			}
			else
			{
				throw new Exception("Load lua source failed " + filepath);
			}
			return str;
		}

		private string GetLuaSourceFile(string name)
		{
			var first = name[0];
			if (first == '#' || first == '@')
			{
				int index = name.LastIndexOfAny(pathSep);
				if (index >= 0)
				{
					string filename = name.Substring(index + 1);
					string fullname = name.Substring(1);
					string[] files = GetLuaFiles(filename);
					for (int i = 0; i < files.Length; i++)
					{
						if (first == '@' || CheckLuaFileWithNamespace(files[i], fullname, filename))
							return files[i];
					}
				}
				return null;
			}
			var pathName = name.Replace('.', '/') + ".lua";
			return LuaSourceFolder + "/" + pathName;
		}

		private string[] GetLuaFiles(string filename)
		{
			if (luaSourceFile == null)
			{
				var dict = new Dictionary<string, List<string>>(StringComparer.Ordinal);
				foreach (var file in Directory.GetFiles(LuaSourceFolder, "*.lua", SearchOption.AllDirectories))
				{
					var name = Path.GetFileNameWithoutExtension(file);
					if (!dict.ContainsKey(name))
					{
						dict[name] = new List<string>();
					}
					dict[name].Add(file);
				}

				luaSourceFile = new Dictionary<string, string[]>(dict.Count, StringComparer.Ordinal);
				foreach (var item in dict)
				{
					luaSourceFile[item.Key] = item.Value.ToArray();
				}
			}
			return luaSourceFile.ContainsKey(filename) ? luaSourceFile[filename] : new string[0];
		}

		private string GetLuaFileNamespace(string path)
		{
			using (var f = File.OpenText(path))
			{
				string line;
				while ((line = f.ReadLine()) != null)
				{
					line = line.Trim();
					if (line.StartsWith("namespace", StringComparison.Ordinal))
					{
						line = line.Substring(10).Trim();
						line = line.Substring(1, line.Length - 2);
						return line;
					}
					if (line != "" && !line.StartsWith("--", StringComparison.Ordinal))
					{
						return null;
					}
				}
			}
			return null;
		}

		private bool CheckLuaFileWithNamespace(string path, string filename, string fullname)
		{
			return GetLuaFileNamespace(path) + "." + filename == fullname;
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		protected static int Print(IntPtr L)
		{
			try
			{
				int n = Lua.lua_gettop(L);
				string s = String.Empty;

				if (0 != Lua.xlua_getglobal(L, "tostring"))
				{
					return Lua.luaL_error(L, "can not get tostring in print:");
				}

				for (int i = 1; i <= n; i++)
				{
					Lua.lua_pushvalue(L, -1);  /* function to be called */
					Lua.lua_pushvalue(L, i);   /* value to print */
					if (0 != Lua.lua_pcall(L, 1, 1, 0))
					{
						return Lua.lua_error(L);
					}
					s += Lua.lua_tostring(L, -1);

					if (i != n) s += "\t";

					Lua.lua_pop(L, 1);  /* pop result */
				}

				Debug.Log("LUA_PRINT: " + s);

				return 0;
			}
			catch (Exception e)
			{
				return Lua.luaL_error(L, "c# exception in print:" + e);
			}
		}
	}
}
