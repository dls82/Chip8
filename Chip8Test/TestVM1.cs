using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Chip8;

namespace Chip8Test
{
	[TestClass]
	public class TestVM1
	{
		[TestMethod]
		public void LoadProgram()
		{
			// TODO: better program
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x1202),
				new Opcode(0x1202),
				new Opcode(0x1202),
				new Opcode(0x1202),
			};
			vm.Load(program);
			Assert.AreEqual(0x200, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);
			// TODO
			// -check registers zero
			// -zero before 0x200 and after end
			// -match program
		}

		// opcodes: 1nnn
		[TestMethod]
		public void Jump()
		{
			VM vm = new VM();
			var program = new List<Opcode> { new Opcode(0x1202) };
			vm.Load(program);

			vm.Execute();
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);
		}

		// opcodes: 2nnn, 00EE
		[TestMethod]
		public void Subroutine()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x2204),	// call subroutine at 0x204
				new Opcode(0x0000),	// skipped
				new Opcode(0x00EE), // return from subroutine
			};
			vm.Load(program);

			vm.Execute();
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x01, vm.SP);
			Assert.AreEqual(0x00, vm.I);

			vm.Execute();
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);
		}

		// opcodes: 6xkk, Annn
		[TestMethod]
		public void LoadRegister()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x6012), // set V0 := 0x12
				new Opcode(0x6B34), // set VB := 0x34
				new Opcode(0xA123), // set I:= 0x123
			};
			vm.Load(program);

			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.VB));
			Assert.AreEqual(0x0, vm.I);

			vm.Execute(); // 0x6012
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.VB));
			Assert.AreEqual(0x0, vm.I);

			vm.Execute(); // 0x6B32
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.VB));
			Assert.AreEqual(0x0, vm.I);

			vm.Execute(); // 0xA123
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x34, vm.Register(VM.RegisterName.VB));
			Assert.AreEqual(0x123, vm.I);
		}

		// opcodes: Cxkk
		[TestMethod]
		public void LoadRegisterRandom()
		{
			VM vm = new VM();
			var program = new List<Opcode> { new Opcode(0xC011), };
			vm.Load(program);
			vm.Seed(0); // Random.Next(0,255) = 185 = 0xB9
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));

			vm.Execute();
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0xB9 & 0x11, vm.Register(VM.RegisterName.V0));
		}

		// opcodes: 3xkk, 4xkk, 5xy0, 6xkk, 9xy0
		[TestMethod]
		public void SkipNext()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x6012), // set V0 := 0x12
				new Opcode(0x4012), // skip next if V0 != 0x12
				new Opcode(0x3012), // skip next if V0 == 0x12
				new Opcode(0x0000), // skipped
				new Opcode(0x5010), // skip next if V0 == V1
				new Opcode(0x9010), // skip next if V0 != V1
			};
			vm.Load(program);

			vm.Execute(); // 0x6012
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V0));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V1));

			vm.Execute(); // 0x4012
			Assert.AreEqual(0x204, vm.PC);

			vm.Execute(); // 0x3012
			Assert.AreEqual(0x208, vm.PC);

			vm.Execute(); // 0x5010
			Assert.AreEqual(0x20A, vm.PC);

			vm.Execute(); // 0x9010
			Assert.AreEqual(0x20E, vm.PC);
		}

		// opcodes: 6xkk, 7xkk, Bnnn
		[TestMethod]
		public void AddToRegister()
		{
			VM vm = new VM();
			var program = new List<Opcode> {
				new Opcode(0x6412), // set V4 := 0x12
				new Opcode(0x7411), // set V4 := V4 + 0x11
				new Opcode(0x6001), // set V0 := 0x01
				new Opcode(0xB444)}; // set PC:= V0 + 0x444
			vm.Load(program);

			vm.Execute(); // 0x6412
			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x12, vm.Register(VM.RegisterName.V4));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));

			vm.Execute(); // 0x7411
			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x12 + 0x11, vm.Register(VM.RegisterName.V4));
			Assert.AreEqual(0x0, vm.Register(VM.RegisterName.V0));

			vm.Execute(); // 0x6001
			Assert.AreEqual(0x206, vm.PC);
			Assert.AreEqual(0x12 + 0x11, vm.Register(VM.RegisterName.V4));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.V0));

			vm.Execute(); // 0xB444
			Assert.AreEqual(0x01 + 0x444, vm.PC);
			Assert.AreEqual(0x12 + 0x11, vm.Register(VM.RegisterName.V4));
			Assert.AreEqual(0x01, vm.Register(VM.RegisterName.V0));
		}
	}
}
