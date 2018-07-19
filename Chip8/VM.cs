using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
	public class VM
	{
		public enum RegisterName
		{
			V0 = 0, V1, V2, V3,
			V4, V5, V6, V7,
			V8, V9, VA, VB,
			VC, VD, VE, VF
		};

		public ushort PC { get; private set; }
		public byte SP { get; private set; }
		public ushort I { get; private set; }

		private byte[] mMemory = new byte[0xFFF];
		private ushort[] mStack = new ushort[0x00F];
		private byte[] mRegisters = new byte[0x10];

		//byte delayTimer;
		//byte soundTimer;

		private Opcode mOpcode;
		private Random mRandom = new Random();

		private void Initialize()
		{
			PC = 0x200;
			SP = 0x0;
			I = 0x0;
			Array.Clear(mMemory, 0, mMemory.Length);
			Array.Clear(mStack, 0, mStack.Length);
			Array.Clear(mRegisters, 0, mRegisters.Length);
			mOpcode.Update(0x0, 0x0);
			mRandom = new Random();
			//delayTimer = 0;
			//soundTimer = 0;
		}

		public void Seed(int seed) => mRandom = new Random(seed);

		public void Load(IEnumerable<Opcode> program)
		{
			// TODO: check length
			Initialize();
			int idx = 0x200;
			foreach (var op in program)
			{
				mMemory[idx] = op.mUpper;
				idx++;
				mMemory[idx] = op.mLower;
				idx++;
			}
		}

		public byte this[int i] => mMemory[i];

		public byte Register(RegisterName vx) => mRegisters[(int)vx];

		public void OneCycle()
		{
			// fetch opcode and execute
			mOpcode.Update(mMemory[PC], mMemory[PC + 1]);

			switch (mOpcode.Type)
			{
				case 0x0:
					if (mOpcode.Value == 0x00e0)
					{
						// 00e0 - clear display
					}
					else if (mOpcode.Value == 0x00ee)
					{
						// 00ee - return from subroutine
						SP--;
						PC = (ushort)(mStack[SP] + 2);
						mStack[SP] = 0x0;
					}
					break;
				case 0x1:
					// 1nnn - jump to location nnn
					PC = mOpcode.NNN;
					break;
				case 0x2:
					// 2nnn - call subroutine at nnn
					mStack[SP] = PC;
					PC = mOpcode.NNN;
					SP++;
					break;
				case 0x3:
					// 3xkk - skip next instruction if Vx == kk
					if (mRegisters[mOpcode.X] == mOpcode.mLower)
						PC += 4;
					else
						PC += 2;
					break;
				case 0x4:
					// 4xkk - skip next instruction if Vx != kk
					if (mRegisters[mOpcode.X] != mOpcode.mLower)
						PC += 4;
					else
						PC += 2;
					break;
				case 0x5:
					if (mOpcode.Nibble == 0)
					{
						// 5xy0 - skip next instruction if Vx == Vy
						if (mRegisters[mOpcode.X] == mRegisters[mOpcode.Y])
							PC += 4;
						else
							PC += 2;
					}
					break;
				case 0x6:
					// 6xkk - set Vx := kk
					mRegisters[mOpcode.X] = mOpcode.mLower;
					PC += 2;
					break;
				case 0x7:
					// 7xkk - set Vx := Vx + kk
					mRegisters[mOpcode.X] += mOpcode.mLower;
					PC += 2;
					break;
				case 0x8:
					switch (mOpcode.Nibble)
					{
						case 0x0:
							// 8xy0 - stores value of register Vy into Vx
							mRegisters[mOpcode.X] = mRegisters[mOpcode.Y];
							break;
						case 0x1:
							// 8xy1 - set Vx := Vx OR Vy.
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.X] | mRegisters[mOpcode.Y]);
							break;
						case 0x2:
							// 8xy2 - set Vx := Vx AND Vy.
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.X] & mRegisters[mOpcode.Y]);
							break;
						case 0x3:
							// 8xy3 - set Vx := Vx XOR Vy.
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.X] ^ mRegisters[mOpcode.Y]);
							break;
						case 0x4:
							// 8xy4 - set Vx := Vx + Vy, set VF = carry.
							int sum = mRegisters[mOpcode.X] + mRegisters[mOpcode.Y];
							mRegisters[0xF] = (byte)((sum > 255) ? 1 : 0);
							mRegisters[mOpcode.X] = (byte)(0xff & sum);
							break;
						case 0x5:
							// 8xy5 - set Vx := Vx - Vy, set VF = NOT borrow.
							mRegisters[0xF] = (byte)((mRegisters[mOpcode.X] > mRegisters[mOpcode.Y]) ? 1 : 0);
							mRegisters[mOpcode.X] -= mRegisters[mOpcode.Y];
							break;
						case 0x6:
							// 8xy6 - bit shift
							// If the least-significant bit of Vy is 1, then VF is set to 1, otherwise 0. Then Vy is divided by 2.
							mRegisters[0xF] = (byte)(0x1 & mRegisters[mOpcode.Y]);
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.Y] >> 1);
							break;
						case 0x7:
							// 8xy7 - set Vx := Vy - Vx, set VF = NOT borrow.
							mRegisters[0xF] = (byte)((mRegisters[mOpcode.Y] > mRegisters[mOpcode.X]) ? 1 : 0);
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.Y] - mRegisters[mOpcode.X]);
							break;
						case 0xE:
							// 8xyE - bit shift
							mRegisters[0xF] = (byte)(mRegisters[mOpcode.Y] >> 7);
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.Y] << 1);
							break;
					}
					PC += 2;
					break;
				case 0x9:
					if (mOpcode.Nibble == 0)
					{
						// 9xy0 - Skip next instruction if Vx != Vy.
						if (mRegisters[mOpcode.X] != mRegisters[mOpcode.Y])
							PC += 4;
						else
							PC += 2;
					}
					break;
				case 0xA:
					// Annn - set I := nnn
					I = mOpcode.NNN;
					PC += 2;
					break;
				case 0xB:
					// Bnnn - Jump to location nnn + V0.
					PC = (ushort)(mOpcode.NNN + mRegisters[0x0]);
					break;
				case 0xC:
					// Cxkk - set Vx := random byte AND kk
					mRegisters[mOpcode.X] = (byte)(mRandom.Next(0, 255) & mOpcode.mLower);
					PC += 2;
					break;
				case 0xD:
					// TODO - drawing
					break;
				case 0xE:
					// TODO - keyboard input
					break;
				case 0xF:
					// TODO
					break;
			}

			// TODO
			// -update timers
			// -PC, SP bounds check
			// -PC & 0x1 == 0
		}
	}
}
