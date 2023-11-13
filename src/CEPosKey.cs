using System.Linq;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
    /// <summary>
    /// Represents a key for storing and comparing positions of two vertices in PMX MMD software.
    /// </summary>
    internal class CEPosKey
    {
        /// <summary>
        /// Gets or sets the position of the first vertex.
        /// </summary>
        public V3 m_Pos1 { get; set; }

        /// <summary>
        /// Gets or sets the position of the second vertex.
        /// </summary>
        public V3 m_Pos2 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CEPosKey"/> class.
        /// </summary>
        /// <param name="EdgeKey">A <see cref="CVSet"/> containing two vertices representing an edge.</param>
        public CEPosKey(CVSet EdgeKey)
        {
            // Extracting positions from the vertex set
            IPXVertex[] array = EdgeKey.m_vset.ToArray();
            m_Pos1 = new V3(array[0].Position);
            m_Pos2 = new V3(array[1].Position);
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

            // Comparing positions with a tolerance of 1E-06f
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

        /// <summary>
        /// Serves as a hash function for the current object.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated by summing up the individual components (X, Y, and Z) of both positions 
        /// (m_Pos1 and m_Pos2) and multiplying the result by 1000f. This provides a simple hashing mechanism 
        /// that incorporates the positions of the vertices with a scaling factor.
        /// </remarks>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            // Calculating a hash code based on the sum of position components multiplied by 1000f
            return (int)((m_Pos1.X + m_Pos1.Y + m_Pos1.Z + m_Pos2.X + m_Pos2.Y + m_Pos2.Z) * 1000f);
        }
    }

}
