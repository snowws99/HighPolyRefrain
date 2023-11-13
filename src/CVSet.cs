using System.Collections.Generic;
using PEPlugin.Pmx;

namespace PECSScriptPlugin
{
	internal class CVSet
	{
		public HashSet<IPXVertex> m_vset;

		public CVSet()
		{
			m_vset = new HashSet<IPXVertex>();
		}

		public CVSet(CVSet p)
		{
			m_vset = new HashSet<IPXVertex>(p.m_vset);
		}

		public CVSet(IPXVertex v)
		{
			m_vset = new HashSet<IPXVertex> { v };
		}

		public CVSet(IPXVertex[] arr)
		{
			m_vset = new HashSet<IPXVertex>(arr);
		}

		public void Add(IPXVertex In)
		{
			m_vset.Add(In);
		}

		public void Add(CVSet In)
		{
			m_vset.UnionWith(In.m_vset);
		}

		public HashSet<CVSet> Separate()
		{
			HashSet<CVSet> hashSet = new HashSet<CVSet>();
			foreach (IPXVertex item in m_vset)
			{
				hashSet.Add(new CVSet(item));
			}
			return hashSet;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			HashSet<IPXVertex> hashSet = new HashSet<IPXVertex>(m_vset);
			hashSet.SymmetricExceptWith(((CVSet)obj).m_vset);
			return hashSet.Count == 0;
		}

		public override int GetHashCode()
		{
			int hash = 0;
			foreach (IPXVertex item in m_vset)
			{
				hash ^= item.GetHashCode();
			}
			return hash;
		}
	}
}
