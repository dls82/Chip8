﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
	public struct Opcode
	{
		public byte mUpper;
		public byte mLower;

		public Opcode(ushort op)
		{
			mUpper = (byte)(op >> 8);
			mLower = (byte)(op & 0x0ff);
		}

		public void Update(byte upper, byte lower)
		{
			mUpper = upper;
			mLower = lower;
		}

		public byte Type => (byte)(mUpper >> 4);
		public byte Nibble => (byte)(mLower & 0x0f);
		public byte X => (byte)(mUpper & 0x0f);
		public byte Y => (byte)(mLower >> 4);
		public ushort NNN => (ushort)((X << 8) + mLower);
		public ushort Value => (ushort)((mUpper << 8) + mLower);
		public override string ToString() => String.Format("0x{0:X4}", Value);
	}
}
