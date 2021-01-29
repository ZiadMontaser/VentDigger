using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000027 RID: 39
namespace VentDigger
{
	public class FloatRange
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x000058C2 File Offset: 0x00003AC2
		// (set) Token: 0x060000E6 RID: 230 RVA: 0x000058CA File Offset: 0x00003ACA
		public float Last { get; private set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000E7 RID: 231 RVA: 0x000058D3 File Offset: 0x00003AD3
		public float Width
		{
			get
			{
				return this.max - this.min;
			}
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x000058E2 File Offset: 0x00003AE2
		public FloatRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x000058F8 File Offset: 0x00003AF8
		public float ChangeRange(float y, float min, float max)
		{
			return Mathf.Lerp(min, max, (y - this.min) / this.Width);
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00005910 File Offset: 0x00003B10
		public float Clamp(float value)
		{
			return Mathf.Clamp(value, this.min, this.max);
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00005924 File Offset: 0x00003B24
		public bool Contains(float t)
		{
			return this.min <= t && this.max >= t;
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00005940 File Offset: 0x00003B40
		public float CubicLerp(float v)
		{
			if (this.min >= this.max)
			{
				return this.min;
			}
			v = Mathf.Clamp(0f, 1f, v);
			return v * v * v * (this.max - this.min) + this.min;
		}

		// Token: 0x060000ED RID: 237 RVA: 0x0000598E File Offset: 0x00003B8E
		public float EitherOr()
		{
			if (UnityEngine.Random.value <= 0.5f)
			{
				return this.max;
			}
			return this.min;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x000059A9 File Offset: 0x00003BA9
		public float LerpUnclamped(float v)
		{
			return Mathf.LerpUnclamped(this.min, this.max, v);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x000059BD File Offset: 0x00003BBD
		public float Lerp(float v)
		{
			return Mathf.Lerp(this.min, this.max, v);
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x000059D1 File Offset: 0x00003BD1
		public float ExpOutLerp(float v)
		{
			return this.Lerp(1f - Mathf.Pow(2f, -10f * v));
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x000059F0 File Offset: 0x00003BF0
		public static float ExpOutLerp(float v, float min, float max)
		{
			return Mathf.Lerp(min, max, 1f - Mathf.Pow(2f, -10f * v));
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00005A10 File Offset: 0x00003C10
		public static float Next(float min, float max)
		{
			return UnityEngine.Random.Range(min, max);
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00005A1C File Offset: 0x00003C1C
		public float Next()
		{
			return this.Last = UnityEngine.Random.Range(this.min, this.max);
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00005A43 File Offset: 0x00003C43
		public IEnumerable<float> Range(int numStops)
		{
			float num;
			for (float i = 0f; i <= (float)numStops; i = num)
			{
				yield return Mathf.Lerp(this.min, this.max, i / (float)numStops);
				num = i + 1f;
			}
			yield break;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00005A5A File Offset: 0x00003C5A
		public IEnumerable<float> RandomRange(int numStops)
		{
			float num;
			for (float i = 0f; i <= (float)numStops; i = num)
			{
				yield return this.Next();
				num = i + 1f;
			}
			yield break;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00005A71 File Offset: 0x00003C71
		internal float ReverseLerp(float t)
		{
			return Mathf.Clamp((t - this.min) / this.Width, 0f, 1f);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00005A94 File Offset: 0x00003C94
		public static float ReverseLerp(float t, float min, float max)
		{
			float num = max - min;
			return Mathf.Clamp((t - min) / num, 0f, 1f);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00005AB9 File Offset: 0x00003CB9
		public IEnumerable<float> SpreadToEdges(int stops)
		{
			return FloatRange.SpreadToEdges(this.min, this.max, stops);
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00005ACD File Offset: 0x00003CCD
		public IEnumerable<float> SpreadEvenly(int stops)
		{
			return FloatRange.SpreadEvenly(this.min, this.max, stops);
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00005AE1 File Offset: 0x00003CE1
		public static IEnumerable<float> SpreadToEdges(float min, float max, int stops)
		{
			if (stops == 1)
			{
				yield break;
			}
			int num;
			for (int i = 0; i < stops; i = num)
			{
				yield return Mathf.Lerp(min, max, (float)i / ((float)stops - 1f));
				num = i + 1;
			}
			yield break;
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00005AFF File Offset: 0x00003CFF
		public static IEnumerable<float> SpreadEvenly(float min, float max, int stops)
		{
			float step = 1f / ((float)stops + 1f);
			for (float i = step; i < 1f; i += step)
			{
				yield return Mathf.Lerp(min, max, i);
			}
			yield break;
		}

		// Token: 0x040000CC RID: 204
		public float min;

		// Token: 0x040000CD RID: 205
		public float max;
	}
}