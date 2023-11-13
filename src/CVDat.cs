using System.Collections.Generic;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
    /// <summary>
    /// Represents a data structure for storing vertex-related information, including morph targets.
    /// </summary>
    internal class CVDat
	{
        /// <summary>
        /// Gets or sets the <see cref="IPXVertex"/> associated with this data.
        /// </summary>
        public IPXVertex m_v;

        /// <summary>
        /// Gets or sets a dictionary of morph targets and their corresponding translation vectors.
        /// </summary>
        public Dictionary<IPXMorph, V3> m_VMO;

        /// <summary>
        /// Gets or sets a dictionary of morph targets and their corresponding UV translation vectors.
        /// </summary>
        public Dictionary<IPXMorph, V4> m_UVMO;

        /// <summary>
        /// Initializes a new instance of the <see cref="CVDat"/> class using the provided PMX builder.
        /// </summary>
        /// <param name="bdx">The <see cref="IPXPmxBuilder"/> instance used to create the vertex.</param>
        public CVDat(IPXPmxBuilder bdx)
		{
			m_v = bdx.Vertex();
			m_VMO = new Dictionary<IPXMorph, V3>();
			m_UVMO = new Dictionary<IPXMorph, V4>();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="CVDat"/> class by copying data from another instance.
        /// </summary>
        /// <param name="p">The source <see cref="CVDat"/> instance to copy data from.</param>
        public CVDat(CVDat p)
		{
            // Copy vertex and morph dictionaries from the source 
            m_v = (IPXVertex)p.m_v.Clone();
			m_VMO = new Dictionary<IPXMorph, V3>(p.m_VMO);
			m_UVMO = new Dictionary<IPXMorph, V4>(p.m_UVMO);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="CVDat"/> class using a specified vertex.
        /// </summary>
        /// <param name="p">The <see cref="IPXVertex"/> instance to use as the vertex data source.</param>
        public CVDat(IPXVertex p)
		{
            // Copy vertex data from the specified instance
            m_v = (IPXVertex)p.Clone();
			m_VMO = new Dictionary<IPXMorph, V3>();
			m_UVMO = new Dictionary<IPXMorph, V4>();
		}
	}
}
