namespace UnityNetMessages;

public static class Extensions
{
    public static int SizeOf(this byte[] bytes) => sizeof(byte) * bytes.Length;
}