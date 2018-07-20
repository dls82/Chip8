using System;
using System.Collections.Generic;

namespace Chip8
{
	public class VM
	{
		public static readonly int MEMORY_SIZE_BYTES = 0xFFF;
		public static readonly int STACK_LENGTH = 0x0F;
		public static readonly int REGISTERS_LENGTH = 0x10;

		public enum RegisterName
		{
			V0 = 0, V1, V2, V3,
			V4, V5, V6, V7,
			V8, V9, VA, VB,
			VC, VD, VE, VF
		};

		public ushort PC { get; private set; }  // program counter
		public byte SP { get; private set; }    // stack pointer
		public ushort I { get; private set; }   // void pointer

		private byte[] mMemory = new byte[MEMORY_SIZE_BYTES];
		private ushort[] mStack = new ushort[STACK_LENGTH];
		private byte[] mRegisters = new byte[REGISTERS_LENGTH];

		public byte DelayTimer { get; private set; }
		public byte SoundTimer { get; private set; }

		private Display mDisplay = new Display();

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
			DelayTimer = 0;
			SoundTimer = 0;
			mDisplay.Clear();
			mOpcode.Update(0x0, 0x0);
			mRandom = new Random();

			// load 5-byte font sprites
			// must be consistent with opcode Fx29
			Array.Copy(Display.SPRITE_0, 0, mMemory, 0, 5);
			Array.Copy(Display.SPRITE_1, 0, mMemory, 5, 5);
			Array.Copy(Display.SPRITE_2, 0, mMemory, 10, 5);
			Array.Copy(Display.SPRITE_3, 0, mMemory, 15, 5);
			Array.Copy(Display.SPRITE_4, 0, mMemory, 20, 5);
			Array.Copy(Display.SPRITE_5, 0, mMemory, 25, 5);
			Array.Copy(Display.SPRITE_6, 0, mMemory, 30, 5);
			Array.Copy(Display.SPRITE_7, 0, mMemory, 35, 5);
			Array.Copy(Display.SPRITE_8, 0, mMemory, 40, 5);
			Array.Copy(Display.SPRITE_9, 0, mMemory, 45, 5);
			Array.Copy(Display.SPRITE_A, 0, mMemory, 50, 5);
			Array.Copy(Display.SPRITE_B, 0, mMemory, 55, 5);
			Array.Copy(Display.SPRITE_C, 0, mMemory, 60, 5);
			Array.Copy(Display.SPRITE_D, 0, mMemory, 65, 5);
			Array.Copy(Display.SPRITE_E, 0, mMemory, 70, 5);
			Array.Copy(Display.SPRITE_F, 0, mMemory, 75, 5);
		}

		public void Seed(int seed) => mRandom = new Random(seed);

		public void Load(IEnumerable<Opcode> program)
		{
			// TODO: check length
			Initialize();
			int idx = 0x200;
			foreach (var op in program)
			{
				mMemory[idx++] = op.mUpper;
				mMemory[idx++] = op.mLower;
			}
		}

		public byte this[int i] => mMemory[i];

		public byte Register(RegisterName vx) => mRegisters[(int)vx];

		private void BoundsCheck()
		{
			// TODO:
			// - 0 <= I < MEMORY_SIZE_BYTES
			// -PC, SP, bounds check
			// -PC & 0x1 == 0
		}

		public void Execute()
		{
			// fetch opcode and execute
			mOpcode.Update(mMemory[PC], mMemory[PC + 1]);
			switch (mOpcode.Type)
			{
				case 0x0:
					if (mOpcode.Value == 0x00E0)
					{
						// 00E0 - clear display
						mDisplay.Clear();
						PC += 2;
						BoundsCheck();
					}
					else if (mOpcode.Value == 0x00EE)
					{
						// 00EE - return from subroutine
						SP--;
						PC = (ushort)(mStack[SP] + 2);
						BoundsCheck();
						mStack[SP] = 0x0;
					}
					else
					{
						// throw
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
					BoundsCheck();
					break;
				case 0x3:
					// 3xkk - skip next instruction if Vx == kk
					if (mRegisters[mOpcode.X] == mOpcode.mLower)
						PC += 4;
					else
						PC += 2;
					BoundsCheck();
					break;
				case 0x4:
					// 4xkk - skip next instruction if Vx != kk
					if (mRegisters[mOpcode.X] != mOpcode.mLower)
						PC += 4;
					else
						PC += 2;
					BoundsCheck();
					break;
				case 0x5:
					if (mOpcode.Nibble == 0)
					{
						// 5xy0 - skip next instruction if Vx == Vy
						if (mRegisters[mOpcode.X] == mRegisters[mOpcode.Y])
							PC += 4;
						else
							PC += 2;
						BoundsCheck();
					}
					else
					{
						// throw
					}
					break;
				case 0x6:
					// 6xkk - set Vx := kk
					mRegisters[mOpcode.X] = mOpcode.mLower;
					PC += 2;
					BoundsCheck();
					break;
				case 0x7:
					// 7xkk - set Vx := Vx + kk
					mRegisters[mOpcode.X] += mOpcode.mLower;
					PC += 2;
					BoundsCheck();
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
							// 8xy6 - bit shift right
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
							// 8xyE - bit shift left
							mRegisters[0xF] = (byte)(mRegisters[mOpcode.Y] >> 7);
							mRegisters[mOpcode.X] = (byte)(mRegisters[mOpcode.Y] << 1);
							break;
						default:
							// throw
							break;
					}
					PC += 2;
					BoundsCheck();
					break;
				case 0x9:
					if (mOpcode.Nibble == 0)
					{
						// 9xy0 - Skip next instruction if Vx != Vy.
						if (mRegisters[mOpcode.X] != mRegisters[mOpcode.Y])
							PC += 4;
						else
							PC += 2;
						BoundsCheck();
					}
					else
					{
						// throw
					}
					break;
				case 0xA:
					// Annn - set I := nnn
					I = mOpcode.NNN;
					PC += 2;
					BoundsCheck();
					break;
				case 0xB:
					// Bnnn - Jump to location nnn + V0.
					PC = (ushort)(mOpcode.NNN + mRegisters[0x0]);
					BoundsCheck();
					break;
				case 0xC:
					// Cxkk - set Vx := random byte AND kk
					mRegisters[mOpcode.X] = (byte)(mRandom.Next(0, 255) & mOpcode.mLower);
					PC += 2;
					BoundsCheck();
					break;
				case 0xD:
					// Dxyn - display n-byte sprite at memory location I at (Vx, Vy), set Vf = collision
					// TODO: I + n < MEMORY_SIZE_BYTES
					byte[] sprite = new byte[mOpcode.Nibble];
					Array.Copy(mMemory, sprite, sprite.Length);
					mDisplay.Draw(mOpcode.X, mOpcode.Y, sprite);
					PC += 2;
					BoundsCheck();
					break;
				case 0xE:
					// TODO - keyboard input
					break;
				case 0xF:
					switch (mOpcode.mLower)
					{
						case 0x07:
							// Fx07 - set Vx := DT
							mRegisters[mOpcode.X] = DelayTimer;
							break;
						case 0x0A:
							// Fx0A - pause for key press, store in Vx
							// TODO
							break;
						case 0x15:
							// Fx15 - set DT := Vx
							DelayTimer = mRegisters[mOpcode.X];
							break;
						case 0x18:
							// Fx18 - set ST := Vx
							SoundTimer = mRegisters[mOpcode.X];
							break;
						case 0x1E:
							// Fx1E - set I := I + Vx
							I += mRegisters[mOpcode.X];
							BoundsCheck();
							break;
						case 0x29:
							// Fx29 - set I := location of hexidecimal sprit for Vx
							I = (ushort)(mRegisters[mOpcode.X] * 5);
							break;
						case 0x33:
							// Fx33 - place binary digits of Vx in I, I+1, I+2
							// TODO: throw if not I + 2 < MEMORY_SIZE_BYTES
							int tmp = mRegisters[mOpcode.X];
							mMemory[I + 2] = (byte)(tmp % 10);
							tmp = (tmp - mMemory[I + 2]) / 10;
							mMemory[I + 1] = (byte)(tmp % 10);
							tmp = (tmp - mMemory[I + 1]) / 10;
							mMemory[I] = (byte)(tmp % 10);
							break;
						case 0x55:
							// Fx55 - store registers V0 through Vx in memory starting at I
							// TODO: I + x + 1 < MEMORY_SIZE_BYTES
							Array.Copy(mRegisters, 0, mMemory, I, 1 + mOpcode.X);
							break;
						case 0x65:
							// Fx65 - load registers V0 through Vx from memory starting at I
							// TODO: I + x + 1 < MEMORY_SIZE_BYTES
							Array.Copy(mMemory, I, mRegisters, 0, 1 + mOpcode.X);
							break;
						default:
							// throw
							break;
					}
					PC += 2;
					BoundsCheck();
					break;
			}

			// TODO: update timers
		}
	}
}
