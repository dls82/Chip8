using System;
using System.Collections.Generic;
using Chip8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8Test
{
	[TestClass]
	public class TestVM2
	{
		// opcodes: 8xy0, 8xy1, 8xy2, 8xy3
		[TestMethod]
		public void Test8000Logical()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x6112), // set V1 := 0x12
				new Opcode(0x6234), // set V2 := 0x34
				new Opcode(0x8010), // set V0 := V1
				new Opcode(0x8121), // set V1 := V1 | V2
				new Opcode(0x8022), // set V0 := V0 & V2

				// swap V0 and V1 using XOR
				new Opcode(0x8013), // set V0 := V0 ^ V1
				new Opcode(0x8103), // set V1 := V1 ^ V0
				new Opcode(0x8013), // set V0 := V0 ^ V1
			};
			vm.Load(program);

			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x6112
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x6234
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x8010
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x8121
			Assert.AreEqual(0x208, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12 | 0x34, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x8022
			Assert.AreEqual(0x20A, vm.PC);
			Assert.AreEqual(0x12 & 0x34, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12 | 0x34, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));

			// swap registers V0 and V1
			vm.Execute();
			vm.Execute();
			vm.Execute();
			Assert.AreEqual(0x210, vm.PC);
			Assert.AreEqual(0x12 | 0x34, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12 & 0x34, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));
		}

		// opcodes: 8xy4, 8xy5, 8xy7
		[TestMethod]
		public void Test8000Arithmetic()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x60FA), // set V0 := 0xFA (= 250 decimal)
				new Opcode(0x6112), // set V1 := 0x12
				new Opcode(0x6234), // set V2 := 0x34
				new Opcode(0x8014), // set V0 := V0 + V1 (carry)
				new Opcode(0x8124), // set V1 := V1 + V2 (no carry)
				new Opcode(0x8015), // set V0 := V0 - V1 (no carry)
				new Opcode(0x8125), // set V2 := V2 - V1 (carry)

				new Opcode(0x6501), // set V5 := 0x01
				new Opcode(0x6603), // set V6 := 0x03
				new Opcode(0x6702), // set V7 := 0x02
				new Opcode(0x8567), // set V5 := V6 - V5 (carry)
				new Opcode(0x8677), // set V6 := V7 - V6 (no carry)
			};
			vm.Load(program);

			vm.Execute(); // 0x60FA
			vm.Execute(); // 0x6112
			vm.Execute(); // 0x6234
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0xFA, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x8014
			Assert.AreEqual(0x208, vm.PC);
			Assert.AreEqual((0xFA + 0x12) & 0xFF, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0x8124
			Assert.AreEqual(0x20A, vm.PC);
			Assert.AreEqual((0xFA + 0x12) & 0xFF, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual((0x12 + 0x34) & 0xFF, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0x8015
			Assert.AreEqual(0x20C, vm.PC);
			Assert.AreEqual((((0xFA + 0x12) - ((0x12 + 0x34) & 0xFF)) & 0xFF) & 0xFF, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual((0x12 + 0x34) & 0xFF, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0x8125
			Assert.AreEqual(0x20E, vm.PC);
			Assert.AreEqual((((0xFA + 0x12) - ((0x12 + 0x34) & 0xFF)) & 0xFF) & 0xFF, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual((((0x12 + 0x34) & 0xFF) - 0x34) & 0xFF, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0x6501
			vm.Execute(); // 0x6603
			vm.Execute(); // 0x6702
			Assert.AreEqual(0x214, vm.PC);
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.V5));
			Assert.AreEqual(0x03, vm.Register(VM.RegisterName.V6));
			Assert.AreEqual(0x02, vm.Register(VM.RegisterName.V7));

			vm.Execute(); // 0x8567
			Assert.AreEqual(0x216, vm.PC);
			Assert.AreEqual(0x02, vm.Register(VM.RegisterName.V5));
			Assert.AreEqual(0x03, vm.Register(VM.RegisterName.V6));
			Assert.AreEqual(0x02, vm.Register(VM.RegisterName.V7));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0x8677
			Assert.AreEqual(0x218, vm.PC);
			Assert.AreEqual(0x02, vm.Register(VM.RegisterName.V5));
			Assert.AreEqual((0x02 - 0x03) & 0xFF, vm.Register(VM.RegisterName.V6));
			Assert.AreEqual(0x02, vm.Register(VM.RegisterName.V7));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.VF));
		}

		// opcodes: 8xy6, 8xyE
		[TestMethod]
		public void Test8000Shift()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x6113), // set V1 := 0x13 (0b00010011)
				new Opcode(0x62F4), // set V2 := 0xF4 (0b11110100)
				new Opcode(0x8516), // set V5 := SHR V1
				new Opcode(0x8626), // set V6 := SHR V2
				new Opcode(0x871E), // set V7 := SHL V1
				new Opcode(0x882E), // set V8 := SHL V2
			};
			vm.Load(program);
			vm.Execute();
			vm.Execute();
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x13, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0xF4, vm.Register(VM.RegisterName.V2));

			vm.Execute(); // 0x8516 - SHR 0b00010011
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x13, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0xF4, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x13 >> 1, vm.Register(VM.RegisterName.V5));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.VF));



			vm.Execute(); // 0x8626 - SHR 0b11110100
			Assert.AreEqual(0x208, vm.PC);
			Assert.AreEqual(0x13, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0xF4, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0xF4 >> 1, vm.Register(VM.RegisterName.V6));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.VF));


			vm.Execute(); // 0x871E - SHL 0b00010011
			Assert.AreEqual(0x20A, vm.PC);
			Assert.AreEqual(0x13, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0xF4, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual(0x13 << 1, vm.Register(VM.RegisterName.V7));
			Assert.AreEqual(0x00, vm.Register(VM.RegisterName.VF));

			vm.Execute(); // 0x882E - - SHL 0b11110100
			Assert.AreEqual(0x20C, vm.PC);
			Assert.AreEqual(0x13, vm.Register(VM.RegisterName.V1));
			Assert.AreEqual(0xF4, vm.Register(VM.RegisterName.V2));
			Assert.AreEqual((0xF4 << 1) & 0xFF, vm.Register(VM.RegisterName.V8));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.VF));
		}
	}
}
