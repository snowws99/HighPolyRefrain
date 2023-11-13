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
			int keyCount = keys.Count;
			Dictionary<IPXMorph, V4> morphDictionary = new Dictionary<IPXMorph, V4>();
			foreach (CVSet key in keys)
			{
				Out.m_v.UV += dic[key].m_v.UV;
				Out.m_v.UVA1 += dic[key].m_v.UVA1;
				Out.m_v.UVA2 += dic[key].m_v.UVA2;
				Out.m_v.UVA3 += dic[key].m_v.UVA3;
				Out.m_v.UVA4 += dic[key].m_v.UVA4;
				Out.m_v.EdgeScale += dic[key].m_v.EdgeScale;
				foreach (IPXMorph morph in dic[key].m_UVMO.Keys)
				{
					if (morphDictionary.ContainsKey(morph))
					{
						morphDictionary[morph] += dic[key].m_UVMO[morph];
					}
					else
					{
						morphDictionary.Add(morph, dic[key].m_UVMO[morph]);
					}
				}
			}
			Out.m_v.UV /= (float)keyCount;
			Out.m_v.UVA1 /= (float)keyCount;
			Out.m_v.UVA2 /= (float)keyCount;
			Out.m_v.UVA3 /= (float)keyCount;
			Out.m_v.UVA4 /= (float)keyCount;
			Out.m_v.EdgeScale /= keyCount;

			foreach (IPXMorph morph in morphDictionary.Keys)
			{
				Out.m_UVMO.Add(morph, morphDictionary[morph] / keyCount);
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
			int keyCount = keys.Count;
			Dictionary<IPXBone, float> boneDictionary = new Dictionary<IPXBone, float>();
			Dictionary<IPXMorph, V3> morphDictionary = new Dictionary<IPXMorph, V3>();
			foreach (CVSet key in keys)
			{
				Out.m_v.Position += dic[key].m_v.Position;
				Out.m_v.Normal += dic[key].m_v.Normal;
				if (boneDictionary.ContainsKey(dic[key].m_v.Bone1))
				{
					boneDictionary[dic[key].m_v.Bone1] += dic[key].m_v.Weight1;
				}
				else
				{
					boneDictionary.Add(dic[key].m_v.Bone1, dic[key].m_v.Weight1);
				}
				if (dic[key].m_v.Bone2 != null)
				{
					if (boneDictionary.ContainsKey(dic[key].m_v.Bone2))
					{
						boneDictionary[dic[key].m_v.Bone2] += dic[key].m_v.Weight2;
					}
					else
					{
						boneDictionary.Add(dic[key].m_v.Bone2, dic[key].m_v.Weight2);
					}
					if (dic[key].m_v.Bone3 != null)
					{
						if (boneDictionary.ContainsKey(dic[key].m_v.Bone3))
						{
							boneDictionary[dic[key].m_v.Bone3] += dic[key].m_v.Weight3;
						}
						else
						{
							boneDictionary.Add(dic[key].m_v.Bone3, dic[key].m_v.Weight3);
						}
						if (dic[key].m_v.Bone4 != null)
						{
							if (boneDictionary.ContainsKey(dic[key].m_v.Bone4))
							{
								boneDictionary[dic[key].m_v.Bone4] += dic[key].m_v.Weight4;
							}
							else
							{
								boneDictionary.Add(dic[key].m_v.Bone4, dic[key].m_v.Weight4);
							}
						}
					}
				}
				foreach (IPXMorph key2 in dic[key].m_VMO.Keys)
				{
					if (morphDictionary.ContainsKey(key2))
					{
						morphDictionary[key2] += dic[key].m_VMO[key2];
					}
					else
					{
						morphDictionary.Add(key2, dic[key].m_VMO[key2]);
					}
				}
			}
			Out.m_v.Position /= (float)keyCount;
			WrapNormalize(Out.m_v.Normal);
			int boneCount = boneDictionary.Count;
			//TODO investigate
			int num = Math.Min(boneDictionary.Count, 4);
			
			IPXBone[] boneKeys = boneDictionary.Keys.ToArray();
			float[] boneValues = boneDictionary.Values.ToArray();
			Array.Sort(boneValues, boneKeys);
			float num2 = 0f;
			for (int i = 1; i <= num; i++)
			{
				num2 += boneValues[boneCount - i];
			}
			for (int j = 1; j <= num; j++)
			{
				boneValues[boneCount - j] /= num2;
			}
			Out.m_v.Bone1 = boneKeys[boneCount - 1];
			Out.m_v.Weight1 = boneValues[boneCount - 1];

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
					Out.m_v.Bone2 = boneKeys[boneCount - 2];
					Out.m_v.Weight2 = boneValues[boneCount - 2];
					Out.m_v.Bone3 = null;
					Out.m_v.Weight3 = 0f;
					Out.m_v.Bone4 = null;
					Out.m_v.Weight4 = 0f;
					break;
				case 3:
					Out.m_v.Bone2 = boneKeys[boneCount - 2];
					Out.m_v.Weight2 = boneValues[boneCount - 2];
					Out.m_v.Bone3 = boneKeys[boneCount - 3];
					Out.m_v.Weight3 = boneValues[boneCount - 3];
					Out.m_v.Bone4 = null;
					Out.m_v.Weight4 = 0f;
					break;
				default:
					Out.m_v.Bone2 = boneKeys[boneCount - 2];
					Out.m_v.Weight2 = boneValues[boneCount - 2];
					Out.m_v.Bone3 = boneKeys[boneCount - 3];
					Out.m_v.Weight3 = boneValues[boneCount - 3];
					Out.m_v.Bone4 = boneKeys[boneCount - 4];
					Out.m_v.Weight4 = boneValues[boneCount - 4];
					break;
			}

			foreach (IPXMorph morph in morphDictionary.Keys)
			{
				Out.m_VMO.Add(morph, morphDictionary[morph] / keyCount);
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
