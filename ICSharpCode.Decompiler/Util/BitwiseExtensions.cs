using System;
using System.Collections.Generic;

namespace ICSharpCode.Decompiler.Util
{
	/// <summary>
	/// Bitwise extension methods for internal use within the decompiler.
	/// </summary>
	static class BitwiseExtension
	{
		public static int BitCount(this int integer)
		{
			int bc = integer - ((integer >> 1) & 0x55555555);
			bc = (bc & 0x33333333) + ((bc >> 2) & 0x33333333);
			return (((bc + (bc >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
		}

		public static bool IsBitSet(this int integer, int position)
		{
			if (position == 31)
				return integer < 0;
			if (position >= 0 && position < 31)
				return (integer & (1 << position)) != 0;
			return false;
		}

		public static bool AnySet(this int integer, int mask = -1)
		{
			return (integer & mask) != 0;
		}

		public static bool AllSet(this int integer, int mask = -1)
		{
			return (integer & mask) == mask;
		}

		public static int Set(this int integer, int mask)
		{
			return integer | mask;
		}

		public static int Clear(this int integer, int mask)
		{
			return integer & ~mask;
		}

		public static IEnumerable<int> Bits(this int integer, bool ascending = true)
		{
			if (ascending)
				return integer.Bits(0, 32, 1);
			else
				return integer.Bits(31, -1, -1);
		}

		private static IEnumerable<int> Bits(this int integer, int start, int end, int step)
		{
			for (int position = start; position != end; position += step)
			{
				if (integer.IsBitSet(position))
					yield return position;
			}
		}
	}
}
