using System.Collections.Generic;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
	internal class CVPosKey
	{
		public V3 m_Pos;

		public CVPosKey(IPXVertex In)
		{
			m_Pos = new V3(In.Position);
		}

		public CVPosKey(CVSet In)
		{
			using (HashSet<IPXVertex>.Enumerator enumerator = In.m_vset.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IPXVertex current = enumerator.Current;
					m_Pos = new V3(current.Position);
				}
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return (m_Pos - ((CVPosKey)obj).m_Pos).Length() < 1E-06f;
		}

		public override int GetHashCode()
		{
			return (int)((m_Pos.X + m_Pos.Y + m_Pos.Z) * 1000f);
		}
	}
}
