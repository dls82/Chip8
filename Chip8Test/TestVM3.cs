using Chip8;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Chip8Test
{
	[TestClass]
	public class TestVM3
	{
		// opcodes: Fx07, Fx15, Fx18
		// the following isn't quite right as
		// DelayTimer not currently decremented by VM
		[TestMethod]
		public void TestF000Timers()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x6011),	// set V0 := 0x11
				new Opcode(0xF015),	// set DT := V0
				new Opcode(0xF018), // set ST := V0
				new Opcode(0xF107), // set V1 := DT
			};
			vm.Load(program);

			vm.Execute(); // 0x6011
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x11, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x0, vm.DelayTimer);
			Assert.AreEqual(0x0, vm.SoundTimer);

			vm.Execute(); // 0xF015
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x11, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x11, vm.DelayTimer);
			Assert.AreEqual(0x0, vm.SoundTimer);

			vm.Execute(); // 0xF018
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x11, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x11, vm.DelayTimer);
			Assert.AreEqual(0x11, vm.SoundTimer);

			vm.Execute(); // 0xF107
			Assert.AreEqual(0x208, vm.PC);
			Assert.AreEqual(0x11, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x11, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x11, vm.DelayTimer);
			Assert.AreEqual(0x11, vm.SoundTimer);
		}

		[TestMethod]
		public void TestFx1E()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0xA222),	// set I := 0x222
				new Opcode(0x6001),	// set V0 := 0x01
				new Opcode(0xF01E), // set I := I + V0
			};
			vm.Load(program);

			vm.Execute(); // 0xA222
			vm.Execute(); // 0x6001
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x222, vm.I);

			vm.Execute(); // 0xF01E
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x223, vm.I);
		}

		[TestMethod]
		public void TestFx33()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0xA080), // set I := 0x80 (just past sprites)
				new Opcode(0x607B),	// set V0 := 0x7B (= 123 decmial)
				new Opcode(0xF033), // place decimal digits of V0 in memory I, I+1, I+2
			};
			vm.Load(program);

			vm.Execute(); // 0xA080
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm[vm.I]);
			Assert.AreEqual(0x0, vm[1 + vm.I]);
			Assert.AreEqual(0x0, vm[2 + vm.I]);

			vm.Execute(); // 0x607B
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x7B, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm[vm.I]);
			Assert.AreEqual(0x0, vm[1 + vm.I]);
			Assert.AreEqual(0x0, vm[2 + vm.I]);

			vm.Execute(); // 0xF033
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x7B, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x1, vm[vm.I]);
			Assert.AreEqual(0x2, vm[1 + vm.I]);
			Assert.AreEqual(0x3, vm[2 + vm.I]);
		}

		// opcodes: Fx55
		[TestMethod]
		public void TestFx55()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0xA080), // set I := 0x001
				new Opcode(0x607E),	// set V0 := 0x7E
				new Opcode(0x617D),	// set V1 := 0x7D
				new Opcode(0x627B),	// set V2 := 0x7B
				new Opcode(0x637A),	// set V3 := 0x7A
				new Opcode(0x647C),	// set V4 := 0x7C
				new Opcode(0xF355), // store registers V0 through V3 starting at memory location I
			};
			vm.Load(program);

			vm.Execute(); // 0xA080
			vm.Execute(); // 0x607E
			vm.Execute(); // 0x617D
			vm.Execute(); // 0x627B
			vm.Execute(); // 0x637A
			vm.Execute(); // 0x647C
			Assert.AreEqual(0x20C, vm.PC);
			Assert.AreEqual(0x080, vm.I);
			Assert.AreEqual(0x0, vm[0x80]);
			Assert.AreEqual(0x0, vm[0x81]);
			Assert.AreEqual(0x0, vm[0x82]);
			Assert.AreEqual(0x0, vm[0x83]);
			Assert.AreEqual(0x0, vm[0x84]);

			vm.Execute(); // 0xF355
			Assert.AreEqual(0x20E, vm.PC);
			Assert.AreEqual(0x80, vm.I);
			Assert.AreEqual(0x7E, vm[0x80]);
			Assert.AreEqual(0x7D, vm[0x81]);
			Assert.AreEqual(0x7B, vm[0x82]);
			Assert.AreEqual(0x7A, vm[0x83]);
			Assert.AreEqual(0x00, vm[0x84]);
		}

		// opcodes: Fx65
		[TestMethod]
		public void TestFx65()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				// fill memory locations 0x083, 0x084, 0x085
				new Opcode(0xA085), // set I := 0x085
				new Opcode(0x607E),	// set V0 := 0x7E
				new Opcode(0xF055), // copy V0 to location I
				new Opcode(0xA084), // set I := 0x084
				new Opcode(0x607A),	// set V0 := 0x7A
				new Opcode(0xF055), // copy V0 to location I
				new Opcode(0xA083), // set I := 0x083
				new Opcode(0x607D),	// set V0 := 0x7D
				new Opcode(0xF055), // copy V0 to location I

				// copy memory locations 0x082 through 0x085 to registers
				new Opcode(0xA082), // set I := 0x082
				new Opcode(0xF265), // read registers V0 through V2 from memory starting at location I
			};
			vm.Load(program);

			vm.Execute();
			vm.Execute();
			vm.Execute();
			vm.Execute();
			vm.Execute();
			vm.Execute();
			vm.Execute();
			vm.Execute();
			vm.Execute();
			Assert.AreEqual(0x212, vm.PC);
			Assert.AreEqual(0x7D, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.V3));

			vm.Execute(); // 0xA082
			vm.Execute(); // 0xF465
			Assert.AreEqual(0x216, vm.PC);
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x7D, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x7A, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.V3));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.V4));
		}
	}
}
