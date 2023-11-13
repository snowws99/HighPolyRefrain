using System.Collections.Generic;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
    /// <summary>
    /// Represents a key for storing and comparing positions of vertices in PMX software.
    /// </summary>
    internal class CVPosKey
	{
        /// <summary>
        /// Gets or sets the position vector associated with this key.
        /// </summary>
        public V3 m_Pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="CVPosKey"/> class using the provided vertex.
        /// </summary>
        /// <param name="In">The <see cref="IPXVertex"/> instance containing the position information.</param>
        public CVPosKey(IPXVertex In)
		{
            // Extract the position vector from the provided vertex
            m_Pos = new V3(In.Position);
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="CVPosKey"/> class using a vertex set.
        /// </summary>
        /// <param name="In">The <see cref="CVSet"/> instance containing vertices to create the position key.</param>
        public CVPosKey(CVSet In)
		{
			using (HashSet<IPXVertex>.Enumerator enumerator = In.m_vset.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
                    // Extract the position vector from the first vertex in the set
                    IPXVertex current = enumerator.Current;
					m_Pos = new V3(current.Position);
				}
			}
		}

        /// <summary>
        /// Determines whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the objects are equal, otherwise false.</returns>
        public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

            // Compare positions with a tolerance of 1E-06f
            return (m_Pos - ((CVPosKey)obj).m_Pos).Length() < 1E-06f;
		}

        /// <summary>
        /// Serves as a hash function for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
		{
            // Calculate a hash code based on the sum of position components multiplied by 1000f
            return (int)((m_Pos.X + m_Pos.Y + m_Pos.Z) * 1000f);
		}
	}
}
