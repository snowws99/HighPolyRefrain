using System.Collections.Generic;

namespace PECSScriptPlugin
{
    /// <summary>
    /// Represents a container for storing edge and face keys in PMX MMD software.
    /// </summary>
    internal class CEFKeys
	{
        /// <summary>
        /// Gets or sets a set of keys representing edges.
        /// </summary>
        public HashSet<CVSet> m_EdgeKeys;

        /// <summary>
        /// Gets or sets a set of keys representing faces.
        /// </summary>
        public HashSet<CVSet> m_FaceKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="CEFKeys"/> class.
        /// </summary>
        public CEFKeys()
		{
            // Initialize the sets for edge and face keys
            m_EdgeKeys = new HashSet<CVSet>();
			m_FaceKeys = new HashSet<CVSet>();
		}
	}
}
