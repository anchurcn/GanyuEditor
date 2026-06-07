using UnityEditor;
using UnityEngine;


namespace GanyuEditor.Physics
{
    public class EditMenu
    {
        // 创建布娃娃流程：
        // 1. 导入模型
        // 2. 右键骨骼创建刚体/带约束刚体
        // 3. 调整刚体位置，调整碰撞形
        // 4. 给附件刚体（jiggle bone）勾上

        #region Hinge Auto Parenting

        static GanyuEditor.Physics.PhysicsBody FindParentRigidbody(GanyuEditor.Physics.PhysicsBody rigidbody)
        {
            var obj = rigidbody.gameObject;
            while (obj.transform.parent)
            {
                obj = obj.transform.parent.gameObject;
                rigidbody = obj.GetComponent<GanyuEditor.Physics.PhysicsBody>();
                if (rigidbody)
                    return rigidbody;
            }
            return null;
        }
        [MenuItem("GameObject/GoldsrcPhysics/AddHinge(Auto Parent)", false, 11)]
        static void AddHinge(MenuCommand menuCommand)
        {
            var studioBoneObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (studioBoneObject == null || studioBoneObject.GetComponent<StudioBone>() == null)
            {
                Debug.Log("Cannot add hinge: please right click a StudioBone object.");
                return;
            }

            var rigidbody = studioBoneObject.GetComponent<GanyuEditor.Physics.PhysicsBody>();
            if (rigidbody == null)
            {
                Debug.Log("Cannot add hinge: selected StudioBone has no PhysicsBody.");
                return;
            }

            var hinge = studioBoneObject.AddComponent<HingeConstraint>();
            hinge.ConnectedBody = FindParentRigidbody(rigidbody);
        }

        [MenuItem("GameObject/GoldsrcPhysics/AddHinge(Auto Parent)", true)]
        static bool ValidateAddHinge()
        {
            return Selection.activeGameObject != null &&
                Selection.activeGameObject.GetComponent<StudioBone>() != null &&
                Selection.activeGameObject.GetComponent<GanyuEditor.Physics.PhysicsBody>() != null;
        }

        #endregion

        #region Cone Auto Parenting

        [MenuItem("GameObject/GoldsrcPhysics/AddConeTwist(Auto Parent)", false, 11)]
        static void AddConeTwist(MenuCommand menuCommand)
        {
            var studioBoneObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (studioBoneObject == null || studioBoneObject.GetComponent<StudioBone>() == null)
            {
                Debug.Log("Cannot add cone twist: please right click a StudioBone object.");
                return;
            }

            var rigidbody = studioBoneObject.GetComponent<GanyuEditor.Physics.PhysicsBody>();
            if (rigidbody == null)
            {
                Debug.Log("Cannot add cone twist: selected StudioBone has no PhysicsBody.");
                return;
            }

            var coneTwist = studioBoneObject.AddComponent<ConeTwistConstraint>();
            coneTwist.ConnectedBody = FindParentRigidbody(rigidbody);
        }

        [MenuItem("GameObject/GoldsrcPhysics/AddConeTwist(Auto Parent)", true)]
        static bool ValidateAddConeTwist()
        {
            return Selection.activeGameObject != null &&
                Selection.activeGameObject.GetComponent<StudioBone>() != null &&
                Selection.activeGameObject.GetComponent<GanyuEditor.Physics.PhysicsBody>() != null;
        }

        #endregion
        //[MenuItem("GameObject/GRigidBodyObjectWithConstraint", false, 11)]
        //static void CreateGRigidBodyObjectWithConstraint(MenuCommand menuCommand)
        //{
        //    var studioBoneObject = menuCommand.context as GameObject;
        //    if (studioBoneObject != null && studioBoneObject.GetComponent<StudioBone>() != null)
        //    {
        //        studioBoneObject.AddComponent<GRigidbody>();
        //        studioBoneObject.AddComponent<GBoxShape>();
        //        studioBoneObject.AddComponent<GConstraint>();
        //    }
        //}
        //[MenuItem("GameObject/GRigidBodyObjectWithConstraint", true, 11)]
        //static bool ValidateCreateGRigidBodyObjectWithConstraint(MenuCommand menuCommand)
        //{
        //    var studioBoneObject = menuCommand.context as GameObject;
        //    return studioBoneObject != null && studioBoneObject.GetComponent<StudioBone>() != null;
        //}



        static void CreateGRigidbody(MenuCommand menuCommand)
        {
            var studioBoneObject = menuCommand.context as GameObject;
            if (studioBoneObject != null && studioBoneObject.GetComponent<StudioBone>() != null)
            {
                //创建新物体
                GameObject obj = new GameObject("GRigidBody");
                obj.AddComponent<BoxCollider>();
                //设置父节点为当前选中物体
                GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
                //注册到Undo系统
                Undo.RegisterCreatedObjectUndo(obj, "Create" + obj.name);
                //将新建物体设为当前选中物体
                Selection.activeObject = obj;

            }
        }
    }
}

