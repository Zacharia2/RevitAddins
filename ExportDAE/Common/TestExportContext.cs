using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ExportDAE
{
    class TestExportContext : IExportContext
    {

        private Stack<ElementId> elementStack = new Stack<ElementId>();
        private Stack<Document> documentStack = new Stack<Document>();
        private Document document;
        public TestExportContext(Document document)
        {
            this.document = document;
        }


        void IExportContext.Finish()
        {
            MessageBox.Show("Finish写文件完成。", "Finish");
        }

        bool IExportContext.IsCanceled()
        {
            return false;
        }

        RenderNodeAction IExportContext.OnElementBegin(ElementId elementId)
        {
            elementStack.Push(elementId);
            Element element = this.document.GetElement(elementId);
            MessageBox.Show("名字：" + element.Name + "        ID：" + element.Id, "OnElementBegin");

            return RenderNodeAction.Proceed;
        }

        void IExportContext.OnElementEnd(ElementId elementId)
        {
            MessageBox.Show("OnElement方法结束");
        }

        RenderNodeAction IExportContext.OnFaceBegin(FaceNode node)
        {
            Face a = node.GetFace();
            Element e = document.GetElement(a.MaterialElementId);
            MessageBox.Show("名字：" + e.Name + "         ID：" + e.Id, "OnFaceBegin");
            return RenderNodeAction.Proceed;
        }

        void IExportContext.OnFaceEnd(FaceNode node)
        {
            MessageBox.Show("OnFaceEnd方法结束");
        }

        RenderNodeAction IExportContext.OnInstanceBegin(InstanceNode node)
        {
            Transform t = node.GetTransform();
            MessageBox.Show(node.NodeName + "：" + t.Scale, "OnInstanceBegin");
           
            return RenderNodeAction.Proceed;
        }

        void IExportContext.OnInstanceEnd(InstanceNode node)
        {
            MessageBox.Show("OnInstanceEnd方法结束");

        }

        void IExportContext.OnLight(LightNode node)
        {
            MessageBox.Show("OnLight,HashCode：" + node.GetHashCode() +"     "+ node.ToString(), node.NodeName);
        }

        RenderNodeAction IExportContext.OnLinkBegin(LinkNode node)
        {
            MessageBox.Show("OnLinkBegin,HashCode：" + node.GetHashCode(), node.NodeName);
            return RenderNodeAction.Proceed;
        }

        void IExportContext.OnLinkEnd(LinkNode node)
        {
            MessageBox.Show("OnLinkEnd,HashCode：" + node.GetHashCode(), node.NodeName);
        }

        void IExportContext.OnMaterial(MaterialNode node)
        {
            MessageBox.Show("标识材质的默认呈现外观是否被覆盖:" + node.HasOverriddenAppearance +  "\n材质名称：" + node.ThumbnailFile , "OnMaterial：" + node.NodeName);
        }

        void IExportContext.OnPolymesh(PolymeshTopology node)
        {
           
            MessageBox.Show(String.Format("NumberOfFacets:{0}，NumberOfNormals：{1}，NumberOfPoints{2}，NumberOfUVs{3}：", node.NumberOfFacets, node.NumberOfNormals, node.NumberOfPoints, node.NumberOfUVs), "OnPolymesh：" + node.GetHashCode());
        }   

        void IExportContext.OnRPC(RPCNode node)
        {
            throw new NotImplementedException();
        }

        RenderNodeAction IExportContext.OnViewBegin(ViewNode node)
        {
            Element element = document.GetElement(node.ViewId);
            MessageBox.Show("名字：" + element.Name + "           ID：" + element.Id, "OnViewBegin");

            return RenderNodeAction.Proceed;
        }

        void IExportContext.OnViewEnd(ElementId elementId)
        {
            Element element = document.GetElement(elementId);
            MessageBox.Show("名字：" + element.Name + "           ID：" + element.Id, "OnViewEnd方法结束");
        }

        bool IExportContext.Start()
        {
            documentStack.Clear();
            documentStack.Push(document);
            MessageBox.Show("Start方法开始", "Starting");

            return true;
        }
    }
}
