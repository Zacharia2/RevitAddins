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

    internal class Mapping
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
        /// 模型数据池：文档和材料ID到Geometries    
        /// </summary>
        private Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> Geometries = new Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>>();

        /// <summary>
        /// 材质数据池：文档和材料ID到Material
        /// </summary>
        private Dictionary<Tuple<Document, ElementId>, ModelMaterial> Material = new Dictionary<Tuple<Document, ElementId>, ModelMaterial>();
        private COLLADA collada = new COLLADA();







        public Mapping(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial, Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> documentAndMaterialIdToGeometries)
        {
            //保存模型及材质
            Geometries = documentAndMaterialIdToGeometries;
            Material = documentAndMaterialIdToExportedMaterial;
        }




        public void SaveModel(string savePath)
        {
            SetAsset();
            SetLibraryGeometries();
            SetLibraryImages();
            SetLibraryMaterials();
            SetLibraryEffects();
            SetLibraryVisualScenes();
            collada.Save(savePath);
        }

       





        /// <summary>
        /// 设置描述——单位名、单位、及指定向上轴向
        /// </summary>
        /// <param name="unit_name">单位名称</param>
        /// <param name="meter">单位，默认米</param>
        /// <param name="up_axis">向上轴向</param>
        /// 
        public void SetAsset()
        {
            //init asset：创建工具和信息
            collada.asset = new asset();
            collada.asset.contributor = new assetContributor[1];
            collada.asset.contributor[0] = new assetContributor();

            collada.asset.unit = new assetUnit();
            collada.asset.up_axis = new UpAxisType();

            collada.Items = new object[1];


            //init library_geometries：创建节点对象library_geometries
            //library_geometries library_geom = new library_geometries();

            // init library_visual_scenes：创建节点对象library_visual_scenes
            //library_visual_scenes lib_visual_scene = new library_visual_scenes();
            collada.asset.created = DateTime.Now;

            collada.asset.unit.name = "feet";
            collada.asset.unit.meter = 0.3048;
            collada.asset.up_axis = UpAxisType.Z_UP;

            collada.asset.contributor[0].author = "RevitAddins";
            collada.asset.contributor[0].authoring_tool = "RevitAddions_ExportDAE";
            collada.asset.contributor[0].comments = "";

        }


        public void SetLibraryGeometries()
        {


            foreach (KeyValuePair<Tuple<Document, ElementId>, IList<ModelGeometry>> current in this.Geometries)
            {
                Tuple<Document, ElementId> key = current.Key;
                if (current.Value != null && current.Value.Count > 0)
                {
                    ModelMaterial exportedMaterial = this.Material[key];
                    string nodeName = exportedMaterial.Name;


                    GeometrySourcePositions(key.GetHashCode(), current.Value, nodeName);
                    GeometrySourceNormals(key.GetHashCode(), current.Value);
                    GeometrySourceMap(key.GetHashCode(), exportedMaterial, current.Value);
                    GeometryVertices(key.GetHashCode());
                    GeometryTrianglesWithMap(key.GetHashCode(), current.Value);

                }
            }










        }
        private void GeometrySourcePositions(int documentAndMaterialIdHash, IList<ModelGeometry> geometries, string nodename)
        {

            



            float_array position_float_array = new float_array();
            float_array normal_float_array = new float_array();
            float_array uv_float_array = new float_array();
            sourceTechnique_common position_technique_common = new sourceTechnique_common();
            sourceTechnique_common normal_technique_common = new sourceTechnique_common();
            sourceTechnique_common uv_technique_common = new sourceTechnique_common();







            Func<ModelGeometry, int> getCount;
            if ((getCount = common.T_PointCount) == null)
            {
                getCount = common.T_PointCount = new Func<ModelGeometry, int>(common.T.GetPointCount);
            }
            //这是统计geometries - {0} - Pooints - count 数值的和
            //应该统计 某项中 Pooints的和

            //[0] 算是一个几何体，我们要得到几何体下面的Potints的数量，然后乘以3 得到坐标的数量。
            // geometries
            //     [0]：物体模型
            //         Points: 点对象
            //             [0]:包含XYZ的坐标
            int num = geometries.Sum(getCount);


            // 现在我有了一个数据模型，我要对这个数据模型进行操作！！！
            // geometries - geometry - mesh - source - float_array
            // 思路，迭代geometries列表，从索引为 0 的第一个ModelGeometry对象geometry获取属性为Point的数量，乘以3得到坐标数量。




            var geometriesPonits = geometries.Select(t => t.Points).ToList();
            int num1 = geometriesPonits.Count;






    
           

            library_geometries library_Geometries = new library_geometries
            {
                geometry = new geometry[geometries.Count] //创建library_geometries中存在的多个geometry对象数组
            };



            mesh[] geomMesh = new mesh[geometries.Count];

            //setting source
            //geomMesh.source[0].id = "geom-" + documentAndMaterialIdHash + "-positions";
            //position_float_array.id = "geom-" + documentAndMaterialIdHash + "-positions-array";
            position_float_array.count = (ulong)(num * 3);//这里有问题3876了都



            using (IEnumerator<ModelGeometry> enumerator = geometries.GetEnumerator())
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
                    for (int i = 0; i < geometry.Points.Count; i++)
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



        

        private void GeometrySourceNormals(int documentAndMaterialIdHash, IList<ModelGeometry> geometries)
        {

            //法线
            Func<ModelGeometry, int> NormalsCount;
            if ((NormalsCount = common.T_NormalsCount) == null)
            {
                NormalsCount = common.T_NormalsCount = new Func<ModelGeometry, int>(common.T.GetNormalsCount);
            }


            // 同GeometrySourcePositions





        }

        private void GeometrySourceMap(int documentAndMaterialIdHash, ModelMaterial exportedMaterial, IList<ModelGeometry> geometries)
        {

        }
        




        private void GeometryVertices(int documentAndMaterialIdHash)
        {

            // 同GeometrySourcePositions

        }





        private void GeometryTrianglesWithMap(int documentAndMaterialIdHash, IList<ModelGeometry> geometries)
        {


        }



        private void SetLibraryEffects()
        {
            throw new NotImplementedException();
        }

        private void SetLibraryVisualScenes()
        {
            throw new NotImplementedException();
        }

        private void SetLibraryMaterials()
        {
            throw new NotImplementedException();
        }

        private void SetLibraryImages()
        {
            throw new NotImplementedException();
        }

    }
}

