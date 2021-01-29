using Hazel;
using UnityEngine;
namespace VentDigger
{
    static class MessageWriterHelper
    {
		// Token: 0x040003E3 RID: 995
		private static readonly FloatRange XRange = new FloatRange(-40f, 40f);

		// Token: 0x040003E4 RID: 996
		private static readonly FloatRange YRange = new FloatRange(-40f, 40f);

		static public void WriteVector2(this MessageWriter writer, Vector2 vec)
		{
			ushort value = (ushort)(XRange.ReverseLerp(vec.x) * 65535f);
			ushort value2 = (ushort)(YRange.ReverseLerp(vec.y) * 65535f);
			writer.Write(value);
			writer.Write(value2);
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00018BDC File Offset: 0x00016DDC
		static public Vector2 ReadVector2(this MessageReader reader)
		{
			float v = (float)reader.ReadUInt16() / 65535f;
			float v2 = (float)reader.ReadUInt16() / 65535f;
			return new Vector2(XRange.Lerp(v), YRange.Lerp(v2));
		}
	}
}
