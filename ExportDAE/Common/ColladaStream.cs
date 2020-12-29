using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Collada141;

namespace ExportDAE
{

    internal class ColladaStream
    {
        [CompilerGenerated]
        [Serializable]
        private sealed class common
        {
            public static readonly common T = new common();
            public static Func<ModelGeometry, int> T_PointCount;
            public static Func<ModelGeometry, int> T_NormalsCount;

            internal int GetPointCount(ModelGeometry item)
            {
                return item.Points.Count;
            }

            internal int GetNormalsCount(ModelGeometry item)
            {
                return item.Normals.Count;
            }
        }
        /// <summary>
        /// 文档和材料ID到Geometries
        /// </summary>
        private Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> Geometries = new Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>>();

        /// <summary>
        /// 文档和材料ID到Material
        /// </summary>
        private Dictionary<Tuple<Document, ElementId>, ModelMaterial> Material = new Dictionary<Tuple<Document, ElementId>, ModelMaterial>();
        private COLLADA mode = new COLLADA();
        public ColladaStream(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial, Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> documentAndMaterialIdToGeometries)
        {
            //保存模型及材质
            Geometries = documentAndMaterialIdToGeometries;
            Material = documentAndMaterialIdToExportedMaterial;
            


            GetGeometrieList();


        }

        public bool GetGeometrieList()
        {
            foreach (Tuple<Document, ElementId> key in Geometries.Keys)
            {
                Console.WriteLine(string.Format("key: {0}::{1} ;;value：{2}", key.Item1,key.Item2, Geometries[key]));
            }
            return true;
        }


        /*
          WriteXmlAsset();
          WriteXmlLibraryGeometries();
          WriteXmlLibraryImages();
          WriteXmlLibraryMaterials();
          WriteXmlLibraryEffects();
          WriteXmlLibraryVisualScenes();
          */

        /// <summary>
        /// 设置描述——单位名、单位、及指定向上轴向
        /// </summary>
        /// <param name="unit_name">单位名称</param>
        /// <param name="meter">单位，默认米</param>
        /// <param name="up_axis">向上轴向</param>
      
        public void PrintModel()
        {
            //init asset：创建工具和信息
            //mode.asset = new asset();
            //mode.asset.contributor[0] = new assetContributor();
            //mode.asset.unit = new assetUnit();//这里有问题
            //mode.asset.up_axis = new UpAxisType();
            //mode.Items = new object[0];

            //init library_geometries：创建节点对象library_geometries
            //library_geometries library_geom = new library_geometries();

            // init library_visual_scenes：创建节点对象library_visual_scenes
            //library_visual_scenes lib_visual_scene = new library_visual_scenes();
            mode.asset.created = DateTime.Now;
            mode.asset.unit.name = "Ft";
            mode.asset.unit.meter = 0.3048;
            mode.asset.contributor[0].author = "ExportDAE";
            mode.asset.contributor[0].authoring_tool = "RevitAddinos_ExportDAE";
            mode.asset.contributor[0].comments = "";
            mode.asset.up_axis = UpAxisType.Z_UP;

            library_geometries library_geom = new library_geometries();
            library_geom.geometry = new geometry[1]; //创建一个数组。
            library_geom.geometry[0] = new geometry();//将对象赋值给数组。

            mesh geomMesh = new mesh();
            //source[] source  = new source[3]
            geomMesh.source = new source[1];
            //used  public source[] source
            geomMesh.source[0] = new source();

            //设置geomtry子集mesh
            library_geom.geometry[0].Item = geomMesh;

            float_array position_float_array = new float_array();
            float_array normal_float_array = new float_array();
            float_array uv_float_array = new float_array();
            sourceTechnique_common position_technique_common = new sourceTechnique_common();
            sourceTechnique_common normal_technique_common = new sourceTechnique_common();
            sourceTechnique_common uv_technique_common = new sourceTechnique_common();
            foreach (KeyValuePair<Tuple<Document, ElementId>, IList<ModelGeometry>> current in Geometries)
            {
                Tuple<Document, ElementId> key = current.Key;
                if(current.Value != null && current.Value.Count > 0)
                {
                    ModelMaterial exprMaterial = Material[key];
                    library_geom.geometry[0].id = "geom-" + key.GetHashCode();
                    library_geom.geometry[0].name = exprMaterial.Name;

                    Func<ModelGeometry, int> getCount;
                    if((getCount = common.T_PointCount) == null)
                    {
                        getCount = common.T_PointCount = new Func<ModelGeometry, int>(common.T.GetPointCount);
                    }
                    int num = current.Value.Sum(getCount);
                    
                    //setting source
                    geomMesh.source[0].id = "geom-"+key.GetHashCode()+ "-positions";
                    position_float_array.id = "geom-" +key.GetHashCode()+"-positions-array";
                    position_float_array.count = (ulong)(num * 3);

                    using(IEnumerator<ModelGeometry> enumerator = current.Value.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ModelGeometry geometry = enumerator.Current;
                            if (!geometry.Transform.IsIdentity)
                            {
                                Parallel.For(0, geometry.Points.Count, delegate (int iPoint)
                                {
                                    geometry.Points[iPoint] = geometry.Transform.OfPoint(geometry.Points[iPoint]);
                                });

                            }
                            for (int i = 0; i<geometry.Points.Count; i++)
                            {
                                XYZ xYZ = geometry.Points[i];
                                position_float_array.Values = new double[]
                                {
                                    xYZ.X,
                                    xYZ.Y,
                                    xYZ.Z
                                };
                                // TODO
                            }
                        }
                    }
                    position_technique_common.accessor = new accessor();
                    position_technique_common.accessor.param = new param[3];
                    position_technique_common.accessor.param[0] = new param();
                    position_technique_common.accessor.param[1] = new param();
                    position_technique_common.accessor.param[2] = new param();
                    position_technique_common.accessor.source = "#" + position_float_array.id;
                    position_technique_common.accessor.count = 3;
                    position_technique_common.accessor.stride = 3;
                    position_technique_common.accessor.param[0].name = "X";
                    position_technique_common.accessor.param[0].type = "float";
                    position_technique_common.accessor.param[1].name = "Y";
                    position_technique_common.accessor.param[1].type = "float";
                    position_technique_common.accessor.param[2].name = "Z";
                    position_technique_common.accessor.param[2].type = "float";
                }

            }


            //法线
            Func<ModelGeometry, int> NormalsCount;
            if((NormalsCount = common.T_NormalsCount) == null)
            {
                NormalsCount = common.T_NormalsCount = new Func<ModelGeometry, int>(common.T.GetNormalsCount);
            }
            



        }
      

        
    }
}
