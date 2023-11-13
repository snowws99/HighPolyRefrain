using System;
using System.Windows.Forms;
using PEPlugin;

namespace PECSScriptPlugin
{
	public class CSScriptClass : PEPluginClass
	{
		public CSScriptClass()
		{
			m_option = new PEPluginOption(false, true, "High-Poly.Refrain");
		}

		public override void Run(IPERunArgs args)
		{
			base.Run(args);
			try
			{
				mainForm form = new mainForm(args);
				form.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
	}
}
