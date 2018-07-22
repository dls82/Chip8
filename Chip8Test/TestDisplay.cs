using Chip8;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Chip8Test
{
	[TestClass]
	public class TestDisplay
	{
		[TestMethod]
		public void TestDrawSprite()
		{
			Display d = new Display();
			Assert.IsFalse(d.Draw(1, 3, Display.SPRITE_F));
			Assert.IsTrue(!d[0, 3]);
			Assert.IsTrue(d[1, 3]);
			Assert.IsTrue(d[2, 3]);
			Assert.IsTrue(d[3, 3]);
			Assert.IsTrue(d[4, 3]);
			Assert.IsTrue(!d[5, 3]);
			Assert.IsTrue(!d[0, 4]);
			Assert.IsTrue(d[1, 4]);
			Assert.IsTrue(!d[2, 4]);
			Assert.IsTrue(!d[3, 4]);
			Assert.IsTrue(!d[4, 4]);
			Assert.IsTrue(!d[5, 4]);

			Assert.IsTrue(d.Draw(1, 3, Display.SPRITE_E));
			Assert.IsTrue(!d[0, 7]);
			Assert.IsTrue(!d[1, 7]);
			Assert.IsTrue(d[2, 7]);
			Assert.IsTrue(d[3, 7]);
			Assert.IsTrue(d[4, 7]);
			Assert.IsTrue(!d[5, 7]);
		}

		[TestMethod]
		public void TestDxyn()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				// draw character '2' at (2,3)... no collision
				new Opcode(0xA00A),	// set I := 0x005 (pointing towards '2')
				new Opcode(0x6002),	// set V0 := 0x02
				new Opcode(0x6103), // set V1 := 0x03
				new Opcode(0xD015), // draw

				 // draw characer '5' at (3,7)... w/ collision
				new Opcode(0xA019),	// set I := 0x019 (pointing towards '5')
				new Opcode(0x6203),	// set V2 := 0x03
				new Opcode(0x6307), // set V3 := 0x07
				new Opcode(0xD235), // draw

				// draw character '5' at (20,20)... no collision
				new Opcode(0x6414),	// set V4 := 0x14
				new Opcode(0x6514), // set V5 := 0x14
				new Opcode(0xD455), // draw
			};
			vm.Load(program);

			vm.Execute(); // 0xA00A
			vm.Execute(); // 0x6002
			vm.Execute(); // 0x6103
			vm.Execute(); // 0xD015
			Assert.AreEqual(0x208, vm.PC);
			Assert.IsTrue(!vm.Pixel(1, 3));
			Assert.IsTrue(vm.Pixel(2, 3));
			Assert.IsTrue(vm.Pixel(3, 3));
			Assert.IsTrue(vm.Pixel(4, 3));
			Assert.IsTrue(vm.Pixel(5, 3));
			Assert.IsTrue(!vm.Pixel(6, 3));
			Assert.IsTrue(!vm.Pixel(1, 7));
			Assert.IsTrue(vm.Pixel(2, 7));
			Assert.IsTrue(vm.Pixel(3, 7));
			Assert.IsTrue(vm.Pixel(4, 7));
			Assert.IsTrue(vm.Pixel(5, 7));
			Assert.IsTrue(!vm.Pixel(6, 7));
			Assert.AreEqual(0, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0xA019
			vm.Execute(); // 0x6203
			vm.Execute(); // 0x6307
			vm.Execute(); // 0xD235
			Assert.AreEqual(0x210, vm.PC);
			Assert.IsTrue(!vm.Pixel(1, 3));
			Assert.IsTrue(vm.Pixel(2, 3));
			Assert.IsTrue(vm.Pixel(3, 3));
			Assert.IsTrue(vm.Pixel(4, 3));
			Assert.IsTrue(vm.Pixel(5, 3));
			Assert.IsTrue(!vm.Pixel(6, 3));
			Assert.IsTrue(!vm.Pixel(1, 7));
			Assert.IsTrue(vm.Pixel(2, 7));
			Assert.IsTrue(!vm.Pixel(3, 7));
			Assert.IsTrue(!vm.Pixel(4, 7));
			Assert.IsTrue(!vm.Pixel(5, 7));
			Assert.IsTrue(vm.Pixel(6, 7));
			Assert.IsTrue(!vm.Pixel(7, 7));
			Assert.AreEqual(1, vm.Register(VM.RegisterName.VF));

			// I := 0x19 pointed at sprite for '5'
			vm.Execute(); // 0x6414
			vm.Execute(); // 0x6514
			vm.Execute(); // 0xD455
			Assert.AreEqual(0, vm.Register(VM.RegisterName.VF));
		}
	}
}
