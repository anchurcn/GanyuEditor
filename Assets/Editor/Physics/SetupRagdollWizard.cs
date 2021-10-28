using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;
using GanyuEditor.Extensions;

namespace GanyuEditor.Physics
{
    internal static class Helper
    {
        public static Vector3 Pos(this StudioBone self) => self.transform.position;
        public static Matrix4x4 Trans(this StudioBone self) => self.transform.localToWorldMatrix;
        /// <summary>
        /// Find the root gameobject of given object on the scene hierarchy.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static GameObject RootObject(this GameObject self)
        {
            while (self.transform.parent)
            {
                self = self.transform.parent.gameObject;
            }
            return self;
        }
        public static void AddShape(this StudioBone self, Matrix4x4 shapeTrans, float height, float radius)
        {
            var shape = self.gameObject.AddComponent<CapsuleShape>();
            self.gameObject.AddComponent<GanyuEditor.Physics.Rigidbody>();
            shape.WorldTransform = shapeTrans;
            shape.Height = height;
            shape.Radius = radius;
        }
    }
    // Bipped ragdoll
    public class SetupRagdollWizard : ScriptableWizard
    {
        public class BoneNameConvention
        {
            public string Name;
            // Do not change the field name below.
            // Must match names in SetupRagdollWizard.
            public string Pelvis;
            public string Spine;
            public string Chest;
            public string Head;

            public string LeftArm;
            public string LeftElbow;
            public string LeftHand;

            public string RightArm;
            public string RightElbow;
            public string RightHand;

            public string LeftHip;
            public string LeftKnee;
            public string LeftFoot;

            public string RightHip;
            public string RightKnee;
            public string RightFoot;
        }
        public enum SelectedConv
        {
            DoNotUseConv = -1,
            Convention0,
            Convention1,
            Convention2,
            Convention3,
            Convention4,
            Convention5,
            Convention6,
            Convention7,
            Convention8,
            Convention9,
            Convention10,
            Convention11,
            Convention12,
            Convention13,
            Convention14,
            Convention15,
            Convention16,
            Convention17,
            Convention18,
            Convention19,
            Convention20,
        }

        public StudioBone Pelvis;

        public StudioBone Spine;

        // Sometimes spine2/3 , is a spine bone aroud elbow upper.
        public StudioBone Chest;

        public StudioBone Head;

        public StudioBone LeftArm;
        public StudioBone LeftElbow;
        public StudioBone LeftHand;

        public StudioBone RightArm;
        public StudioBone RightElbow;
        public StudioBone RightHand;

        public StudioBone LeftHip;
        public StudioBone LeftKnee;
        public StudioBone LeftFoot;

        public StudioBone RightHip;
        public StudioBone RightKnee;
        public StudioBone RightFoot;

        public SelectedConv SelectedConvention = SelectedConv.DoNotUseConv;
        public List<string> Conventions = new List<string>();
        public bool SaveCurrent = true;
        private SelectedConv _lastSelected = SelectedConv.DoNotUseConv;
        private List<BoneNameConvention> _boneNames = new List<BoneNameConvention>();

        [MenuItem("GameObject/SetupRagdoll...", priority = 11)]
        static void SetupRagdoll(MenuCommand menuCommand)
        {
            DisplayWizard<SetupRagdollWizard>("Setup ragdoll", "Setup", "SaveConvension");
        }
        private void OnWizardCreate()
        {
            if (Validate())
            {
                ClearnUp();
                BuildBodies();
                BuildConstraints();
                Debug.Log($"Setup ragdoll for {Pelvis.gameObject.RootObject().name} successfully.");
            }
            else
            {
                Debug.LogError("Invalid assigness.");
            }
        }
        private void OnWizardOtherButton()
        {
            SaveConventions();
        }
        private void Awake()
        {
            helpString = "Drag all the bones from the hierarchy into their slots.\nMake sure your character is in T-Pose, and face to -Z axis.\n" +
               "将骨骼从左边的层级视图拖到对应的格子。\n确保模型为T-Pose（蒙皮姿态），且面向负Z轴。\n" +
               "Chest sometimes spine2/3 , is a spine bone aroud elbow upper.\n";

            LoadConventions();
        }
        
        private string _filePath = @"BoneNameConvensions.xml";
        private void LoadConventions()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(_boneNames.GetType());
            if (!File.Exists(_filePath))
            {
                using (var stream = File.Open(_filePath, FileMode.Create))
                {
                    xmlSerializer.Serialize(stream, _boneNames);
                }
            }
            else
            {
                using (var stream = File.OpenRead(_filePath))
                {
                    _boneNames = (List<BoneNameConvention>)xmlSerializer.Deserialize(stream);
                }
            }

            foreach (var i in _boneNames)
            {
                Conventions.Add(i.Name);
            }
        }
        private void SaveConventions()
        {
            var tobesave = new List<BoneNameConvention>();
            var tobedel = new List<string>();
            for (int i = 0; i < Conventions.Count; i++)
            {
                if (!string.IsNullOrEmpty(Conventions[i]))
                {
                    _boneNames[i].Name = Conventions[i];
                    tobesave.Add(_boneNames[i]);
                }
                else
                {
                    tobedel.Add(_boneNames[i].Name);
                }
            }

            if (tobedel.Count > 0)
            {
                var result = EditorUtility.DisplayDialog("Info",
                    $"Are you sure to delete {(tobedel.Count > 1 ? $"these {tobedel.Count}" : "this")} entry " +
                    string.Join(", ", tobedel) + "?\n\n" +
                    "Click Cancel button to cancel the save operatoin.", "OK", "Cancel");

                // Revert empty name.
                if (result == false)
                {
                    for (int i = 0; i < Conventions.Count; i++)
                    {
                        if(string.IsNullOrEmpty(Conventions[i]))
                        {
                            Conventions[i] = _boneNames[i].Name;
                        }
                    }
                    return;
                }
            }

            if (SaveCurrent)
            {
                tobesave.Add(GetCurrentSetup());
                tobesave.Last().Name = "New entry";
            }

            using (var stream = File.OpenWrite(_filePath))
            {
                stream.SetLength(0);
                var xml = new XmlSerializer(tobesave.GetType());
                xml.Serialize(stream, tobesave);
            }

            // Reload
            Conventions.Clear();
            _boneNames.Clear();
            LoadConventions();
        }

        BoneNameConvention GetCurrentSetup() => AllBodyPartsInfo()
            .Aggregate(new BoneNameConvention(),
            (a,next)=> {
                var t = a.GetType();
                var field = t.GetField(next.name);
                var n = next.obj.name;
                field.SetValue(a, n);
                return a;
            });
        private void SetupWithConventions()
        {
            if (SelectedConvention < 0)
            {
                SelectedConvention = SelectedConv.DoNotUseConv;
                _lastSelected = SelectedConv.DoNotUseConv;
                return;
            }

            if ((int)SelectedConvention >= _boneNames.Count)
            {
                SelectedConvention = _lastSelected;
                EditorUtility.DisplayDialog("Info", $"Out of range [0...{_boneNames.Count}].", "OK");
                return;
            }

            var modelRoot = Selection.activeGameObject;
            if (modelRoot?.GetComponent<ModelInfo>())
            {
                var conv = _boneNames[(int)SelectedConvention];
                var bones = modelRoot.GetComponentsInChildren<StudioBone>();
                AllBodyPartsInfo().All(
                    x =>
                    {
                        string boneName = (string)conv.GetType().GetField(x.name).GetValue(conv);
                        var boneFound = bones.FirstOrDefault(bone => bone.gameObject.name == boneName);
                        GetType().GetField(x.name).SetValue(this, boneFound);
                        return true;
                    });

                _lastSelected = SelectedConvention;
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "Must select a model in hierarchy.", "OK");
                SelectedConvention = _lastSelected;
            }
        }
        private bool SelectedChanged()
        {
            if (SelectedConvention == _lastSelected)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void BuildBodies()
        {
            var lArmPos = LeftArm.Pos();
            var rArmPos = RightArm.Pos();
            var bodyWidth = (lArmPos - rArmPos).magnitude;
            // head
            var headPos = Head.Pos();
            var headWidth = bodyWidth / 2.5f;//躯干是头的2.5倍
            var headRaito = 5.85f / 8.1f;//头宽高比
            var headHeight = headWidth / headRaito;
            var headShapeTrans = Matrix4x4.Translate(headPos + new Vector3(0, headHeight / 2, 0)).Lookat(headPos, Vector3.right);
            Head.AddShape(headShapeTrans, headHeight, headWidth / 2);
            // up limbs
            CreateLimb(LeftArm, LeftElbow.Pos(), headWidth * 0.55f);
            CreateLimb(LeftElbow, LeftHand.Pos(), headWidth * 0.5f);
            CreateLimb(RightArm, RightElbow.Pos(), headWidth * 0.55f);
            CreateLimb(RightElbow, RightHand.Pos(), headWidth * 0.5f);
            // low limbs
            CreateLimb(LeftHip, LeftKnee.Pos(), headWidth * 0.55f);
            CreateLimb(LeftKnee, LeftFoot.Pos(), headWidth * 0.5f);
            CreateLimb(RightHip, RightKnee.Pos(), headWidth * 0.55f);
            CreateLimb(RightKnee, RightFoot.Pos(), headWidth * 0.5f);
            // pelvis
            var pelvisTrans = Pelvis.Trans().Lookat(Pelvis.Pos() + new Vector3(1, 0, 0), Vector3.right);
            Pelvis.AddShape(pelvisTrans, bodyWidth, (Spine.Pos() - Pelvis.Pos()).magnitude * 1.1f);
            // spine
            var spineTrans = Matrix4x4.Translate((Chest.Pos() + Spine.Pos()) / 2);
            var spineShape = Spine.gameObject.AddComponent<BoxShape>();
            spineShape.WorldTransform = spineTrans;
            spineShape.HalfExtent = new Vector3(bodyWidth / 2, (Chest.Pos() - Spine.Pos()).magnitude / 2, headWidth * 0.7f);
            Spine.gameObject.AddComponent<GanyuEditor.Physics.Rigidbody>();
            // chest
            Vector3 neckPos = (new Vector3(Head.Pos().x, LeftArm.Pos().y, Head.Pos().z) + headPos) / 2;
            var chestTrans = Matrix4x4.Translate((neckPos + Chest.Pos()) / 2);
            var chestShape = Chest.gameObject.AddComponent<BoxShape>();
            chestShape.WorldTransform = chestTrans;
            chestShape.HalfExtent = new Vector3(bodyWidth / 2, (neckPos - Chest.Pos()).magnitude / 2, headWidth * 0.7f);
            Chest.gameObject.AddComponent<GanyuEditor.Physics.Rigidbody>();
        }
        private void BuildConstraints()
        {
            //AddConstraint(Spine, Pelvis);
            var cone = AddConeTwist(Spine, Pelvis);
            var rot = Matrix4x4.Translate(Spine.Pos()).Lookat(Chest.Pos(), Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 20;
            cone.SwingSpan1 = 35;
            cone.SwingSpan2 = 10;

            //AddConstraint(Chest, Spine);
            cone = AddConeTwist(Chest, Spine);
            rot = Matrix4x4.Translate(Chest.Pos()).Lookat(Head.Pos(), Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 25;
            cone.SwingSpan1 = 15;
            cone.SwingSpan2 = 5;

            //AddConstraint(Head, Chest);
            cone = AddConeTwist(Head, Chest);
            rot = Matrix4x4.Translate(Head.Pos()).Lookat(Head.Pos() + Vector3.up, Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 30;
            cone.SwingSpan1 = 30;
            cone.SwingSpan2 = 8;

            //AddConstraint(LeftArm, Chest);
            cone = AddConeTwist(LeftArm, Chest);
            rot = Matrix4x4.Translate(LeftArm.Pos()).Lookat(LeftArm.Pos() + new Vector3(1.2f, -1, 0), Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 25;
            cone.SwingSpan1 = 90;
            cone.SwingSpan2 = 50;

            //AddConstraint(LeftElbow, LeftArm);
            AddLeftElbowJoint();

            //AddConstraint(RightArm, Chest);
            cone = AddConeTwist(RightArm, Chest);
            rot = Matrix4x4.Translate(RightArm.Pos()).Lookat(RightArm.Pos() + new Vector3(-1.2f, -1, 0), Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 25;
            cone.SwingSpan1 = 90;
            cone.SwingSpan2 = 50;

            //AddConstraint(RightElbow, RightArm);
            AddRightElbowJoint();

            //AddConstraint(LeftHip, Pelvis);
            cone = AddConeTwist(LeftHip, Pelvis);
            rot = Matrix4x4.Translate(LeftHip.Pos()).Lookat(LeftKnee.Pos(), Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 3;
            cone.SwingSpan1 = 40;
            cone.SwingSpan2 = 20;

            //AddConstraint(RightHip, Pelvis);
            cone = AddConeTwist(RightHip, Pelvis);
            rot = Matrix4x4.Translate(RightHip.Pos()).Lookat(RightKnee.Pos(), Vector3.right).rotation;
            cone.Rotation = rot;
            cone.TwistSpan = 3;
            cone.SwingSpan1 = 40;
            cone.SwingSpan2 = 20;

            //AddConstraint(LeftKnee, LeftHip);
            var hinge = AddHinge(LeftKnee, LeftHip);
            rot = Matrix4x4.identity.Lookat(Vector3.right, Vector3.up).rotation;
            hinge.Rotation = rot;
            hinge.High = 135;

            //AddConstraint(RightKnee, RightHip);
            hinge = AddHinge(RightKnee, RightHip);
            rot = Matrix4x4.identity.Lookat(Vector3.right, Vector3.up).rotation;
            hinge.Rotation = rot;
            hinge.High = 135;
        }
        private void AddLeftElbowJoint()
        {
            var a = LeftHand.Pos() - LeftElbow.Pos();
            var b = LeftArm.Pos() - LeftElbow.Pos();
            var c = Vector3.Cross(a, b);
            var rot = Quaternion.identity;
            if (Mathf.Abs(c.magnitude) > 1e-6)
            {
                rot = Matrix4x4.identity.Lookat(c, Vector3.up).rotation;
            }
            else
            {
                rot = Quaternion.identity;
                // TODO:
            }
            var hinge = AddHinge(LeftElbow, LeftArm);
            hinge.Rotation = rot;
            hinge.Low = -140;
        }
        private void AddRightElbowJoint()
        {
            var a = RightHand.Pos() - RightElbow.Pos();
            var b = RightArm.Pos() - RightElbow.Pos();
            var c = Vector3.Cross(a, b);
            var rot = Quaternion.identity;
            if (Mathf.Abs(c.magnitude) > 1e-6)
            {
                rot = Matrix4x4.identity.Lookat(c, Vector3.up).rotation;
            }
            else
            {
                rot = Quaternion.identity;
                // TODO:
            }
            var hinge = AddHinge(RightElbow, RightArm);
            hinge.Rotation = rot;
            hinge.Low = -140;
        }
        private HingeConstraint AddHinge(StudioBone bone, StudioBone parent)
        {
            HingeConstraint hinge = bone.gameObject.AddComponent<HingeConstraint>();
            hinge.ConnectedBody = parent.GetComponent<GanyuEditor.Physics.Rigidbody>();
            return hinge;
        }
        private ConeTwistConstraint AddConeTwist(StudioBone bone, StudioBone parent)
        {
            ConeTwistConstraint cone = bone.gameObject.AddComponent<ConeTwistConstraint>();
            cone.ConnectedBody = parent.GetComponent<GanyuEditor.Physics.Rigidbody>();
            return cone;
        }
        private void CreateLimb(StudioBone bone, Vector3 childPos, float radius)
        {
            var shape = bone.gameObject.AddComponent<CapsuleShape>();
            bone.gameObject.AddComponent<GanyuEditor.Physics.Rigidbody>();

            shape.Height = (bone.Pos() - childPos).magnitude;
            shape.Radius = radius;
            shape.WorldTransform = Matrix4x4.Translate((bone.Pos() + childPos) / 2).Lookat(childPos, Vector3.right);
        }
        private void OnWizardUpdate()
        {
            if (SelectedChanged())
                SetupWithConventions();
            if (Validate())
                isValid = true;
            else
                isValid = false;
        }
        // true for valid
        private bool Validate()
        {
            var assigned = AllBodyParts().Where(x => x != null);
            if (assigned.Any())
            {
                var first = assigned.First();
                var root = first.RootObject();
                var diffroot = assigned.FirstOrDefault(x => x.RootObject() != root);
                if (diffroot != null)
                {
                    errorString = $"{first.name} and {diffroot.name} are not in the same model.";
                    return false;
                }
                if (root.GetComponent<ModelInfo>() == null)
                {
                    errorString = $"Missing {nameof(ModelInfo)} component at the model root.";
                    return false;
                }
            }

            // foreach assigned item
            foreach (var i in AllBodyPartsInfo().Where(x => x.obj != null))
            {
                var same = AllBodyPartsInfo().
                    Except(AllBodyPartsInfo().Where(x => x.name == i.name)).
                    FirstOrDefault(x => x.obj == i.obj);
                if (same.obj != null)
                {
                    errorString = $"{i.name} and {same.name} may not be assigned to the same bone.";
                    return false;
                }
            }

            foreach (var i in AllBodyPartsInfo())
            {
                if (i.obj == null)
                {
                    errorString = $"{i.name} has not been assigned yet.\n";
                    return false;
                }
            }
            errorString = string.Empty;
            return true;
        }

        private void ClearnUp()
        {

        }

        private IEnumerable<GameObject> AllBodyParts()
        {
            yield return Pelvis?.gameObject;
            yield return Spine?.gameObject;
            yield return Chest?.gameObject;
            yield return Head?.gameObject;
            yield return LeftArm?.gameObject;
            yield return LeftElbow?.gameObject;
            yield return LeftHand?.gameObject;
            yield return RightArm?.gameObject;
            yield return RightElbow?.gameObject;
            yield return RightHand?.gameObject;
            yield return LeftHip?.gameObject;
            yield return LeftKnee?.gameObject;
            yield return LeftFoot?.gameObject;
            yield return RightHip?.gameObject;
            yield return RightKnee?.gameObject;
            yield return RightFoot?.gameObject;
        }
        private IEnumerable<(GameObject obj, string name)> AllBodyPartsInfo()
        {
            yield return (Pelvis?.gameObject, nameof(Pelvis));
            yield return (Spine?.gameObject, nameof(Spine));
            yield return (Chest?.gameObject, nameof(Chest));
            yield return (Head?.gameObject, nameof(Head));
            yield return (LeftArm?.gameObject, nameof(LeftArm));
            yield return (LeftElbow?.gameObject, nameof(LeftElbow));
            yield return (LeftHand?.gameObject, nameof(LeftHand));
            yield return (RightArm?.gameObject, nameof(RightArm));
            yield return (RightElbow?.gameObject, nameof(RightElbow));
            yield return (RightHand?.gameObject, nameof(RightHand));
            yield return (LeftHip?.gameObject, nameof(LeftHip));
            yield return (LeftKnee?.gameObject, nameof(LeftKnee));
            yield return (LeftFoot?.gameObject, nameof(LeftFoot));
            yield return (RightHip?.gameObject, nameof(RightHip));
            yield return (RightKnee?.gameObject, nameof(RightKnee));
            yield return (RightFoot?.gameObject, nameof(RightFoot));
        }
    }
}
