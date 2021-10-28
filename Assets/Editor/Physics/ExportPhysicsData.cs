//using System.Collections.Generic;
//using System.Xml;
//using System.Linq;
//using UnityEngine;

//public unsafe class ExportPhysicsData
//{
//    public string OutputPath;
//    public GameObject GameObjectWithModelInfo;
//    void OnWizardCreate()
//    {
//        Export();
//    }

//    public ExportPhysicsData()
//    {

//        CollisionShapeBlock = new List<CollisionShapeData>();
//        RigidBodyBlock = new List<RigidBodyData>();
//        ConstraintBlock = new List<SphericalConstraintData>();
//        RigidBodyIndeces = new int[128];
//        for (int i = 0; i < RigidBodyIndeces.Length; i++)
//        {
//            RigidBodyIndeces[i] = -1;
//        }
//    }
//    #region Blocks
//    List<CollisionShapeData> CollisionShapeBlock { get; }
//    List<RigidBodyData> RigidBodyBlock { get; }
//    List<SphericalConstraintData> ConstraintBlock { get; }
//    int[] RigidBodyIndeces { get; }
//    #endregion
//    public void Export()
//    {
//        GameObject modelObject = GameObjectWithModelInfo;
//        foreach (Transform i in modelObject.GetComponentsInChildren<Transform>())
//        {
//            // process each bone
//            var bone = i.gameObject;
//            if(bone.GetComponent<GRigidbody>())
//                ProcessBone(bone);
//        }
//        WriteToGPD();
//    }
//    /// <summary>
//    /// StudioBone/RigidBody
//    /// </summary>
//    /// <param name="node"></param>
//    private void ProcessNode(GameObject node)
//    {
//        var rigidbody = node.GetComponent<GRigidbody>();
//        if (rigidbody != null)
//        {
//            ProcessGRigidBody(node);
//        }
//    }
//    private void ProcessBone(GameObject gameObject)
//    {
//        int shapeBlockIndex = CollisionShapeBlock.Count;
//        var shapeComponents = gameObject.GetComponents<GCollisionShape>();
//        CollisionShapeBlock.Add(new CollisionShapeData()
//        {
//            Name = gameObject.name,
//            SubShape = new List<GCollisionShape>(shapeComponents)
//        });

//        int rigidBlockIndex = RigidBodyBlock.Count;
//        var rigidComponent = gameObject.GetComponent<GRigidbody>();
//        RigidBodyBlock.Add(new RigidBodyData()
//        {
//            BoneIndex = rigidComponent.BoneIndex,
//            ColliderDataIndex=shapeBlockIndex,
//             IsAttachment=rigidComponent.IsAttachment,
//              MaterialData=-1,
//               WorldTransform=rigidComponent.LocalTransform
//        });
//        RigidBodyIndeces[rigidComponent.BoneIndex] = rigidBlockIndex;

//        var constraintComponent = gameObject.GetComponent<GConstraint>();
//        if(constraintComponent!=null)
//        {
//            var constraintWorldTransform = constraintComponent.WorldTransform;
//            var rigidBodyATransform = constraintComponent.Target.WorldTransform;
//            var rigidBodyBTransform = rigidComponent.WorldTransform;

//            var localInA = rigidBodyATransform.inverse * constraintWorldTransform;
//            var localInB = rigidBodyBTransform.inverse * constraintWorldTransform;

//            var targetRigidIndexInBlock = RigidBodyIndeces[constraintComponent.Target.BoneIndex];

//            ConstraintBlock.Add(new SphericalConstraintData()
//            {
//                LocalFrameInA = localInA,
//                LocalFrameInB = localInB,
//                RigidBodyDataIndexA = targetRigidIndexInBlock,
//                RigidBodyDataIndexB = rigidBlockIndex
//            });

//        }

//    }
    
//    private void ProcessGRigidBody(GameObject rigidbodyObject)
//    {
//        //int colliderIndex = CollisionShapeBlock.Count;
//        //var shape = rigidbodyObject.GetComponent<GBoxShape>();
//        //CollisionShapeBlock.Add(new CollisionShapeData()
//        //{
//        //    SubShape = new List<(GBoxShape Shape, Matrix4x4 LocalMatrix)>()
//        //     {
//        //         (shape,Matrix4x4.identity)
//        //     }
//        //});

//        //int rigidBodyIndex = RigidBodyBlock.Count;
//        //var rigidBody = rigidbodyObject.GetComponent<GRigidbody>();
//        //var bone = rigidbodyObject.transform.parent.gameObject.GetComponent<StudioBone>();
//        //RigidBodyBlock.Add(new RigidBodyData()
//        //{
//        //    BoneIndex = bone.Index,
//        //    ColliderDataIndex = colliderIndex,
//        //    IsAttachment = rigidBody.IsAttachment,
//        //    WorldTransform = GConstant.RebaseMatrix * rigidBody.WorldTransform
//        //});
//        //// 储存当前骨骼刚体数据的索引给未来的约束引用
//        //RigidBodyIndeces[bone.Index] = rigidBodyIndex;

//        //var constraint = rigidbodyObject.GetComponent<GConstraint>();
//        //if (constraint != null)
//        //{
//        //    var constraintWorldTransform = constraint.WorldTransform;
//        //    var rigidBodyATransform = constraint.Target.WorldTransform;
//        //    var rigidBodyBTransform = rigidBody.WorldTransform;

//        //    var localInA = rigidBodyATransform.inverse * constraintWorldTransform;
//        //    var localInB = rigidBodyBTransform.inverse * constraintWorldTransform;

//        //    var targetStudioBoneIndex = constraint.Target.transform.parent.gameObject.GetComponent<StudioBone>().Index;
//        //    var targetRigidIndexInBlock = RigidBodyIndeces[targetStudioBoneIndex];
//        //    Debug.Assert(targetRigidIndexInBlock != -1);
//        //    ConstraintBlock.Add(new SphericalConstraintData()
//        //    {
//        //        LocalFrameInA = localInA,
//        //        LocalFrameInB = localInB,
//        //        RigidBodyDataIndexA = targetRigidIndexInBlock,
//        //        RigidBodyDataIndexB = rigidBodyIndex,
//        //    });
//        //}
//    }

//    private void WriteToGPD()
//    {
//        XmlDocument gpd = new XmlDocument();
//        // xml header
//        gpd.AppendChild(gpd.CreateXmlDeclaration("1.0", "utf-8", null));

//        // root
//        XmlElement gpdRoot = gpd.CreateElement("goldsrc-physics-data");
//        gpdRoot.SetAttribute("version", "1.0");
//        gpd.AppendChild(gpdRoot);

//        // collision shape block
//        XmlElement shapes = gpd.CreateElement("collision-shape-block");
//        foreach (var i in CollisionShapeBlock)
//        {
//            XmlElement shape = gpd.CreateElement("collision-shape");
//            foreach (var j in i.SubShape)
//            {
//                XmlElement subShape = gpd.CreateElement("sub-collision-shape");
//                // set shape
//                {
//                    if(j is GBoxShape)
//                    {
//                        var box = j as GBoxShape;
//                        subShape.SetAttribute("type", "primitive.box");
//                        subShape.AppendChild(MatrixElement(gpd, box.LocalTransform));
//                        subShape.AppendChild(VectorElement(gpd, box.HalfExtent));

//                    }else if(j is object)
//                    {

//                    }
                    
//                }
//                shape.AppendChild(subShape);
//            }
//            shapes.AppendChild(shape);
//        }
//        gpdRoot.AppendChild(shapes);

//        // rigid body block
//        XmlElement rigidBodys = gpd.CreateElement("rigidbody-block");
//        gpdRoot.AppendChild(rigidBodys);
//        foreach (var i in RigidBodyBlock)
//        {
//            XmlElement rigidBody = gpd.CreateElement("rigidbody");
//            rigidBodys.AppendChild(rigidBody);
//            rigidBody.SetAttribute("bone", i.BoneIndex.ToString());
//            rigidBody.SetAttribute("shape", i.ColliderDataIndex.ToString());
//            rigidBody.SetAttribute("type", (i.IsAttachment ? "1" : "0"));
//            rigidBody.AppendChild(MatrixElement(gpd,i.WorldTransform));
//        }

//        // constraint block
//        XmlElement constraints = gpd.CreateElement("constraint-block");
//        gpdRoot.AppendChild(constraints);
//        foreach (var i in ConstraintBlock)
//        {
//            XmlElement constraint = gpd.CreateElement("constraint");
//            constraints.AppendChild(constraint);
//            constraint.SetAttribute("rigidbody-a", i.RigidBodyDataIndexA.ToString());
//            constraint.SetAttribute("rigidbody-b", i.RigidBodyDataIndexB.ToString());
//            var localA = MatrixElement(gpd, i.LocalFrameInA);
//            localA.SetAttribute("name", "local-frame-a");
//            var localB = MatrixElement(gpd, i.LocalFrameInB);
//            localB.SetAttribute("name", "local-frame-b");
//            constraint.AppendChild(localA);
//            constraint.AppendChild(localB);
//        }
//        // xml save
//        gpd.Save(OutputPath);

//    }
//    private XmlElement MatrixElement(XmlDocument doc, Matrix4x4 m)
//    {
//        XmlElement matrixElement = doc.CreateElement("matrix44");
//        string text = string.Empty;
//        text += "\n";
//        for (int i = 0; i < 4; i++)
//        {
//            var row = m.GetRow(i);
//            text += string.Format("{0,-10} {1,-10} {2,-10} {3,-10}\n", row.x, row.y, row.z, row.w);
//        }
//        matrixElement.InnerText = text;
//        return matrixElement;
//    }
//    private XmlElement VectorElement(XmlDocument doc, Vector3 v)
//    {
//        XmlElement vectorElement = doc.CreateElement("vector3");
//        vectorElement.InnerText = string.Format("{0,-10} {1,-10} {2,-10}", v.x, v.y, v.z);
//        return vectorElement;
//    }
//    private void WriteBoxShape(XmlDocument gpd)
//    {

//    }
//}
//#region gpd data structure
///// <summary>
///// 
///// </summary>
//public class CollisionShapeData
//{
//    public string Name { get; set; }
//    public List<GCollisionShape> SubShape { get; set; }

//}
//public class RigidBodyData
//{
//    public int BoneIndex { get; set; }
//    public int ColliderDataIndex { get; set; }
//    public bool IsAttachment { get; set; }
//    public int MaterialData { get; set; }
//    public Matrix4x4 WorldTransform { get; set; }
//}
///// <summary>
///// aka Point2PointConstraint
///// </summary>
//public class SphericalConstraintData
//{
//    public int RigidBodyDataIndexA { get; set; }
//    public int RigidBodyDataIndexB { get; set; }
//    public Matrix4x4 LocalFrameInA { get; set; }
//    public Matrix4x4 LocalFrameInB { get; set; }
//}
//#endregion
