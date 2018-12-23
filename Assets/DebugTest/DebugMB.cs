using UnityEngine;

namespace Assets.DebugTest
{
	public class DebugMB : MonoBehaviour
	{
		private void Start()
		{
			LuaManager.Instance.Init();
		}

		public void Test()
		{
			LuaManager.Instance.CallStaticLuaFunc("Test.TestManager", "TestAdd", 3, 4);
		}
	}
}
