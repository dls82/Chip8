using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Chip8Test
{
	[TestClass]
	public class TestVM
	{
		

		[TestMethod]
		public void LoadProgram()
		{
			Chip8.VM vm = new Chip8.VM();
			var program = new List<Chip8.Opcode> {
							new Chip8.Opcode(0x1202),	// 0x200
							new Chip8.Opcode(0x1202),	// 0x202
							new Chip8.Opcode(0x1202),	// 0x204
							new Chip8.Opcode(0x1202)};  // 0x206
			vm.Load(program);
			// TODO
			// -zero before 0x200 and after end
			// -match program
		}

		[TestMethod]
		public void Jump()
		{
			Chip8.VM vm = new Chip8.VM();
			var program = new List<Chip8.Opcode> { new Chip8.Opcode(0x1202) };
			vm.Load(program);

			Assert.AreEqual(0x200, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);

			vm.OneCycle();

			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);
		}

		[TestMethod]
		public void Subroutine()
		{
			Chip8.VM vm = new Chip8.VM();
			var program = new List<Chip8.Opcode> {
							new Chip8.Opcode(0x2204),	// 0x200, 0x201
							new Chip8.Opcode(0x000),	// 0x202, 0x203
							new Chip8.Opcode(0x00ee) }; // 0x204, 0x205
			vm.Load(program);

			Assert.AreEqual(0x200, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);

			vm.OneCycle();

			Assert.AreEqual(0x204, vm.PC);
			Assert.AreEqual(0x01, vm.SP);
			Assert.AreEqual(0x00, vm.I);

			vm.OneCycle();

			Assert.AreEqual(0x202, vm.PC);
			Assert.AreEqual(0x00, vm.SP);
			Assert.AreEqual(0x00, vm.I);
		}


		[TestMethod]
		public void LoadRegister()
		{
			Chip8.VM vm = new Chip8.VM();
			var program = new List<Chip8.Opcode> {
							new Chip8.Opcode(0x6012),
							new Chip8.Opcode(0x6b34)};
			vm.Load(program);

			Assert.AreEqual(0x200, vm.PC);
			// TODO
		}
	}
}
