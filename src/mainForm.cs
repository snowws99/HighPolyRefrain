using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using PEPlugin;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PEPlugin.View;
using System.Media;

namespace PECSScriptPlugin
{
	internal class mainForm : Form
	{
		private IPEPluginHost host;

		private IPXPmxBuilder bdx;

		private IPEConnector connect;

		private IPEPMDViewConnector view;

		private IPXPmx pmx;

		private IList<IPXVertex> vertex;

		private IList<IPXMaterial> material;

		private IList<IPXMorph> morph;

		private IPEPmd pmd;

		private IList<int> face;


        private GroupBox groupBoxModes;
        private GroupBox groupBoxEdgeProcessing;
        private Button ButtonProcess;
        private RadioButton radioButtonAddAndSoft;
        private ProgressBar progressBar1;
        private RadioButton radioButtonSoft;
        private RadioButton radioButtonAdd;
        private CheckBox checkBoxMergeDuplicates;
        private Label label1;
        private CheckBox checkBoxRollUp;

		public mainForm(IPERunArgs args)
		{
			host = args.Host;
			bdx = host.Builder.Pmx;
			connect = host.Connector;
			view = host.Connector.View.PMDView;
			InitializeComponent();
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            getcurrent();
            //progressBar1.();
            int[] selectedFaceIndices = view.GetSelectedFaceIndices();
            //calculate amount of faces selected
            int faces = selectedFaceIndices.Length / 3;
            
            if (faces <= 0)
            {
                MessageBox.Show("Select surfaces in main window before launching processing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            progressBar1.PerformStep();
            List<int> faceIndiceList = new List<int>();
            Dictionary<IPXMaterial, IPXMaterial> materialsDictionary = new Dictionary<IPXMaterial, IPXMaterial>();
            Dictionary<CVSet, CVDat> cvDatDictionary1 = new Dictionary<CVSet, CVDat>();
            Dictionary<CVSet, CVDat> cvDatDictionary2 = new Dictionary<CVSet, CVDat>();
            Dictionary<CVSet, CVDat> cvDatDictionary3 = new Dictionary<CVSet, CVDat>();
            Dictionary<CVPosKey, CVSet> duplicatesDictionary = new Dictionary<CVPosKey, CVSet>();
            Dictionary<CEPosKey, HashSet<CVSet>> mergeDuplicatesDictionary = new Dictionary<CEPosKey, HashSet<CVSet>>();
            Dictionary<CVSet, HashSet<CVSet>> cvRollupDictionary = new Dictionary<CVSet, HashSet<CVSet>>();
            Dictionary<CVSet, CEFKeys> dictionary8 = new Dictionary<CVSet, CEFKeys>();
            Dictionary<CVSet, CVDat> dictionary9 = new Dictionary<CVSet, CVDat>();
            Dictionary<CVSet, CVDat> dictionary10 = new Dictionary<CVSet, CVDat>();

            //Count face indices
            for (int i = 0; i < faces; i++)
            {
                faceIndiceList.Add(selectedFaceIndices[i * 3] / 3);
            }
            faceIndiceList.Sort();
            
            int count = material.Count;
            int materialIndice = 0;
            int j = material[materialIndice].Faces.Count();
            //clone selected materials
            foreach (int faceIndice in faceIndiceList)
            {
                for (; faceIndice >= j; j += material[++materialIndice].Faces.Count)
                {
                }
                if (!materialsDictionary.ContainsKey(material[materialIndice]))
                {
                    IPXMaterial iPXMaterial = (IPXMaterial)material[materialIndice].Clone();
                    if (radioButtonSoft.Checked)
                    {
                        iPXMaterial.Name += "x1";
                        iPXMaterial.NameE += "x1";
                    }
                    else
                    {
                        iPXMaterial.Name += "x2";
                        iPXMaterial.NameE += "x2";
                    }
                    iPXMaterial.Faces.Clear();
                    material.Add(iPXMaterial);
                    materialsDictionary.Add(material[materialIndice], material[material.Count - 1]);
                }
            }
            
            connect.View.PMDViewHelper.PartsSelect.SelectObject = PartsSelectObject.Material;
            int[] array = new int[material.Count - count];
            for (int k = 0; k < material.Count - count; k++)
            {
                array[k] = count + k;
            }
            connect.View.PMDViewHelper.PartsSelect.SetCheckedMaterialIndices(array);



            progressBar1.PerformStep();
            progressBar1.PerformStep();

            //load all Morphs materials into materialOffsetList
            LoadMaterialOffsets(materialsDictionary);

            progressBar1.PerformStep();

            foreach (int faceIndice in faceIndiceList)
            {
                for (int l = 0; l < 3; l++)
                {
                    CVSet cVSet = new CVSet(vertex[face[3 * faceIndice + l]]);
                    if (cvDatDictionary1.ContainsKey(cVSet))
                        continue;

                    cvDatDictionary1.Add(cVSet, new CVDat(vertex[face[3 * faceIndice + l]]));

                    if (checkBoxMergeDuplicates.Checked && checkBoxMergeDuplicates.Enabled)
                    {
                        CVPosKey key = new CVPosKey(vertex[face[3 * faceIndice + l]]);
                        if (!duplicatesDictionary.ContainsKey(key))
                        {
                            duplicatesDictionary.Add(key, new CVSet());
                        }
                        duplicatesDictionary[key].Add(cVSet);
                    }
                }
            }

            foreach (IPXMorph morphItem in morph)
            {
                if (morphItem.IsVertex)
                {
                    foreach (IPXVertexMorphOffset morphOffset in morphItem.Offsets)
                    {
                        CVSet cvkey = new CVSet(morphOffset.Vertex);
                        if (cvDatDictionary1.ContainsKey(cvkey))
                        {
                            if (!cvDatDictionary1[cvkey].m_VMO.ContainsKey(morphItem))
                            {
                                cvDatDictionary1[cvkey].m_VMO.Add(morphItem, morphOffset.Offset);
                            }
                            else
                            {
                                cvDatDictionary1[cvkey].m_VMO[morphItem] += morphOffset.Offset;
                            }
                        }
                    }
                }
                if (!morphItem.IsUV)
                    continue;

                foreach (IPXUVMorphOffset morphOffset in morphItem.Offsets)
                {
                    CVSet cvkey = new CVSet(morphOffset.Vertex);
                    if (cvDatDictionary1.ContainsKey(cvkey))
                    {
                        if (!cvDatDictionary1[cvkey].m_UVMO.ContainsKey(morphItem))
                        {
                            cvDatDictionary1[cvkey].m_UVMO.Add(morphItem, morphOffset.Offset);
                        }
                        else
                        {
                            cvDatDictionary1[cvkey].m_UVMO[morphItem] += morphOffset.Offset;
                        }
                    }
                }
            }

            progressBar1.PerformStep();

            foreach (int faceIndice in faceIndiceList)
            {
                for (int m = 0; m < 3; m++)
                {
                    CVSet cVSet2 = new CVSet(new IPXVertex[2]
                    {
                        vertex[face[3 * faceIndice + m]],
                        vertex[face[3 * faceIndice + (m + 1) % 3]]
                    });
                    if (cvDatDictionary2.ContainsKey(cVSet2))
                    {
                        continue;
                    }
                    CVDat cVDat = new CVDat(bdx);
                    Ifunc.CalcAverage_NonOverlapable(cVSet2.Separate(), cvDatDictionary1, cVDat);
                    Ifunc.CalcAverage_Overlapable(cVSet2.Separate(), cvDatDictionary1, cVDat);
                    cvDatDictionary2.Add(cVSet2, cVDat);
                    if (checkBoxMergeDuplicates.Checked && checkBoxMergeDuplicates.Enabled)
                    {
                        CEPosKey key4 = new CEPosKey(cVSet2);
                        if (!mergeDuplicatesDictionary.ContainsKey(key4))
                        {
                            mergeDuplicatesDictionary.Add(key4, new HashSet<CVSet>());
                        }
                        mergeDuplicatesDictionary[key4].Add(cVSet2);
                    }
                }
                if (radioButtonAdd.Checked)
                {
                    continue;
                }
                CVSet cVSet3 = new CVSet(new IPXVertex[3]
                {
                    vertex[face[3 * faceIndice]],
                    vertex[face[3 * faceIndice + 1]],
                    vertex[face[3 * faceIndice + 2]]
                });
                if (!cvDatDictionary3.ContainsKey(cVSet3))
                {
                    CVDat cVDat2 = new CVDat(bdx);
                    Ifunc.CalcAverage_NonOverlapable(cVSet3.Separate(), cvDatDictionary1, cVDat2);
                    Ifunc.CalcAverage_Overlapable(cVSet3.Separate(), cvDatDictionary1, cVDat2);
                    cvDatDictionary3.Add(cVSet3, cVDat2);
                }
                for (int n = 0; n < 3; n++)
                {
                    CVSet cVSet4 = new CVSet(new IPXVertex[2]
                    {
                        vertex[face[3 * faceIndice + n]],
                        vertex[face[3 * faceIndice + (n + 1) % 3]]
                    });
                    if (!radioButtonSoft.Checked || (checkBoxRollUp.Checked && checkBoxRollUp.Enabled))
                    {
                        if (!cvRollupDictionary.ContainsKey(cVSet4))
                        {
                            cvRollupDictionary.Add(cVSet4, new HashSet<CVSet>());
                        }
                        cvRollupDictionary[cVSet4].Add(cVSet3);
                    }
                    CVSet key5 = new CVSet(vertex[face[3 * faceIndice + n]]);
                    CVSet item = new CVSet(new IPXVertex[2]
                    {
                        vertex[face[3 * faceIndice + n]],
                        vertex[face[3 * faceIndice + (n + 2) % 3]]
                    });
                    if (!dictionary8.ContainsKey(key5))
                    {
                        dictionary8.Add(key5, new CEFKeys());
                    }
                    dictionary8[key5].m_FaceKeys.Add(cVSet3);
                    dictionary8[key5].m_EdgeKeys.Add(cVSet4);
                    dictionary8[key5].m_EdgeKeys.Add(item);
                }
            }

            progressBar1.PerformStep();

            if (!radioButtonSoft.Checked)
            {
                foreach (CVSet cvSetItem in cvDatDictionary2.Keys)
                {
                    CEPosKey cekey = new CEPosKey(cvSetItem);
                    int selector = 3;
                    if (radioButtonAddAndSoft.Checked)
                    {
                        if (cvRollupDictionary[cvSetItem].Count >= 2)
                        {
                            selector = 1;
                        }
                        else if (checkBoxMergeDuplicates.Checked && checkBoxMergeDuplicates.Enabled && mergeDuplicatesDictionary[cekey].Count > 1)
                        {
                            selector = 2;
                        }
                    }
                    if (selector != 3)
                    {
                        HashSet<CVSet> hashSet = new HashSet<CVSet>();
                        Dictionary<CVSet, CVDat> dictionary11 = new Dictionary<CVSet, CVDat>();
                        foreach (CVSet cvdatkeySet in cvSetItem.Separate())
                        {
                            hashSet.Add(cvdatkeySet);
                            dictionary11.Add(cvdatkeySet, cvDatDictionary1[cvdatkeySet]);
                        }
                        CVDat cVDat3;
                        if (selector == 1)
                        {
                            foreach (CVSet item9 in cvRollupDictionary[cvSetItem])
                            {
                                hashSet.Add(item9);
                                dictionary11.Add(item9, cvDatDictionary3[item9]);
                            }
                            cVDat3 = new CVDat(bdx);
                            Ifunc.CalcAverage_NonOverlapable(hashSet, dictionary11, cVDat3);
                            Ifunc.CalcAverage_Overlapable(hashSet, dictionary11, cVDat3);
                        }
                        else
                        {
                            cVDat3 = new CVDat(cvDatDictionary2[cvSetItem]);
                            foreach (CVSet item10 in mergeDuplicatesDictionary[cekey])
                            {
                                foreach (CVSet item11 in cvRollupDictionary[item10])
                                {
                                    hashSet.Add(item11);
                                    dictionary11.Add(item11, cvDatDictionary3[item11]);
                                }
                            }
                            Ifunc.CalcAverage_Overlapable(hashSet, dictionary11, cVDat3);
                        }
                        dictionary9.Add(cvSetItem, cVDat3);
                    }
                    else
                    {
                        dictionary9.Add(cvSetItem, cvDatDictionary2[cvSetItem]);
                    }
                    vertex.Add(dictionary9[cvSetItem].m_v);
                }
            }

            progressBar1.PerformStep();

            foreach (CVSet cvSetItem in cvDatDictionary1.Keys)
            {
                HashSet<CEPosKey> hashSet2 = new HashSet<CEPosKey>();
                HashSet<CVSet> hashSet3 = new HashSet<CVSet>();
                int selector = 5;
                if (!radioButtonAdd.Checked)
                {
                    if (dictionary8[cvSetItem].m_EdgeKeys.Count == dictionary8[cvSetItem].m_FaceKeys.Count)
                    {
                        selector = 1;
                    }
                    else if (!checkBoxMergeDuplicates.Checked || !checkBoxMergeDuplicates.Enabled)
                    {
                        if (checkBoxRollUp.Checked && checkBoxRollUp.Enabled)
                        {
                            selector = 2;
                        }
                    }
                    else
                    {
                        CVSet cVSet5 = duplicatesDictionary[new CVPosKey(cvSetItem)];
                        if (cVSet5.m_vset.Count < 2)
                        {
                            if (checkBoxRollUp.Checked && checkBoxRollUp.Enabled)
                            {
                                selector = 2;
                            }
                        }
                        else
                        {
                            foreach (CVSet item12 in cVSet5.Separate())
                            {
                                foreach (CVSet edgeKey in dictionary8[item12].m_EdgeKeys)
                                {
                                    hashSet2.Add(new CEPosKey(edgeKey));
                                }
                                hashSet3.UnionWith(dictionary8[item12].m_FaceKeys);
                            }
                            if (hashSet2.Count == hashSet3.Count)
                            {
                                selector = 3;
                            }
                            else if (checkBoxRollUp.Checked && checkBoxRollUp.Enabled)
                            {
                                selector = 4;
                            }
                        }
                    }
                }
                if (selector == 1)
                {
                    CVDat cVDat4 = new CVDat(bdx);
                    CVDat cVDat5 = new CVDat(bdx);
                    CVDat cVDat6 = new CVDat(bdx);
                    Ifunc.CalcAverage_NonOverlapable(dictionary8[cvSetItem].m_EdgeKeys, cvDatDictionary2, cVDat5);
                    Ifunc.CalcAverage_Overlapable(dictionary8[cvSetItem].m_EdgeKeys, cvDatDictionary2, cVDat5);
                    Ifunc.CalcAverage_NonOverlapable(dictionary8[cvSetItem].m_FaceKeys, cvDatDictionary3, cVDat6);
                    Ifunc.CalcAverage_Overlapable(dictionary8[cvSetItem].m_FaceKeys, cvDatDictionary3, cVDat6);
                    Ifunc.CalcCatmullClarkMethod(cvDatDictionary1[cvSetItem], cVDat5, cVDat6, dictionary8[cvSetItem].m_FaceKeys.Count, cVDat4, "NonOverlapable", bdx);
                    Ifunc.CalcCatmullClarkMethod(cvDatDictionary1[cvSetItem], cVDat5, cVDat6, dictionary8[cvSetItem].m_FaceKeys.Count, cVDat4, "Overlapable", bdx);
                    dictionary10.Add(cvSetItem, cVDat4);
                }
                if (selector == 2)
                {
                    CVDat cVDat7 = new CVDat(bdx);
                    HashSet<CVSet> hashSet4 = new HashSet<CVSet>();
                    Dictionary<CVSet, CVDat> dictionary12 = new Dictionary<CVSet, CVDat>();
                    hashSet4.Clear();
                    dictionary12.Clear();
                    hashSet4.Add(cvSetItem);
                    dictionary12.Add(cvSetItem, cvDatDictionary1[cvSetItem]);
                    foreach (CVSet edgeKey2 in dictionary8[cvSetItem].m_EdgeKeys)
                    {
                        if (cvRollupDictionary[edgeKey2].Count < 2)
                        {
                            hashSet4.Add(edgeKey2);
                            dictionary12.Add(edgeKey2, cvDatDictionary2[edgeKey2]);
                        }
                    }
                    cVDat7.m_v.UV = new V2(cvDatDictionary1[cvSetItem].m_v.UV);
                    cVDat7.m_v.UVA1 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA1);
                    cVDat7.m_v.UVA2 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA2);
                    cVDat7.m_v.UVA3 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA3);
                    cVDat7.m_v.UVA4 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA4);
                    cVDat7.m_v.EdgeScale = cvDatDictionary1[cvSetItem].m_v.EdgeScale;
                    cVDat7.m_UVMO = new Dictionary<IPXMorph, V4>(cvDatDictionary1[cvSetItem].m_UVMO);
                    Ifunc.CalcAverage_Overlapable(hashSet4, dictionary12, cVDat7);
                    dictionary10.Add(cvSetItem, cVDat7);
                }
                if (selector == 3)
                {
                    CVDat cVDat8 = new CVDat(bdx);
                    CVDat cVDat9 = new CVDat(bdx);
                    CVDat cVDat10 = new CVDat(bdx);
                    HashSet<CVSet> hashSet5 = new HashSet<CVSet>();
                    Dictionary<CVSet, CVDat> dictionary13 = new Dictionary<CVSet, CVDat>();
                    cVDat8.m_v.UV = new V2(cvDatDictionary1[cvSetItem].m_v.UV);
                    cVDat8.m_v.UVA1 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA1);
                    cVDat8.m_v.UVA2 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA2);
                    cVDat8.m_v.UVA3 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA3);
                    cVDat8.m_v.UVA4 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA4);
                    cVDat8.m_v.EdgeScale = cvDatDictionary1[cvSetItem].m_v.EdgeScale;
                    cVDat8.m_UVMO = new Dictionary<IPXMorph, V4>(cvDatDictionary1[cvSetItem].m_UVMO);
                    hashSet5.Clear();
                    dictionary13.Clear();
                    //TODO
                    foreach (CEPosKey item13 in hashSet2)
                    {
                        if (mergeDuplicatesDictionary[item13].Count > 1)
                        {
                            CVDat cVDat11 = new CVDat(bdx);
                            CVSet cVSet6 = new CVSet(bdx.Vertex());
                            Ifunc.CalcAverage_Overlapable(mergeDuplicatesDictionary[item13], cvDatDictionary2, cVDat11);
                            hashSet5.Add(cVSet6);
                            dictionary13.Add(cVSet6, cVDat11);
                            continue;
                        }
                        foreach (CVSet item14 in mergeDuplicatesDictionary[item13])
                        {
                            hashSet5.Add(item14);
                            dictionary13.Add(item14, cvDatDictionary2[item14]);
                        }
                    }
                    Ifunc.CalcAverage_Overlapable(hashSet5, dictionary13, cVDat9);
                    Ifunc.CalcAverage_Overlapable(hashSet3, cvDatDictionary3, cVDat10);
                    Ifunc.CalcCatmullClarkMethod(cvDatDictionary1[cvSetItem], cVDat9, cVDat10, hashSet3.Count, cVDat8, "Overlapable", bdx);
                    dictionary10.Add(cvSetItem, cVDat8);
                }
                if (selector == 4)
                {
                    CVDat cVDat12 = new CVDat(bdx);
                    new CVDat(bdx);
                    new CVDat(bdx);
                    HashSet<CVSet> hashSet6 = new HashSet<CVSet>();
                    Dictionary<CVSet, CVDat> dictionary14 = new Dictionary<CVSet, CVDat>();
                    cVDat12.m_v.UV = new V2(cvDatDictionary1[cvSetItem].m_v.UV);
                    cVDat12.m_v.UVA1 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA1);
                    cVDat12.m_v.UVA2 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA2);
                    cVDat12.m_v.UVA3 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA3);
                    cVDat12.m_v.UVA4 = new V4(cvDatDictionary1[cvSetItem].m_v.UVA4);
                    cVDat12.m_v.EdgeScale = cvDatDictionary1[cvSetItem].m_v.EdgeScale;
                    cVDat12.m_UVMO = new Dictionary<IPXMorph, V4>(cvDatDictionary1[cvSetItem].m_UVMO);
                    hashSet6.Clear();
                    dictionary14.Clear();
                    hashSet6.Add(cvSetItem);
                    dictionary14.Add(cvSetItem, cvDatDictionary1[cvSetItem]);
                    foreach (CEPosKey item15 in hashSet2)
                    {
                        if (mergeDuplicatesDictionary[item15].Count != 1)
                        {
                            continue;
                        }
                        foreach (CVSet item16 in mergeDuplicatesDictionary[item15])
                        {
                            if (cvRollupDictionary[item16].Count == 1)
                            {
                                hashSet6.Add(item16);
                                dictionary14.Add(item16, cvDatDictionary2[item16]);
                            }
                        }
                    }
                    Ifunc.CalcAverage_Overlapable(hashSet6, dictionary14, cVDat12);
                    dictionary10.Add(cvSetItem, cVDat12);
                }
                if (selector == 5)
                {
                    dictionary10.Add(cvSetItem, cvDatDictionary1[cvSetItem]);
                }
                vertex.Add(dictionary10[cvSetItem].m_v);
            }

            progressBar1.PerformStep();

            foreach (CVDat value in dictionary10.Values)
            {
                foreach (KeyValuePair<IPXMorph, V3> item17 in value.m_VMO)
                {
                    IPXVertexMorphOffset iPXVertexMorphOffset2 = bdx.VertexMorphOffset();
                    iPXVertexMorphOffset2.Vertex = value.m_v;
                    iPXVertexMorphOffset2.Offset = item17.Value;
                    item17.Key.Offsets.Add(iPXVertexMorphOffset2);
                }
                foreach (KeyValuePair<IPXMorph, V4> item18 in value.m_UVMO)
                {
                    IPXUVMorphOffset iPXUVMorphOffset2 = bdx.UVMorphOffset();
                    iPXUVMorphOffset2.Vertex = value.m_v;
                    iPXUVMorphOffset2.Offset = item18.Value;
                    item18.Key.Offsets.Add(iPXUVMorphOffset2);
                }
            }
            if (!radioButtonSoft.Checked)
            {
                foreach (CVDat value2 in dictionary9.Values)
                {
                    foreach (KeyValuePair<IPXMorph, V3> item19 in value2.m_VMO)
                    {
                        IPXVertexMorphOffset iPXVertexMorphOffset3 = bdx.VertexMorphOffset();
                        iPXVertexMorphOffset3.Vertex = value2.m_v;
                        iPXVertexMorphOffset3.Offset = item19.Value;
                        item19.Key.Offsets.Add(iPXVertexMorphOffset3);
                    }
                    foreach (KeyValuePair<IPXMorph, V4> item20 in value2.m_UVMO)
                    {
                        IPXUVMorphOffset iPXUVMorphOffset3 = bdx.UVMorphOffset();
                        iPXUVMorphOffset3.Vertex = value2.m_v;
                        iPXUVMorphOffset3.Offset = item20.Value;
                        item20.Key.Offsets.Add(iPXUVMorphOffset3);
                    }
                }
            }

            progressBar1.PerformStep();

            materialIndice = 0;
            j = material[materialIndice].Faces.Count();

            foreach (int faceIndice in faceIndiceList)
            {
                for (; faceIndice >= j; j += material[++materialIndice].Faces.Count)
                {
                }
                CVSet key7 = new CVSet(vertex[face[3 * faceIndice]]);
                CVSet key8 = new CVSet(vertex[face[3 * faceIndice + 1]]);
                CVSet key9 = new CVSet(vertex[face[3 * faceIndice + 2]]);
                if (radioButtonSoft.Checked)
                {
                    IPXFace iPXFace = bdx.Face();
                    iPXFace.Vertex1 = dictionary10[key7].m_v;
                    iPXFace.Vertex2 = dictionary10[key8].m_v;
                    iPXFace.Vertex3 = dictionary10[key9].m_v;
                    materialsDictionary[material[materialIndice]].Faces.Add(iPXFace);
                    continue;
                }
                CVSet key10 = new CVSet(new IPXVertex[2]
                {
                    vertex[face[3 * faceIndice]],
                    vertex[face[3 * faceIndice + 1]]
                });
                CVSet key11 = new CVSet(new IPXVertex[2]
                {
                    vertex[face[3 * faceIndice + 1]],
                    vertex[face[3 * faceIndice + 2]]
                });
                CVSet key12 = new CVSet(new IPXVertex[2]
                {
                    vertex[face[3 * faceIndice + 2]],
                    vertex[face[3 * faceIndice]]
                });
                IPXFace iPXFace2 = bdx.Face();
                IPXFace iPXFace3 = bdx.Face();
                IPXFace iPXFace4 = bdx.Face();
                IPXFace iPXFace5 = bdx.Face();
                iPXFace2.Vertex1 = dictionary10[key7].m_v;
                iPXFace2.Vertex2 = dictionary9[key10].m_v;
                iPXFace2.Vertex3 = dictionary9[key12].m_v;
                iPXFace3.Vertex1 = dictionary10[key8].m_v;
                iPXFace3.Vertex2 = dictionary9[key11].m_v;
                iPXFace3.Vertex3 = dictionary9[key10].m_v;
                iPXFace4.Vertex1 = dictionary10[key9].m_v;
                iPXFace4.Vertex2 = dictionary9[key12].m_v;
                iPXFace4.Vertex3 = dictionary9[key11].m_v;
                iPXFace5.Vertex1 = dictionary9[key10].m_v;
                iPXFace5.Vertex2 = dictionary9[key11].m_v;
                iPXFace5.Vertex3 = dictionary9[key12].m_v;
                materialsDictionary[material[materialIndice]].Faces.Add(iPXFace2);
                materialsDictionary[material[materialIndice]].Faces.Add(iPXFace3);
                materialsDictionary[material[materialIndice]].Faces.Add(iPXFace4);
                materialsDictionary[material[materialIndice]].Faces.Add(iPXFace5);
            }
            progressBar1.PerformStep();
            update();
            SystemSounds.Beep.Play();
            //MessageBox.Show("Operation was successfully completed.","Success",, MessageBoxButtons.OK, MessageBoxIcon.);


        }

        private void LoadMaterialOffsets(Dictionary<IPXMaterial, IPXMaterial> materialsDictionary)
        {
            foreach (IPXMorph morphItem in morph)
            {
                if (!morphItem.IsMaterial)
                    continue;

                List<IPXMaterialMorphOffset> materialOffsetList = new List<IPXMaterialMorphOffset>();
                foreach (IPXMaterialMorphOffset morphOffset in morphItem.Offsets)
                {
                    if (morphOffset.Material != null && materialsDictionary.ContainsKey(morphOffset.Material))
                    {
                        IPXMaterialMorphOffset iPXMaterialMorphOffset2 = bdx.MaterialMorphOffset();
                        iPXMaterialMorphOffset2.Material = materialsDictionary[morphOffset.Material];
                        iPXMaterialMorphOffset2.Op = morphOffset.Op;
                        iPXMaterialMorphOffset2.Diffuse = morphOffset.Diffuse;
                        iPXMaterialMorphOffset2.Ambient = morphOffset.Ambient;
                        iPXMaterialMorphOffset2.Specular = morphOffset.Specular;
                        iPXMaterialMorphOffset2.Power = morphOffset.Power;
                        iPXMaterialMorphOffset2.EdgeSize = morphOffset.EdgeSize;
                        iPXMaterialMorphOffset2.EdgeColor = morphOffset.EdgeColor;
                        iPXMaterialMorphOffset2.Tex = morphOffset.Tex;
                        iPXMaterialMorphOffset2.Toon = morphOffset.Toon;
                        iPXMaterialMorphOffset2.Sphere = morphOffset.Sphere;
                        materialOffsetList.Add(iPXMaterialMorphOffset2);
                    }
                }

                if (materialOffsetList.Count <= 0)
                    continue;

                foreach (IPXMaterialMorphOffset morphOffset in materialOffsetList)
                {
                    morphItem.Offsets.Add(morphOffset);
                }
            }
        }

        private void getcurrent()
		{
			pmx = connect.Pmx.GetCurrentState();
			vertex = pmx.Vertex;
			material = pmx.Material;
			morph = pmx.Morph;
			pmd = connect.Pmd.GetCurrentState();
			face = pmd.Face;
		}

		private void update()
		{
			connect.Pmx.Update(pmx);
			connect.Form.UpdateList(UpdateObject.All);
			connect.View.PMDView.UpdateModel();
			connect.View.PMDView.UpdateView();
		}

        private void InitializeComponent()
        {
            this.groupBoxModes = new System.Windows.Forms.GroupBox();
            this.radioButtonAdd = new System.Windows.Forms.RadioButton();
            this.radioButtonSoft = new System.Windows.Forms.RadioButton();
            this.radioButtonAddAndSoft = new System.Windows.Forms.RadioButton();
            this.groupBoxEdgeProcessing = new System.Windows.Forms.GroupBox();
            this.checkBoxMergeDuplicates = new System.Windows.Forms.CheckBox();
            this.checkBoxRollUp = new System.Windows.Forms.CheckBox();
            this.ButtonProcess = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBoxModes.SuspendLayout();
            this.groupBoxEdgeProcessing.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxModes
            // 
            this.groupBoxModes.Controls.Add(this.radioButtonAdd);
            this.groupBoxModes.Controls.Add(this.radioButtonSoft);
            this.groupBoxModes.Controls.Add(this.radioButtonAddAndSoft);
            this.groupBoxModes.Location = new System.Drawing.Point(12, 12);
            this.groupBoxModes.Name = "groupBoxModes";
            this.groupBoxModes.Size = new System.Drawing.Size(200, 94);
            this.groupBoxModes.TabIndex = 0;
            this.groupBoxModes.TabStop = false;
            this.groupBoxModes.Text = "Modes";
            // 
            // radioButtonAdd
            // 
            this.radioButtonAdd.AutoSize = true;
            this.radioButtonAdd.Location = new System.Drawing.Point(6, 65);
            this.radioButtonAdd.Name = "radioButtonAdd";
            this.radioButtonAdd.Size = new System.Drawing.Size(67, 17);
            this.radioButtonAdd.TabIndex = 3;
            this.radioButtonAdd.TabStop = true;
            this.radioButtonAdd.Text = "Only add";
            this.radioButtonAdd.UseVisualStyleBackColor = true;
            this.radioButtonAdd.CheckedChanged += new System.EventHandler(this.radioButtonAdd_CheckedChanged);
            // 
            // radioButtonSoft
            // 
            this.radioButtonSoft.AutoSize = true;
            this.radioButtonSoft.Location = new System.Drawing.Point(6, 42);
            this.radioButtonSoft.Name = "radioButtonSoft";
            this.radioButtonSoft.Size = new System.Drawing.Size(78, 17);
            this.radioButtonSoft.TabIndex = 2;
            this.radioButtonSoft.TabStop = true;
            this.radioButtonSoft.Text = "Only soften";
            this.radioButtonSoft.UseVisualStyleBackColor = true;
            // 
            // radioButtonAddAndSoft
            // 
            this.radioButtonAddAndSoft.AutoSize = true;
            this.radioButtonAddAndSoft.Location = new System.Drawing.Point(6, 19);
            this.radioButtonAddAndSoft.Name = "radioButtonAddAndSoft";
            this.radioButtonAddAndSoft.Size = new System.Drawing.Size(97, 17);
            this.radioButtonAddAndSoft.TabIndex = 1;
            this.radioButtonAddAndSoft.TabStop = true;
            this.radioButtonAddAndSoft.Text = "Add and soften";
            this.radioButtonAddAndSoft.UseVisualStyleBackColor = true;
            // 
            // groupBoxEdgeProcessing
            // 
            this.groupBoxEdgeProcessing.Controls.Add(this.checkBoxMergeDuplicates);
            this.groupBoxEdgeProcessing.Controls.Add(this.checkBoxRollUp);
            this.groupBoxEdgeProcessing.Location = new System.Drawing.Point(218, 12);
            this.groupBoxEdgeProcessing.Name = "groupBoxEdgeProcessing";
            this.groupBoxEdgeProcessing.Size = new System.Drawing.Size(168, 67);
            this.groupBoxEdgeProcessing.TabIndex = 1;
            this.groupBoxEdgeProcessing.TabStop = false;
            this.groupBoxEdgeProcessing.Text = "Edge processing";
            // 
            // checkBoxMergeDuplicates
            // 
            this.checkBoxMergeDuplicates.AutoSize = true;
            this.checkBoxMergeDuplicates.Location = new System.Drawing.Point(6, 42);
            this.checkBoxMergeDuplicates.Name = "checkBoxMergeDuplicates";
            this.checkBoxMergeDuplicates.Size = new System.Drawing.Size(109, 17);
            this.checkBoxMergeDuplicates.TabIndex = 1;
            this.checkBoxMergeDuplicates.Text = "Merge Duplicates";
            this.checkBoxMergeDuplicates.UseVisualStyleBackColor = true;
            // 
            // checkBoxRollUp
            // 
            this.checkBoxRollUp.AutoSize = true;
            this.checkBoxRollUp.Location = new System.Drawing.Point(6, 19);
            this.checkBoxRollUp.Name = "checkBoxRollUp";
            this.checkBoxRollUp.Size = new System.Drawing.Size(56, 17);
            this.checkBoxRollUp.TabIndex = 0;
            this.checkBoxRollUp.Text = "Rollup";
            this.checkBoxRollUp.UseVisualStyleBackColor = true;
            // 
            // ButtonProcess
            // 
            this.ButtonProcess.Location = new System.Drawing.Point(218, 83);
            this.ButtonProcess.Name = "ButtonProcess";
            this.ButtonProcess.Size = new System.Drawing.Size(168, 23);
            this.ButtonProcess.TabIndex = 2;
            this.ButtonProcess.Text = "Process";
            this.ButtonProcess.UseVisualStyleBackColor = true;
            this.ButtonProcess.Click += new System.EventHandler(this.Btn_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 112);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(374, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // myForm
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(392, 145);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.ButtonProcess);
            this.Controls.Add(this.groupBoxEdgeProcessing);
            this.Controls.Add(this.groupBoxModes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "myForm";
            this.Text = "High poly.Refrain";
            this.groupBoxModes.ResumeLayout(false);
            this.groupBoxModes.PerformLayout();
            this.groupBoxEdgeProcessing.ResumeLayout(false);
            this.groupBoxEdgeProcessing.PerformLayout();
            this.ResumeLayout(false);

        }


        private void radioButtonAdd_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAdd.Checked)
            {

                checkBoxRollUp.Enabled = false;
                checkBoxMergeDuplicates.Enabled = false;
            }
            else
            {
                checkBoxRollUp.Enabled = true;
                checkBoxMergeDuplicates.Enabled = true;
            }
        }
    }
}
