using System.Diagnostics;


namespace Assets.Utils
{
	enum LOGLEVEL
	{
		ERROR,
		META,
		WARNING,
		INFO,
		VERBOSE
	}
	static class Debug
	{
		/**
        Logging function that does
        a) not produce any logs in production (CONDITIONAL)
        b) supports setting DEBUG_LEVELS in the editor
        **/
		[Conditional("DEBUG")]
		public static void Log(object msg, LOGLEVEL level = LOGLEVEL.INFO)
		{
#if (DEBUG_LEVEL_4)
            if ((int)level > 4) return;
#elif (DEBUG_LEVEL_3)
            if ((int)level > 3) return;
#elif (DEBUG_LEVEL_2)
			if ((int)level > 2) return;
#elif (DEBUG_LEVEL_1)
            if ((int)level > 1) return;
#elif (DEBUG_LEVEL_0)
            if ((int)level > 0) return;
#endif
			//UnityEngine.Debug.Log(string.Format("{1}({2}): {0}", msg, level, (int)level));
		}
	}
}
