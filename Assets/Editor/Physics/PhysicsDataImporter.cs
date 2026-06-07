using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using GanyuEditor.Extensions;
using UnityEngine;

namespace GanyuEditor.Physics
{
    public class PhysicsDataImporter
    {
        private XmlDocument _doc;
        private GameObject _modelRoot;
        private Dictionary<int, StudioBone> _bones;
        private readonly List<Matrix4x4> _rigidWorldGoldsrc = new List<Matrix4x4>();
        private readonly List<PhysicsBody> _rigidBodies = new List<PhysicsBody>();
        private int _shapeCount;
        private int _rigidbodyCount;
        private int _constraintCount;


        public static bool TryImportSameDirectory(GameObject modelRoot)
        {
            var info = modelRoot ? modelRoot.GetComponent<ModelInfo>() : null;
            if (info == null)
            {
                Debug.LogWarning("Skip auto GPD import: ModelRoot has no ModelInfo.");
                return false;
            }

            if (string.IsNullOrEmpty(info.OutputPath))
            {
                Debug.LogWarning($"Skip auto GPD import for {modelRoot.name}: empty output path.");
                return false;
            }

            Debug.Log($"Try auto import GPD for {modelRoot.name}: {info.OutputPath}");
            if (!File.Exists(info.OutputPath))
            {
                Debug.Log($"No same-directory GPD found for {modelRoot.name}: {info.OutputPath}");
                return false;
            }

            new PhysicsDataImporter().Import(modelRoot, info.OutputPath);
            return true;
        }

        public void Import(GameObject modelRoot, string inputPath)
        {
            if (modelRoot == null) throw new ArgumentNullException(nameof(modelRoot));
            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath)) throw new FileNotFoundException(inputPath);

            Debug.Log($"Start importing GPD. ModelRoot={modelRoot.name}, Path={inputPath}");
            _modelRoot = modelRoot;
            _bones = modelRoot.GetComponentsInChildren<StudioBone>().ToDictionary(x => x.Index);
            Debug.Log($"Found {_bones.Count} StudioBone components under {modelRoot.name}.");
            _doc = new XmlDocument();
            _doc.Load(inputPath);

            ValidateRoot(inputPath);
            ClearExistingPhysics();
            ImportRigidBodies();
            ImportConstraints();
            Debug.Log($"Import GPD completed. ModelRoot={modelRoot.name}, Shapes={_shapeCount}, RigidBodies={_rigidbodyCount}, Constraints={_constraintCount}, Path={inputPath}");
        }

        private void ValidateRoot(string inputPath)
        {
            var root = _doc.DocumentElement;
            if (root == null || root.Name != "goldsrc-physics-data")
                throw new InvalidDataException($"{inputPath} is not a goldsrc physics data file.");

            var modelInfo = _modelRoot.GetComponent<ModelInfo>();
            var checksum = root.GetAttribute("checksum");
            if (modelInfo != null && !string.IsNullOrEmpty(checksum) && checksum != modelInfo.Checksum)
                Debug.LogWarning($"GPD checksum mismatch. Model={modelInfo.Checksum}, GPD={checksum}, Path={inputPath}");
        }

        private void ClearExistingPhysics()
        {
            var constraints = _modelRoot.GetComponentsInChildren<PhysicsConstraintComponent>();
            var bodies = _modelRoot.GetComponentsInChildren<PhysicsBody>();
            var shapes = _modelRoot.GetComponentsInChildren<CollisionShapeComponent>();
            Debug.Log($"Clear existing physics components before GPD import. Constraints={constraints.Length}, RigidBodies={bodies.Length}, Shapes={shapes.Length}");
            foreach (var c in constraints) UnityEngine.Object.DestroyImmediate(c);
            foreach (var b in bodies) UnityEngine.Object.DestroyImmediate(b);
            foreach (var s in shapes) UnityEngine.Object.DestroyImmediate(s);
        }

        private void ImportRigidBodies()
        {
            var shapes = _doc.SelectNodes("/goldsrc-physics-data/collision-shape-block/collision-shape");
            var rigids = _doc.SelectNodes("/goldsrc-physics-data/rigidbody-block/rigidbody");
            if (shapes == null || rigids == null)
            {
                Debug.LogWarning("GPD contains no collision-shape-block or rigidbody-block.");
                return;
            }

            Debug.Log($"Importing GPD rigid bodies. ShapeElements={shapes.Count}, RigidBodies={rigids.Count}");

            foreach (XmlElement rigid in rigids)
            {
                int boneIndex = IntAttr(rigid, "bone");
                int shapeIndex = IntAttr(rigid, "shape");
                if (!_bones.TryGetValue(boneIndex, out var bone))
                    throw new InvalidDataException($"Cannot find bone index {boneIndex} for rigidbody.");

                if (shapeIndex < 0 || shapeIndex >= shapes.Count)
                    throw new InvalidDataException($"Invalid shape index {shapeIndex} for rigidbody on bone {boneIndex}.");

                var rigidLocal = MatrixField(rigid, "local");
                var rigidGoldsrc = bone.WorldTransform.ToGoldsrc() * rigidLocal;
                ImportShape(bone, rigidGoldsrc, (XmlElement)shapes[shapeIndex]);

                var body = bone.gameObject.AddComponent<PhysicsBody>();
                body.IsAttachment = IntAttr(rigid, "type") == 1;
                _rigidWorldGoldsrc.Add(rigidGoldsrc);
                _rigidBodies.Add(body);
                _rigidbodyCount++;
            }
        }

        private void ImportShape(StudioBone bone, Matrix4x4 rigidGoldsrc, XmlElement shapeElement)
        {
            var subs = shapeElement.SelectNodes("sub-collision-shape");
            if (subs == null) return;

            foreach (XmlElement sub in subs)
            {
                var shapeGoldsrc = sub.SelectSingleNode("local") is XmlElement local
                    ? rigidGoldsrc * MatrixField(sub, "local")
                    : rigidGoldsrc;
                var shapeUnity = shapeGoldsrc.ToUnity();
                var type = sub.GetAttribute("type");

                if (type == "primitive.box")
                {
                    var box = bone.gameObject.AddComponent<BoxCollisionShapeComponent>();
                    var half = VectorField(sub, "halfextent");
                    box.HalfExtent = new Vector3(half.x, half.z, half.y);
                    box.WorldTransform = shapeUnity;
                    _shapeCount++;
                }
                else if (type == "primitive.capsule")
                {
                    var capsule = bone.gameObject.AddComponent<CapsuleCollisionShapeComponent>();
                    capsule.Radius = FloatField(sub, "radius");
                    capsule.Height = FloatField(sub, "height");
                    capsule.WorldTransform = shapeUnity;
                    _shapeCount++;
                }
                else throw new NotSupportedException($"Unsupported collision shape {type}.");
            }
        }

        private void ImportConstraints()
        {
            var constraints = _doc.SelectNodes("/goldsrc-physics-data/constraint-block/constraint");
            if (constraints == null)
            {
                Debug.Log("GPD contains no constraint-block constraints.");
                return;
            }

            Debug.Log($"Importing GPD constraints. Constraints={constraints.Count}");

            foreach (XmlElement e in constraints)
            {
                int rba = IntAttr(e, "rba");
                int rbb = IntAttr(e, "rbb");
                if (rba < 0 || rba >= _rigidBodies.Count || rbb < 0 || rbb >= _rigidBodies.Count)
                    throw new InvalidDataException($"Invalid constraint rigidbody index. rba={rba}, rbb={rbb}, rigidbodyCount={_rigidBodies.Count}.");

                var go = _rigidBodies[rba].gameObject;
                PhysicsConstraintComponent c;
                switch (e.GetAttribute("type"))
                {
                    case "spherical": c = go.AddComponent<SphericalConstraintComponent>(); break;
                    case "cone":
                        var cone = go.AddComponent<ConeTwistConstraintComponent>();
                        cone.TwistSpan = FloatField(e, "twistspan");
                        cone.SwingSpan1 = FloatField(e, "swingspan1");
                        cone.SwingSpan2 = FloatField(e, "swingspan2");
                        c = cone; break;
                    case "hinge":
                        var hinge = go.AddComponent<HingeConstraintComponent>();
                        hinge.Low = FloatField(e, "low");
                        hinge.High = FloatField(e, "high");
                        c = hinge; break;
                    default: throw new NotSupportedException($"Unsupported constraint {e.GetAttribute("type")}.");
                }
                c.ConnectedBody = _rigidBodies[rbb];
                c.WorldTransform = (_rigidWorldGoldsrc[rba] * MatrixField(e, "locala")).ToUnity();
                _constraintCount++;
            }
        }

        private static int IntAttr(XmlElement e, string name) => int.Parse(e.GetAttribute(name));
        private static float FloatField(XmlElement e, string name) => ParseFloats(e[name]?.InnerText, 1)[0];
        private static Vector3 VectorField(XmlElement e, string name) { var v = ParseFloats(e[name]?.InnerText, 3); return new Vector3(v[0], v[1], v[2]); }
        private static Matrix4x4 MatrixField(XmlElement e, string name) { var v = ParseFloats(e[name]?.InnerText, 16); var m = new Matrix4x4(); for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++) m[r, c] = v[r * 4 + c]; return m; }
        private static float[] ParseFloats(string text, int count)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidDataException("Missing numeric field in GPD.");
            var values = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseFloat)
                .Take(count)
                .ToArray();
            if (values.Length < count) throw new InvalidDataException($"GPD numeric field requires {count} values, but got {values.Length}.");
            return values;
        }

        private static float ParseFloat(string value)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
            return float.Parse(value, CultureInfo.CurrentCulture);
        }
    }
}
