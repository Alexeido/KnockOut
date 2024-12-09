using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BackgroundObjectController))]
public class BackgroundObjectControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BackgroundObjectController controller = (BackgroundObjectController)target;

        // Rotation
        controller.rotateX = EditorGUILayout.Toggle("Rotate X", controller.rotateX);
        if (controller.rotateX)
        {
            controller.rotationSpeedX = EditorGUILayout.FloatField("Rotation Speed X", controller.rotationSpeedX);
        }

        controller.rotateY = EditorGUILayout.Toggle("Rotate Y", controller.rotateY);
        if (controller.rotateY)
        {
            controller.rotationSpeedY = EditorGUILayout.FloatField("Rotation Speed Y", controller.rotationSpeedY);
        }

        controller.rotateZ = EditorGUILayout.Toggle("Rotate Z", controller.rotateZ);
        if (controller.rotateZ)
        {
            controller.rotationSpeedZ = EditorGUILayout.FloatField("Rotation Speed Z", controller.rotationSpeedZ);
        }

        // Movement
        controller.moveX = EditorGUILayout.Toggle("Move X", controller.moveX);
        if (controller.moveX)
        {
            controller.moveSpeedX = EditorGUILayout.FloatField("Move Speed X", controller.moveSpeedX);
            controller.moveRangeX = EditorGUILayout.FloatField("Move Range X", controller.moveRangeX);
        }

        controller.moveY = EditorGUILayout.Toggle("Move Y", controller.moveY);
        if (controller.moveY)
        {
            controller.moveSpeedY = EditorGUILayout.FloatField("Move Speed Y", controller.moveSpeedY);
            controller.moveRangeY = EditorGUILayout.FloatField("Move Range Y", controller.moveRangeY);
        }

        controller.moveZ = EditorGUILayout.Toggle("Move Z", controller.moveZ);
        if (controller.moveZ)
        {
            controller.moveSpeedZ = EditorGUILayout.FloatField("Move Speed Z", controller.moveSpeedZ);
            controller.moveRangeZ = EditorGUILayout.FloatField("Move Range Z", controller.moveRangeZ);
        }

        // Scaling
        controller.scale = EditorGUILayout.Toggle("Scale", controller.scale);
        if (controller.scale)
        {
            controller.scaleSpeed = EditorGUILayout.FloatField("Scale Speed", controller.scaleSpeed);
            controller.scaleRange = EditorGUILayout.FloatField("Scale Range", controller.scaleRange);
        }

        // Random Smooth Movement
        controller.randomMove = EditorGUILayout.Toggle("Random Move", controller.randomMove);
        if (controller.randomMove)
        {
            controller.randomMoveSpeed = EditorGUILayout.FloatField("Random Move Speed", controller.randomMoveSpeed);
            controller.randomMoveRange = EditorGUILayout.FloatField("Random Move Range", controller.randomMoveRange);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(controller);
        }
    }
}