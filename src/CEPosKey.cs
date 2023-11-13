using System.Linq;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
	internal class CEPosKey
	{
		public V3 m_Pos1;

		public V3 m_Pos2;

		public CEPosKey(CVSet EdgeKey)
		{
			IPXVertex[] array = EdgeKey.m_vset.ToArray();
			m_Pos1 = new V3(array[0].Position);
			m_Pos2 = new V3(array[1].Position);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			if (!((m_Pos1 - ((CEPosKey)obj).m_Pos1).Length() < 1E-06f) || !((m_Pos2 - ((CEPosKey)obj).m_Pos2).Length() < 1E-06f))
			{
				if ((m_Pos1 - ((CEPosKey)obj).m_Pos2).Length() < 1E-06f)
				{
					return (m_Pos2 - ((CEPosKey)obj).m_Pos1).Length() < 1E-06f;
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return (int)((m_Pos1.X + m_Pos1.Y + m_Pos1.Z + m_Pos2.X + m_Pos2.Y + m_Pos2.Z) * 1000f);
		}
	}
}
