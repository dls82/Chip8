using Chip8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8Test
{
	[TestClass]
	public class TestDisplay
	{
		[TestMethod]
		public void TestMethod1()
		{
			Display d = new Display();
			d.Draw(0, 0, Display.SPRITE_F);
			var s = d.ToString();
		}
	}
}
