using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Collada141;

namespace HelloRevit
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            COLLADA model = new COLLADA();

            // init new
            model.asset = new asset();
            model.asset.contributor[1] = new assetContributor();
            model.asset.contributor[0] = new assetContributor();

            model.asset.unit = new assetUnit();
            model.asset.up_axis = new UpAxisType();

            model.Items = new object[1];

            // asset
            model.asset.contributor[0].author = "caimagic";
            model.asset.contributor[0].authoring_tool = "FBX COLLADA exporter";
            model.asset.contributor[0].comments = "hello world";
            model.asset.unit.meter = 0.01;
            model.asset.unit.name = "centimer";
            model.asset.up_axis = UpAxisType.Y_UP;

            // library_geometries
            library_geometries library_geom = new library_geometries();

            library_geom.geometry[1] = new geometry();
            library_geom.geometry[0] = new geometry();

            geometry geom = new geometry();

            mesh geomMesh = new mesh();
            geomMesh.source[3] = new source();
            geomMesh.source[0] = new source();
            geomMesh.source[1] = new source();
            geomMesh.source[2] = new source();

            float_array position_float_array = new float_array();
            float_array normal_float_array = new float_array();
            float_array uv_float_array = new float_array();
            sourceTechnique_common position_technique_common = new sourceTechnique_common();
            sourceTechnique_common normal_technique_common = new sourceTechnique_common();
            sourceTechnique_common uv_technique_common = new sourceTechnique_common();

            position_float_array.id = "Plane001-POSITION-array";
            position_float_array.count = 9;
            //根据count创建一个数组
            position_float_array.Values = new double[position_float_array.count];
            position_float_array.Values[0] = -49.719101f;
            position_float_array.Values[1] = -41.011238f;
            position_float_array.Values[2] = 0.000000f;
            position_float_array.Values[3] = 49.719101f;
            position_float_array.Values[4] = -41.011238f;
            position_float_array.Values[5] = 0.000000f;
            position_float_array.Values[6] = -49.719101f;
            position_float_array.Values[7] = 41.011238f;
            position_float_array.Values[8] = 0.000000f;

            position_technique_common.accessor = new accessor();
            /*
             * 创建数组的几种形式
             * double[] array = new double[10];
             * double[] array = { 0.0, 1.1, 2.2};
             * double[] array = new double[5]  { 99,  98, 92, 97, 95};
             * double[] array = new double[  ]  { 99,  98, 92, 97, 95};
             * double[] another_array = array;*/
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

            normal_float_array.id = "Plane001-Normal0-array";
            normal_float_array.count = 9;
            normal_float_array.Values = new double[normal_float_array.count];
            normal_float_array.Values[0] = 0.0f;
            normal_float_array.Values[1] = 0.0f;
            normal_float_array.Values[2] = 1.0f;
            normal_float_array.Values[3] = 0.0f;
            normal_float_array.Values[4] = 0.0f;
            normal_float_array.Values[5] = 1.0f;
            normal_float_array.Values[6] = 0.0f;
            normal_float_array.Values[7] = 0.0f;
            normal_float_array.Values[8] = 1.0f;

            normal_technique_common.accessor = new accessor();
            normal_technique_common.accessor.param = new param[3];
            normal_technique_common.accessor.param[0] = new param();
            normal_technique_common.accessor.param[1] = new param();
            normal_technique_common.accessor.param[2] = new param();
            normal_technique_common.accessor.source = "#" + normal_float_array.id;
            normal_technique_common.accessor.count = 3;
            normal_technique_common.accessor.stride = 3;
            normal_technique_common.accessor.param[0].name = "X";
            normal_technique_common.accessor.param[0].type = "float";
            normal_technique_common.accessor.param[1].name = "Y";
            normal_technique_common.accessor.param[1].type = "float";
            normal_technique_common.accessor.param[2].name = "Z";
            normal_technique_common.accessor.param[2].type = "float";

            uv_float_array.id = "Plane001-UV0-array";
            uv_float_array.count = 6;
            uv_float_array.Values = new double[uv_float_array.count];
            uv_float_array.Values[0] = 1.0000f;
            uv_float_array.Values[1] = 0.0000f;
            uv_float_array.Values[2] = 0.0000f;
            uv_float_array.Values[3] = 0.0000f;
            uv_float_array.Values[4] = 1.0000f;
            uv_float_array.Values[5] = 1.0000f;

            uv_technique_common.accessor = new accessor();
            uv_technique_common.accessor.param = new param[2];
            uv_technique_common.accessor.param[0] = new param();
            uv_technique_common.accessor.param[1] = new param();
            uv_technique_common.accessor.source = "#" + uv_float_array.id;
            uv_technique_common.accessor.count = 3;
            uv_technique_common.accessor.stride = 2;
            uv_technique_common.accessor.param[0].name = "S";
            uv_technique_common.accessor.param[0].type = "float";
            uv_technique_common.accessor.param[1].name = "T";
            uv_technique_common.accessor.param[1].type = "float";

            geomMesh.source[0].id = "Plane001-POSITION";
            geomMesh.source[0].Item = position_float_array;
            geomMesh.source[0].technique_common = position_technique_common;
            geomMesh.source[1].id = "Plane001-Normal0";
            geomMesh.source[1].Item = normal_float_array;
            geomMesh.source[1].technique_common = normal_technique_common;
            geomMesh.source[2].id = "Plane001-UV0";
            geomMesh.source[2].Item = uv_float_array;
            geomMesh.source[2].technique_common = uv_technique_common;

            geomMesh.vertices = new vertices();
            geomMesh.vertices.input[0] = new InputLocal();
            geomMesh.vertices.input[0].semantic = "POSITION";
            geomMesh.vertices.input[0].source = "#Plane001-POSITION";
            geomMesh.vertices.id = "Plane001-VERTEX";

            triangles meshTriangle = new triangles();
            meshTriangle.count = 1;
            meshTriangle.input = new InputLocalOffset[3];
            meshTriangle.input[0] = new InputLocalOffset();
            meshTriangle.input[1] = new InputLocalOffset();
            meshTriangle.input[2] = new InputLocalOffset();
            meshTriangle.input[0].semantic = "VERTEX";
            meshTriangle.input[0].offset = 0;
            meshTriangle.input[0].source = "#Plane001-VERTEX";
            meshTriangle.input[1].semantic = "NORMAL";
            meshTriangle.input[1].offset = 1;
            meshTriangle.input[1].source = "#Plane001-Normal0";
            meshTriangle.input[2].semantic = "TEXCOORD";
            meshTriangle.input[2].offset = 2;
            meshTriangle.input[2].set = 0;
            meshTriangle.input[2].source = "#Plane001-UV0";

            string p = "";
            int[] pArray = new int[9];
            pArray[0] = 0;
            pArray[1] = 0;
            pArray[2] = 0;
            pArray[3] = 1;
            pArray[4] = 1;
            pArray[5] = 1;
            pArray[6] = 2;
            pArray[7] = 2;
            pArray[8] = 2;
            foreach (var data in pArray)
            {
                if (data is int)
                {
                    p += " " + data.ToString();
                }
            }
            meshTriangle.p = p;

            geomMesh.Items = new object[1];
            geomMesh.Items[0] = new object();
            geomMesh.Items[0] = meshTriangle;

            geom.Item = geomMesh;
            geom.id = "Plane001-lib";
            geom.name = "Plane001Mesh";
            library_geom.geometry[0] = geom;

            // library_visual_scenes
            library_visual_scenes lib_visual_scene = new library_visual_scenes();
            lib_visual_scene.visual_scene = new visual_scene[1];
            lib_visual_scene.visual_scene[0] = new visual_scene();

            visual_scene visaul = new visual_scene();
            visaul.node = new node[1];
            visaul.node[0] = new node();
            visaul.node[0].name = "Triangle001";
            visaul.node[0].id = "Triangle001";
            visaul.node[0].sid = "Triangle001";

            visaul.node[0].instance_geometry = new instance_geometry[1];
            visaul.node[0].instance_geometry[0] = new instance_geometry();
            visaul.node[0].instance_geometry[0].url = "#Plane001-lib";

            visaul.node[0].extra = new extra[1];
            visaul.node[0].extra[0] = new extra();
            visaul.node[0].extra[0].technique = new technique[1];
            visaul.node[0].extra[0].technique[0] = new technique();
            visaul.node[0].extra[0].technique[0].profile = "FCOLLADA";

            visaul.name = "cube";
            visaul.id = "cube";

            lib_visual_scene.visual_scene[0] = visaul;

            model.Items = new object[2];
            model.Items[0] = library_geom;
            model.Items[1] = lib_visual_scene;

            model.scene = new COLLADAScene();
            model.scene.instance_visual_scene = new InstanceWithExtra();
            model.scene.instance_visual_scene.url = "#" + visaul.id;

            model.Save("C:\\ProgramData\\Autodesk\\revit\\Addins\\2020\\dd.dae");
            return 0;
        }
    }
}
