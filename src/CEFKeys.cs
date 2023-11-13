using System.Collections.Generic;

namespace PECSScriptPlugin
{
	internal class CEFKeys
	{
		public HashSet<CVSet> m_EdgeKeys;

		public HashSet<CVSet> m_FaceKeys;

		public CEFKeys()
		{
			m_EdgeKeys = new HashSet<CVSet>();
			m_FaceKeys = new HashSet<CVSet>();
		}
	}
}
