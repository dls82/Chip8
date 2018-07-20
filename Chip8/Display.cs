using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
	public class Display
	{
		public static readonly int WIDTH = 64;
		public static readonly int HEIGHT = 32;

		public static readonly byte[] SPRITE_0 = new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0 };
		public static readonly byte[] SPRITE_1 = new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 };
		public static readonly byte[] SPRITE_2 = new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 };
		public static readonly byte[] SPRITE_3 = new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 };
		public static readonly byte[] SPRITE_4 = new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 };
		public static readonly byte[] SPRITE_5 = new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 };
		public static readonly byte[] SPRITE_6 = new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 };
		public static readonly byte[] SPRITE_7 = new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 };
		public static readonly byte[] SPRITE_8 = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 };
		public static readonly byte[] SPRITE_9 = new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 };
		public static readonly byte[] SPRITE_A = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 };
		public static readonly byte[] SPRITE_B = new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 };
		public static readonly byte[] SPRITE_C = new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 };
		public static readonly byte[] SPRITE_D = new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 };
		public static readonly byte[] SPRITE_E = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 };
		public static readonly byte[] SPRITE_F = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 };

		bool[,] mPixels = new bool[WIDTH, HEIGHT];

		public bool this[int x, int y] => mPixels[x, y];

		// 00E0
		public void Clear() => Array.Clear(mPixels, 0, mPixels.Length);

		// Dxyn
		public bool Draw(byte xAnchor, byte yAnchor, byte[] sprite)
		{
			// TODO: sprite.Length <= 15
			bool anyErased = false;
			for (int y = 0; y < sprite.Length; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					// TODO: 0 <= x + xAnchor < WIDTH
					// TODO: 0 <= y + yAnchor < HEIGHT
					bool inBounds
						= (x + xAnchor < WIDTH) && (y + yAnchor < HEIGHT);
					if (inBounds)
					{
						bool pixelNew = (sprite[y] & (1 << (8 - x))) != 0;
						bool pixelOld = mPixels[x + xAnchor, y + yAnchor];
						mPixels[x + xAnchor, y + yAnchor] ^= pixelNew;
						if (pixelOld & pixelNew) anyErased = true;
					}
				}
			}
			return anyErased;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			for (int y = 0; y < HEIGHT; y++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					sb.Append(mPixels[x, y] ? "*" : " ");
				}
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}
	}
}
