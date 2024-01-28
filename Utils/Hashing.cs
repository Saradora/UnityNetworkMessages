using System.Runtime.CompilerServices;
using System.Text;

namespace UnityNetMessages;

// shamelessly copied from unity netcode which for some reason is internal?
public static class Hashing
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint Hash32(byte* input, int length, uint seed = 0)
    {
      uint num1 = seed + 374761393U;
      if (length >= 16)
      {
        uint num2 = (uint) ((int) seed - 1640531535 - 2048144777);
        uint num3 = seed + 2246822519U;
        uint num4 = seed;
        uint num5 = seed - 2654435761U;
        int num6 = length >> 4;
        for (int index = 0; index < num6; ++index)
        {
          uint num7 = *(uint*) input;
          uint num8 = *(uint*) (input + 4);
          uint num9 = *(uint*) (input + 8);
          uint num10 = *(uint*) (input + 12);
          uint num11 = num2 + num7 * 2246822519U;
          num2 = (num11 << 13 | num11 >> 19) * 2654435761U;
          uint num12 = num3 + num8 * 2246822519U;
          num3 = (num12 << 13 | num12 >> 19) * 2654435761U;
          uint num13 = num4 + num9 * 2246822519U;
          num4 = (num13 << 13 | num13 >> 19) * 2654435761U;
          uint num14 = num5 + num10 * 2246822519U;
          num5 = (num14 << 13 | num14 >> 19) * 2654435761U;
          input += 16;
        }
        num1 = (uint) (((int) num2 << 1 | (int) (num2 >> 31)) + ((int) num3 << 7 | (int) (num3 >> 25)) + ((int) num4 << 12 | (int) (num4 >> 20)) + ((int) num5 << 18 | (int) (num5 >> 14)));
      }
      uint num15 = num1 + (uint) length;
      for (length &= 15; length >= 4; length -= 4)
      {
        uint num16 = num15 + *(uint*) input * 3266489917U;
        num15 = (uint) (((int) num16 << 17 | (int) (num16 >> 15)) * 668265263);
        input += 4;
      }
      for (; length > 0; --length)
      {
        uint num17 = num15 + *input * 374761393U;
        num15 = (uint) (((int) num17 << 11 | (int) (num17 >> 21)) * -1640531535);
        ++input;
      }
      uint num18 = (num15 ^ num15 >> 15) * 2246822519U;
      uint num19 = (num18 ^ num18 >> 13) * 3266489917U;
      return num19 ^ num19 >> 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint Hash32(this byte[] buffer)
    {
      int length = buffer.Length;
      fixed (byte* input = buffer)
        return Hash32(input, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(this string text) => Encoding.UTF8.GetBytes(text).Hash32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(this Type type) => type.FullName.Hash32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32<T>() => typeof (T).Hash32();
}