using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using GanyuEditor.Extensions;

/*
 * 
                #region test local

                var shapeWorldTrans = subShapes.First().WorldTransform;
                var boneWorldTrans = boneObject.transform.localToWorldMatrix;
                var local = boneWorldTrans.inverse * shapeWorldTrans;
                var localGoldsrc = boneWorldTrans.ToGoldsrc().inverse * shapeWorldTrans.ToGoldsrc();
                Debug.Log(local.Equal(localGoldsrc.ToGoldsrc()));// true
 */
namespace GanyuEditor.Physics
{

    public class PhysicsDataExporter
    {
        public string OutputPath { get; set; }
        public GameObject ModelRoot { get; set; }


        public void Export(GameObject modelRoot,string outputPath)
        {
            OutputPath = outputPath;
            ModelRoot = modelRoot;
            Export();
        }

        public void Export()
        {
            if(Validate())
            {
                _doc = new XmlDocument();
                // xml header
                _doc.AppendChild(_doc.CreateXmlDeclaration("1.0", "utf-8", null));

                // gpd
                var gpd = _doc.CreateElement("goldsrc-physics-data");
                gpd.SetAttribute("version", "2.0");
                gpd.SetAttribute("checksum", CalcHdrChecksum());
                _doc.AppendChild(gpd);

                // 3 blocks
                _shapeBlock = gpd.AppendChild(_doc.CreateElement("collision-shape-block")) as XmlElement;
                _rigidBlock = gpd.AppendChild(_doc.CreateElement("rigidbody-block"))       as XmlElement;
                _constBlock = gpd.AppendChild(_doc.CreateElement("constraint-block"))      as XmlElement;

                foreach (var i in ModelRoot.GetComponentsInChildren<Transform>())
                {
                    if(i.GetComponent<GanyuEditor.Physics.Rigidbody>())
                    {
                        ProcessPhysicalBone(i.gameObject);
                    }
                }
                _doc.Save(OutputPath);
            }
        }
        private XmlDocument _doc;
        private XmlElement _shapeBlock;
        private XmlElement _rigidBlock;
        private XmlElement _constBlock;

        // 如果只有一个 shape 的话，则为 shape 的local transform，
        // 如果是multishape，则为 identity。
        // shpae count elements
        private List<Matrix4x4> _offsets=new List<Matrix4x4>();
        private int _rigidCount = 0;
        private List<Matrix4x4> _rigidTransGoldsrcList = new List<Matrix4x4>();
        private int[] _rigidbodyIndeces = new int[128];
        private string CalcHdrChecksum()
        {
            return ModelRoot.GetComponent<ModelInfo>().Checksum;
        }
        private void ProcessPhysicalBone(GameObject boneObject)
        {
            var studioBone = boneObject.GetComponent<StudioBone>();
            int boneIndex = studioBone.Index;
            int shapeIndex = _offsets.Count;
            int rigidIndex = _rigidCount++;
            _rigidbodyIndeces[boneIndex] = rigidIndex;

            // append shape
            var subShapes = boneObject.GetComponents<CollisionShape>();
            var shapeElement = _doc.CreateElement("collision-shape");
            _shapeBlock.AppendChild(shapeElement);

            // insert sub shape to shape element
            if (subShapes.Length>1)
            {
                _offsets.Add(Matrix4x4.identity);
                for (int i = 0; i < subShapes.Length; i++)
                {
                    var subElement = GetSubShapeElement(subShapes[i]);
                    shapeElement.AppendChild(subElement);
                    var childOffset = studioBone.WorldTransform.ToGoldsrc().inverse * subShapes[i].WorldTransform.ToGoldsrc();
                    subElement.AppendField("local", childOffset);
                }
            }
            else
            {
                _offsets.Add(studioBone.WorldTransform.ToGoldsrc().inverse * subShapes.First().WorldTransform.ToGoldsrc());
                var subElement = GetSubShapeElement(subShapes.First());
                shapeElement.AppendChild(subElement);
            }

            // append rigidbody
            var rigidCompo = boneObject.GetComponent<GanyuEditor.Physics.Rigidbody>();
            var rigidElement = _doc.CreateElement("rigidbody");
            _rigidBlock.AppendChild(rigidElement);

            rigidElement.SetAttribute("bone", boneIndex.ToString());
            rigidElement.SetAttribute("shape", shapeIndex.ToString());
            rigidElement.SetAttribute("type", rigidCompo.IsAttachment ? "1" : "0");
            rigidElement.AppendField("local", _offsets.Last());

            var rigidTransGoldsrc = studioBone.WorldTransform.ToGoldsrc() * _offsets.Last();
            _rigidTransGoldsrcList.Add(rigidTransGoldsrc);

            // append constraint
            // rigidbody b is the connected body
            // sometimes body a connect to the world. (no body b)
            var constCompo = boneObject.GetComponent<Constraint>();
            if(constCompo)
            {
                var constElement = _doc.CreateElement("constraint");
                _constBlock.AppendChild(constElement);

                if(constCompo.ConnectedBody == null)
                {
                    throw new NotImplementedException();
                    //constraint1();
                }
                else
                {
                    constraint2();
                }
                //void constraint1()
                //{
                //}
                void constraint2()
                {
                    var rbaIndex = _rigidbodyIndeces[boneIndex];
                    var rbbIndex = _rigidbodyIndeces[constCompo.ConnectedBody.BoneIndex];
                    var locala = _rigidTransGoldsrcList[rbaIndex].inverse * constCompo.WorldTransform.ToGoldsrc();
                    var localb = _rigidTransGoldsrcList[rbbIndex].inverse * constCompo.WorldTransform.ToGoldsrc();

                    constElement.SetAttribute("rba", rbaIndex.ToString());
                    constElement.SetAttribute("rbb", rbbIndex.ToString());
                    constElement.AppendField("locala", locala);
                    constElement.AppendField("localb", localb);

                    if (constCompo is SphericalConstraint)
                    {
                        constElement.SetAttribute("type", "spherical");
                    }
                    else if (constCompo is ConeTwistConstraint)
                    {
                        var cone = constCompo as ConeTwistConstraint;
                        constElement.SetAttribute("type", "cone");

                        constElement.AppendField("twistspan", cone.TwistSpan < 0 ? 0 : cone.TwistSpan);
                        constElement.AppendField("swingspan1", cone.SwingSpan1 < 0 ? 0 : cone.SwingSpan1);
                        constElement.AppendField("swingspan2", cone.SwingSpan2 < 0 ? 0 : cone.SwingSpan2);
                    }
                    else if (constCompo is HingeConstraint)
                    {
                        var hinge = constCompo as HingeConstraint;
                        constElement.SetAttribute("type", "hinge");

                        constElement.AppendField("low", hinge.Low);
                        constElement.AppendField("high", hinge.High);
                    }
                    else
                    {
                        throw new NotSupportedException($"unknow constraint {constCompo}.");
                    }
                }
            }
        }

        private XmlElement GetSubShapeElement(CollisionShape collisionShape)
        {
            var result = _doc.CreateElement("sub-collision-shape");

            if(collisionShape is BoxShape)
            {
                var box = collisionShape as BoxShape;
                result.SetAttribute("type", "primitive.box");
                result.AppendField("halfextent", new Vector3(box.HalfExtent.x, box.HalfExtent.z, box.HalfExtent.y));
            }
            else if(collisionShape is CapsuleShape)
            {
                var capsule = collisionShape as CapsuleShape;
                result.SetAttribute("type", "primitive.capsule");
                result.AppendField("radius", capsule.Radius);
                result.AppendField("height", capsule.Height);
            }
            else
            {
                throw new NotImplementedException($"{collisionShape} is not support yet.");
            }
            return result;
        }

        private bool Validate()
        {
            foreach (var i in ModelRoot.GetComponentsInChildren<Transform>())
            {
                if (i.GetComponent<ModelInfo>())
                    break;
                if(i.GetComponent<StudioBone>()==null)
                {
                    Debug.LogError($"Missing {nameof(StudioBone)} component at {i.name}.");
                    return false;
                }
            }
            return true;
        }
        
    }
    internal static class ExporterHelper
    {
        public static void AppendField(this XmlElement self, string fieldName,Matrix4x4 value)
        {
            XmlElement matrixElement = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(matrixElement);
            string text = string.Empty;
            text += "\n";
            for (int i = 0; i < 4; i++)
            {
                var row = value.GetRow(i);
                text += string.Format("{0,-10} {1,-10} {2,-10} {3,-10}\n", row.x, row.y, row.z, row.w);
            }
            matrixElement.InnerText = text;
        }
        public static void AppendField(this XmlElement self, string fieldName, Vector3 value)
        {
            XmlElement vectorElement = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(vectorElement);
            vectorElement.InnerText = string.Format("{0,-10} {1,-10} {2,-10}", value.x, value.y, value.z);
        }
        public static void AppendField(this XmlElement self, string fieldName, float value)
        {
            XmlElement e = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(e);
            e.InnerText = value.ToString();
        }
        public static void AppendField(this XmlElement self, string fieldName, int value)
        {
            XmlElement e = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(e);
            e.InnerText = value.ToString();
        }
    }
}
