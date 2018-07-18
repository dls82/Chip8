using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
	public class VM
	{
		public ushort PC { get; private set; }
		public byte[] memory = new byte[0xfff];

		public byte SP { get; private set; }
		ushort[] stack = new ushort[0x00f];

		byte[] registers = new byte[0x010];

		public ushort I { get; private set; }

		private Opcode opcode;
		private Random random = new Random();
		
		//byte delayTimer;
		//byte soundTimer;

		private void Initialize()
		{
			PC = 0x200;
			SP = 0x0;
			I = 0x0;
			opcode.Update(0x0, 0x0);
			Array.Clear(memory, 0, memory.Length);
			Array.Clear(stack, 0, stack.Length);
			Array.Clear(registers, 0, registers.Length);
			//delayTimer = 0;
			//soundTimer = 0;
		}

		public void Load(IEnumerable<Opcode> program)
		{
			if (program.Count() > 0xfff)
				throw new InvalidOperationException();
			
			Initialize();
			int idx = 0x200;
			foreach (var op in program)
			{
				memory[idx] = op.mUpper;
				idx++;
				memory[idx] = op.mLower;
				idx++;
			}
		}

		public byte this[int i]
		{
			get => memory[i];
		}

		public byte Register(int idx) => registers[idx];

		public void OneCycle()
		{
			// fetch opcode
			opcode.Update(memory[PC], memory[PC + 1]);

			// execute
			switch (opcode.Type)
			{
				case 0x0:
					if (opcode.Value == 0x00e0)
					{
						// 00e0 - clear display
					}
					else if (opcode.Value == 0x00ee)
					{
						// 00xx - return from subroutine
						SP--;
						PC = (ushort)(stack[SP] + 2);
						stack[SP] = 0x0;
					}
					break;
				case 0x1:
					// 1nnn - jump to location nnn
					PC = opcode.NNN;
					break;
				case 0x2:
					// 2nnn - call subroutine at nnn
					stack[SP] = PC;
					PC = opcode.NNN;
					SP++;
					break;
				case 0x3:
					// 3xkk - skip next instruction if Vx == kk
					if (registers[opcode.X] == opcode.mLower)
						PC += 4;
					else
						PC += 2;
					break;
				case 0x4:
					// 4xkk - skip next instruction if Vx != kk
					if (registers[opcode.X] != opcode.mLower)
						PC += 4;
					else
						PC += 2;
					break;
				case 0x5:
					// 5xy0 - skip next instruction if Vx == Vy
					if (registers[opcode.X] != registers[opcode.Y])
						PC += 4;
					else
						PC += 2;
					break;
				case 0x6:
					// 6xkk - set Vx := kk
					registers[opcode.X] = opcode.mLower;
					PC += 2;
					break;
				case 0x7:
					// 7xkk - set Vx := Vx + kk
					registers[opcode.X] += opcode.mLower;
					PC += 2;
					break;
				case 0x8:
					switch (opcode.Nibble)
					{
						case 0x0:
							// 8xy0 - stores value of register Vy into Vx
							registers[opcode.X] = registers[opcode.Y];
							break;
						case 0x1:
							// 8xy1 - set Vx := Vx OR Vy.
							registers[opcode.X] = (byte)(registers[opcode.X] | registers[opcode.Y]);
							break;
						case 0x2:
							// 8xy2 - set Vx := Vx AND Vy.
							registers[opcode.X] = (byte)(registers[opcode.X] & registers[opcode.Y]);
							break;
						case 0x3:
							// 8xy3 - set Vx := Vx XOR Vy.
							registers[opcode.X] = (byte)(registers[opcode.X] ^ registers[opcode.Y]);
							break;
						case 0x4:
							// 8xy4 - set Vx := Vx + Vy, set VF = carry.
							int sum = registers[opcode.X] + registers[opcode.Y];
							registers[0xf] = (byte)((sum > 255) ? 1 : 0);
							registers[opcode.X] = (byte)(0xff & sum);
							break;
						case 0x5:
							// 8xy5 - set Vx := Vx - Vy, set VF = NOT borrow.
							registers[0xf] = (byte)((registers[opcode.X] > registers[opcode.Y]) ? 1 : 0);
							registers[opcode.X] -= registers[opcode.Y];
							break;
						case 0x6:
							// 8xy6 - bit shift
							// If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
							registers[0xf] = (byte)(0x1 & registers[opcode.X]);
							registers[opcode.X] = (byte)(registers[opcode.X] >> 1);
							break;
						case 0x7:
							// 8xy7 - set Vx := Vy - Vx, set VF = NOT borrow.
							registers[0xf] = (byte)((registers[opcode.Y] > registers[opcode.X]) ? 1 : 0);
							registers[opcode.X] = (byte)(registers[opcode.Y] - registers[opcode.X]);
							break;
						case 0xe:
							// 8xye - bit shift
							registers[0xf] = (byte)(0x80 & registers[opcode.X]);
							registers[opcode.X] = (byte)(registers[opcode.X] << 1);
							break;
					}
					PC += 2;
					break;
				case 0x9:
					if (opcode.Nibble == 0)
					{
						// 9xy0 - Skip next instruction if Vx != Vy.
						if (registers[opcode.X] != registers[opcode.Y])
							PC += 4;
						else
							PC += 2;
					}
					break;
				case 0xa:
					// annn - set I := nnn
					I = opcode.NNN;
					PC += 2;
					break;
				case 0xb:
					// bnnn - Jump to location nnn + V0.
					PC = (ushort)(opcode.NNN + registers[0x0]);
					break;
				case 0xc:
					// cxkk - set Vx := random byte AND kk
					registers[opcode.X] = (byte)(random.Next(0, 255) & opcode.mLower);
					PC += 2;
					break;
				case 0xd:
					// TODO - drawing
					break;
				case 0xe:
					// TODO - keyboard input
					break;
				case 0xf:
					// TODO
					break;
			}

			// TODO
			// -update timers
			// -PC, SP bounds check
		}
	}
}
