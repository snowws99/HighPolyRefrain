using System.Collections.Generic;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
	internal class CVDat
	{
		public IPXVertex m_v;

		public Dictionary<IPXMorph, V3> m_VMO;

		public Dictionary<IPXMorph, V4> m_UVMO;

		public CVDat(IPXPmxBuilder bdx)
		{
			m_v = bdx.Vertex();
			m_VMO = new Dictionary<IPXMorph, V3>();
			m_UVMO = new Dictionary<IPXMorph, V4>();
		}

		public CVDat(CVDat p)
		{
			m_v = (IPXVertex)p.m_v.Clone();
			m_VMO = new Dictionary<IPXMorph, V3>(p.m_VMO);
			m_UVMO = new Dictionary<IPXMorph, V4>(p.m_UVMO);
		}

		public CVDat(IPXVertex p)
		{
			m_v = (IPXVertex)p.Clone();
			m_VMO = new Dictionary<IPXMorph, V3>();
			m_UVMO = new Dictionary<IPXMorph, V4>();
		}
	}
}
