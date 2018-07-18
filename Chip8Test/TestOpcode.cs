using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8Test
{
	[TestClass]
	public class TestOpcode
	{
		[TestMethod]
		public void OpcodeProperties()
		{
			var opcode = new Chip8.Opcode();
			opcode.Update(0x10, 0x00);
			Assert.AreEqual(0x1, opcode.Type);
			Assert.AreEqual(0x0, opcode.X);
			Assert.AreEqual(0x0, opcode.Y);
			Assert.AreEqual(0x0, opcode.Nibble);
			Assert.AreEqual(0x000, opcode.NNN);
			Assert.AreEqual(0x1000, opcode.Value);

			opcode.Update(0x1e, 0xbc);
			Assert.AreEqual(0x1, opcode.Type);
			Assert.AreEqual(0xe, opcode.X);
			Assert.AreEqual(0xb, opcode.Y);
			Assert.AreEqual(0xc, opcode.Nibble);
			Assert.AreEqual(0xebc, opcode.NNN);
			Assert.AreEqual(0x1ebc, opcode.Value);

			opcode.Update(0x12, 0x22);
			Assert.AreEqual(0x12, opcode.mUpper);
			Assert.AreEqual(0x22, opcode.mLower);
			Assert.AreEqual(0x1, opcode.Type);
			Assert.AreEqual(0x2, opcode.X);
			Assert.AreEqual(0x2, opcode.Y);
			Assert.AreEqual(0x2, opcode.Nibble);
			Assert.AreEqual(0x222, opcode.NNN);
			Assert.AreEqual(0x1222, opcode.Value);

			opcode = new Chip8.Opcode(0x1222);
			Assert.AreEqual(0x12, opcode.mUpper);
			Assert.AreEqual(0x22, opcode.mLower);
			Assert.AreEqual(0x1, opcode.Type);
			Assert.AreEqual(0x2, opcode.X);
			Assert.AreEqual(0x2, opcode.Y);
			Assert.AreEqual(0x2, opcode.Nibble);
			Assert.AreEqual(0x222, opcode.NNN);
			Assert.AreEqual(0x1222, opcode.Value);
		}
	}
}
