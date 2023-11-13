using System;
using System.Collections.Generic;
using System.Linq;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace PECSScriptPlugin
{
	internal class Ifunc
	{
		public static void CalcAverage_NonOverlapable(HashSet<CVSet> keys, Dictionary<CVSet, CVDat> dic, CVDat Out)
		{
			Out.m_v.UV = new V2(0f, 0f);
			Out.m_v.UVA1 = new V4(0f, 0f, 0f, 0f);
			Out.m_v.UVA2 = new V4(0f, 0f, 0f, 0f);
			Out.m_v.UVA3 = new V4(0f, 0f, 0f, 0f);
			Out.m_v.UVA4 = new V4(0f, 0f, 0f, 0f);
			Out.m_v.EdgeScale = 0f;
			Out.m_UVMO.Clear();
			int count = keys.Count;
			Dictionary<IPXMorph, V4> dictionary = new Dictionary<IPXMorph, V4>();
			foreach (CVSet key in keys)
			{
				Out.m_v.UV += dic[key].m_v.UV;
				Out.m_v.UVA1 += dic[key].m_v.UVA1;
				Out.m_v.UVA2 += dic[key].m_v.UVA2;
				Out.m_v.UVA3 += dic[key].m_v.UVA3;
				Out.m_v.UVA4 += dic[key].m_v.UVA4;
				Out.m_v.EdgeScale += dic[key].m_v.EdgeScale;
				foreach (IPXMorph key2 in dic[key].m_UVMO.Keys)
				{
					if (dictionary.ContainsKey(key2))
					{
						dictionary[key2] += dic[key].m_UVMO[key2];
					}
					else
					{
						dictionary.Add(key2, dic[key].m_UVMO[key2]);
					}
				}
			}
			Out.m_v.UV /= (float)count;
			Out.m_v.UVA1 /= (float)count;
			Out.m_v.UVA2 /= (float)count;
			Out.m_v.UVA3 /= (float)count;
			Out.m_v.UVA4 /= (float)count;
			Out.m_v.EdgeScale /= count;
			foreach (IPXMorph key3 in dictionary.Keys)
			{
				Out.m_UVMO.Add(key3, dictionary[key3] / count);
			}
		}

		public static void CalcAverage_Overlapable(HashSet<CVSet> keys, Dictionary<CVSet, CVDat> dic, CVDat Out)
		{
			Out.m_v.Position = new V3(0f, 0f, 0f);
			Out.m_v.Normal = new V3(0f, 0f, 0f);
			Out.m_VMO.Clear();
			Out.m_v.QDEF = false;
			Out.m_v.SDEF = false;
			Out.m_v.SDEF_C = new V3(0f, 0f, 0f);
			Out.m_v.SDEF_R0 = new V3(0f, 0f, 0f);
			Out.m_v.SDEF_R1 = new V3(0f, 0f, 0f);
			int count = keys.Count;
			Dictionary<IPXBone, float> dictionary = new Dictionary<IPXBone, float>();
			Dictionary<IPXMorph, V3> dictionary2 = new Dictionary<IPXMorph, V3>();
			foreach (CVSet key in keys)
			{
				Out.m_v.Position += dic[key].m_v.Position;
				Out.m_v.Normal += dic[key].m_v.Normal;
				if (dictionary.ContainsKey(dic[key].m_v.Bone1))
				{
					dictionary[dic[key].m_v.Bone1] += dic[key].m_v.Weight1;
				}
				else
				{
					dictionary.Add(dic[key].m_v.Bone1, dic[key].m_v.Weight1);
				}
				if (dic[key].m_v.Bone2 != null)
				{
					if (dictionary.ContainsKey(dic[key].m_v.Bone2))
					{
						dictionary[dic[key].m_v.Bone2] += dic[key].m_v.Weight2;
					}
					else
					{
						dictionary.Add(dic[key].m_v.Bone2, dic[key].m_v.Weight2);
					}
					if (dic[key].m_v.Bone3 != null)
					{
						if (dictionary.ContainsKey(dic[key].m_v.Bone3))
						{
							dictionary[dic[key].m_v.Bone3] += dic[key].m_v.Weight3;
						}
						else
						{
							dictionary.Add(dic[key].m_v.Bone3, dic[key].m_v.Weight3);
						}
						if (dic[key].m_v.Bone4 != null)
						{
							if (dictionary.ContainsKey(dic[key].m_v.Bone4))
							{
								dictionary[dic[key].m_v.Bone4] += dic[key].m_v.Weight4;
							}
							else
							{
								dictionary.Add(dic[key].m_v.Bone4, dic[key].m_v.Weight4);
							}
						}
					}
				}
				foreach (IPXMorph key2 in dic[key].m_VMO.Keys)
				{
					if (dictionary2.ContainsKey(key2))
					{
						dictionary2[key2] += dic[key].m_VMO[key2];
					}
					else
					{
						dictionary2.Add(key2, dic[key].m_VMO[key2]);
					}
				}
			}
			Out.m_v.Position /= (float)count;
			WrapNormalize(Out.m_v.Normal);
			int count2 = dictionary.Count;
			int num = Math.Min(dictionary.Count, 4);
			IPXBone[] array = dictionary.Keys.ToArray();
			float[] array2 = dictionary.Values.ToArray();
			Array.Sort(array2, array);
			float num2 = 0f;
			for (int i = 1; i <= num; i++)
			{
				num2 += array2[count2 - i];
			}
			for (int j = 1; j <= num; j++)
			{
				array2[count2 - j] /= num2;
			}
			Out.m_v.Bone1 = array[count2 - 1];
			Out.m_v.Weight1 = array2[count2 - 1];
			switch (num)
			{
			case 1:
				Out.m_v.Bone2 = null;
				Out.m_v.Weight2 = 0f;
				Out.m_v.Bone3 = null;
				Out.m_v.Weight3 = 0f;
				Out.m_v.Bone4 = null;
				Out.m_v.Weight4 = 0f;
				break;
			case 2:
				Out.m_v.Bone2 = array[count2 - 2];
				Out.m_v.Weight2 = array2[count2 - 2];
				Out.m_v.Bone3 = null;
				Out.m_v.Weight3 = 0f;
				Out.m_v.Bone4 = null;
				Out.m_v.Weight4 = 0f;
				break;
			case 3:
				Out.m_v.Bone2 = array[count2 - 2];
				Out.m_v.Weight2 = array2[count2 - 2];
				Out.m_v.Bone3 = array[count2 - 3];
				Out.m_v.Weight3 = array2[count2 - 3];
				Out.m_v.Bone4 = null;
				Out.m_v.Weight4 = 0f;
				break;
			default:
				Out.m_v.Bone2 = array[count2 - 2];
				Out.m_v.Weight2 = array2[count2 - 2];
				Out.m_v.Bone3 = array[count2 - 3];
				Out.m_v.Weight3 = array2[count2 - 3];
				Out.m_v.Bone4 = array[count2 - 4];
				Out.m_v.Weight4 = array2[count2 - 4];
				break;
			}
			foreach (IPXMorph key3 in dictionary2.Keys)
			{
				Out.m_VMO.Add(key3, dictionary2[key3] / count);
			}
		}

		public static void WrapNormalize(V3 In)
		{
			if (In.Length() > 0f)
			{
				In.Normalize();
			}
			else
			{
				In = new V3(0f, 1f, 0f);
			}
		}

		public static void CalcCatmullClarkMethod(CVDat OV, CVDat MPA, CVDat FPA, int Num, CVDat Out, string target, IPXPmxBuilder bdx)
		{
			HashSet<CVSet> hashSet = new HashSet<CVSet>();
			Dictionary<CVSet, CVDat> dictionary = new Dictionary<CVSet, CVDat>();
			CVSet cVSet = new CVSet(bdx.Vertex());
			hashSet.Add(cVSet);
			dictionary.Add(cVSet, FPA);
			CVSet cVSet2 = new CVSet(bdx.Vertex());
			hashSet.Add(cVSet2);
			dictionary.Add(cVSet2, MPA);
			CVSet cVSet3 = new CVSet(bdx.Vertex());
			hashSet.Add(cVSet3);
			dictionary.Add(cVSet3, MPA);
			for (int i = 0; i < Num - 3; i++)
			{
				CVSet cVSet4 = new CVSet(bdx.Vertex());
				hashSet.Add(cVSet4);
				dictionary.Add(cVSet4, OV);
			}
			if (target == "NonOverlapable")
			{
				CalcAverage_NonOverlapable(hashSet, dictionary, Out);
			}
			if (target == "Overlapable")
			{
				CalcAverage_Overlapable(hashSet, dictionary, Out);
			}
		}
	}
}
