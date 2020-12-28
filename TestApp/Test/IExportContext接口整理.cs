#region 程序集 RevitAPI, Version=20.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Program Files\Autodesk\Revit 2020\RevitAPI.dll
#endregion


namespace Autodesk.Revit.DB
{
    //
    // 摘要:
    //     An interface that is used in custom export to process a Revit model.
    //
    // 言论：
        // An instance of a class that implements this interface is passed in as a parameter
        // of the Autodesk.Revit.DB.CustomExporter constructor. The methods of the context
        // are then called at times of exporting entities of the model. This is a base class
        // for two other interfaces derived from it: Autodesk.Revit.DB.IPhotoRenderContext
        // and Autodesk.Revit.DB.IModelExportContext. This base class contains methods that
        // are common to both the leaf interfaces. Although it is still possible to use
        // classes deriving directly from this base interface (for backward compatibility),
        // future applications should implement the new leaf interfaces only.

        // 实现此接口的类的实例作为Autodesk.Revit.DB.CustomExporter构造函数的参数传入。 
        // 然后在导出模型实体时调用上下文的方法。 这
        // 是从其派生的其他两个接口的基类：Autodesk.Revit.DB.IPhotoRenderContext和Autodesk.Revit.DB.IModelExportContext。 
        // 该基类包含两个叶接口都通用的方法。 
        // 尽管仍然可以使用直接从此基本接口派生的类（为了向后兼容），但将来的应用程序应仅实现新的叶子接口。
        
    public interface IExportContext
    {
        //
        // 摘要:
        //     This method is called at the very end of the export process, after all entities
        //     were processed (or after the process was cancelled).
        //     在处理完所有实体之后（或取消处理之后），在导出过程的最后将调用此方法。
        void Finish();



        //
        // 摘要:
        //     This method is queried at the beginning of every element.
        //     在每个元素的开头都会查询此方法。
        //
        // 返回结果:
        //     Return True if you wish to cancel the exporting process, or False otherwise.
        //     如果要取消导出过程，则返回True，否则返回False。
        //
        bool IsCanceled();

        

        //
        // 摘要:
        //     This method marks the beginning of an element to be exported.
        //     此方法标记要导出的元素的开始。
        //
        // 参数:
        //   elementId:
        //     The Id of the element that is about to be processed.
        //     即将处理的元素的ID。
        //
        // 返回结果:
        //     Return RenderNodeAction.Skip if you wish to skip exporting this element, or return
        //     RenderNodeAction.Proceed otherwise.
        //     如果要跳过导出此元素，请返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。
        //
        // 言论：
        //     This method is never called for 2D export (see cref="Autodesk::Revit::DB::IExportContext2D").
        //     永远不要为2D导出调用此方法（请参阅cref =“ Autodesk :: Revit :: DB :: IExportContext2D”）。
        //
        RenderNodeAction OnElementBegin(ElementId elementId);



        //
        // 摘要:
        //     This method marks the end of an element being exported.
        //     此方法标记要导出的元素的结尾。
        //
        // 参数:
        //   elementId:
        //     The Id of the element that has just been processed.
        //     刚刚处理过的元素的ID。
        //
        // 言论：
        //     This method is never called for 2D export (see cref="Autodesk::Revit::DB::IExportContext2D").
        void OnElementEnd(ElementId elementId);



        //
        // 摘要:
        //     This method marks the beginning of a Face to be exported.
        //     此方法标记要导出的Face的开始。
        //
        // 参数:
        //   node:
        //     An output node that represents a Face.
        //     表示面的输出节点。
        //
        // 返回结果:
        //     Return RenderNodeAction. Proceed if you wish to receive geometry (polymesh) for
        //     this face, or return RenderNodeAction.Skip otherwise.
        //     返回RenderNodeAction。 如果希望接收此面的几何图形（多边形），请继续，否则返回RenderNodeAction.Skip。
        //
        // 言论：
        //     Note that this method (as well as OnFaceEnd) is invoked only if the custom exporter
        //     was set up to include geometric objects in the output stream. See Autodesk.Revit.DB.CustomExporter.IncludeGeometricObjects
        //     for mode details.
        //     请注意，仅当将自定义导出器设置为在输出流中包含几何对象时，才调用此方法（以及OnFaceEnd）。 
        //     有关模式详细信息，请参见Autodesk.Revit.DB.CustomExporter.IncludeGeometricObjects。
        RenderNodeAction OnFaceBegin(FaceNode node);



        //
        // 摘要:
        //     This method marks the end of the current face being exported.
        //     此方法标记要导出的当前面的末端。
        //
        // 参数:
        //   node:
        //     An output node that represents a Face.
        //     表示面的输出节点。
        void OnFaceEnd(FaceNode node);



        //
        // 摘要:
        //     This method marks the beginning of a family instance to be exported.
        //     此方法标记了要导出的族实例的开始。
        //
        // 返回结果:
        //     Return RenderNodeAction.Skip if you wish to skip processing this family instance,
        //     or return RenderNodeAction.Proceed otherwise.
        //     如果您希望跳过处理此族实例，则返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。
        RenderNodeAction OnInstanceBegin(InstanceNode node);



        //
        // 摘要:
        //     This method marks the end of a family instance being exported.
        //     此方法标志着要导出的族实例的结尾。
        //
        // 参数:
        //   node:
        //     An output node that represents a family instance.
        //     表示族实例的输出节点。
        void OnInstanceEnd(InstanceNode node);



        //
        // 摘要:
        //     This method marks the beginning of export of a light which is enabled for rendering.
        //     此方法标记了已启用渲染的灯光的开始输出。
        //
        // 参数:
        //   node:
        //     A node describing the light object.
        //     描述灯光对象的节点。
        //
        // 言论：
        //     This method is only called for photo-rendering export (a custom exporter that
        //     implements Autodesk.Revit.DB.IPhotoRenderContext).
        //     仅对照片渲染导出（实现Autodesk.Revit.DB.IPhotoRenderContext的自定义导出器）调用此方法。
        void OnLight(LightNode node);



        //
        // 摘要:
        //     This method marks the beginning of a link instance to be exported.
        //     此方法标记要导出的链接实例的开始。
        //
        // 返回结果:
        //     Return RenderNodeAction.Skip if you wish to skip processing this link instance,
        //     or return RenderNodeAction.Proceed otherwise.
        //     如果您希望跳过处理此链接实例，则返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。
        RenderNodeAction OnLinkBegin(LinkNode node);



        //
        // 摘要:
        //     This method marks the end of a link instance being exported.
        //     此方法标记要导出的链接实例的结尾。
        //
        // 参数:
        //   node:
        //     An output node that represents a Revit link.
        //     表示Revit链接的输出节点。
        void OnLinkEnd(LinkNode node);



        //
        // 摘要:
        //     This method marks a change of the material.
        //     这种方法标志着材料的变化。
        //
        // 参数:
        //   node:
        //     A node describing the current material.
        //     描述当前材料的节点。
        void OnMaterial(MaterialNode node);



        //
        // 摘要:
        //     This method is called when a tessellated polymesh of a 3d face is being output.
        //     当输出3d面的镶嵌多边形时，将调用此方法。
        //
        // 参数:
        //   node:
        //     A node representing topology of the polymesh
        //     表示多边形网格拓扑的节点
        void OnPolymesh(PolymeshTopology node);


        
        //
        // 摘要:
        //     This method marks the beginning of export of an RPC object.
        //     此方法标志着RPC对象导出的开始。
        //
        // 参数:
        //   node:
        //     A node with asset information about the RPC object.
        //     具有有关RPC对象的资产信息的节点。
        //
        // 言论：
        //     This method is only called for photo-rendering export (a custom exporter that
        //     implements Autodesk.Revit.DB.IPhotoRenderContext). When an RPC object is encountered
        //     for a model context export (a custom exporter that implements Autodesk.Revit.DB.IModelExportContext),
        //     the RPC object will be provided as a polymesh (via Autodesk.Revit.DB.IExportContext.OnPolymesh(Autodesk.Revit.DB.PolymeshTopology)).
        //     仅对照片渲染导出（实现Autodesk.Revit.DB.IPhotoRenderContext的自定义导出器）调用此方法。 
        //     当模型上下文导出（实现Autodesk.Revit.DB.IModelExportContext的自定义导出器）遇到RPC对象时，
        //     RPC对象将作为多边形提供（通过Autodesk.Revit.DB.IExportContext.OnPolymesh（Autodesk.Revit  .DB.PolymeshTopology）。
        void OnRPC(RPCNode node);



        //
        // 摘要:
        //     This method marks the beginning of a 3D view to be exported.
        //     此方法标记要导出的3D视图的开始。
        //
        // 参数:
        //   node:
        //     Geometry node associated with the view.
        //     与视图关联的几何节点。
        //
        // 返回结果:
        //     Return RenderNodeAction.Skip if you wish to skip exporting this view, or return
        //     RenderNodeAction.Proceed otherwise.
        //     如果要跳过导出此视图，则返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。
        RenderNodeAction OnViewBegin(ViewNode node);


        
        //
        // 摘要:
        //     This method marks the end of a 3D view being exported.
        //     此方法标记要导出的3D视图的结束。
        //
        // 参数:
        //   elementId:
        //     The Id of the 3D view that has just been processed.
        //     刚刚处理过的3D视图的ID。
        void OnViewEnd(ElementId elementId);



        //
        // 摘要:
        //      This method is called at the very start of the export process, still before the
        //      first entity of the model was send out.
        //      在导出过程的最开始即在发送模型的第一个实体之前就调用此方法。
        //
        // 返回结果:
        //     Return True if you are ready to proceed with processing the export.
        //     如果您准备继续进行导出，则返回True。
        bool Start();
    }
}