using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CharacterAnimatorGenerator : MonoBehaviour
{
    [MenuItem("Assets/Create Animator from CharacterData", false, 1)]
    public static void CreateAnimatorFromCharacterData()
    {
        // Obtener el objeto seleccionado en el editor
        CharacterData characterData = Selection.activeObject as CharacterData;

        if (characterData == null)
        {
            Debug.LogError("Selecciona un CharacterData para generar un Animator.");
            return;
        }

        // Crear un AnimatorController en la misma carpeta que el CharacterData
        string path = AssetDatabase.GetAssetPath(characterData);
        string folderPath = System.IO.Path.GetDirectoryName(path);
        string animatorPath = $"{folderPath}/{characterData.characterName}_Animator.controller";

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animatorPath);

        // Agregar parámetros al Animator
        controller.AddParameter("idle", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("crouch", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("walk", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("running", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("damage", AnimatorControllerParameterType.Trigger);

        // Agregar parámetros para los ataques
        for (int i = 0; i < characterData.attacks.Length; i++)
        {
            controller.AddParameter($"Attack{i}", AnimatorControllerParameterType.Trigger);
        }

        // Crear la capa base
        var rootStateMachine = controller.layers[0].stateMachine;

        // Añadir los estados principales
        AddStateToStateMachine(rootStateMachine, "idle", characterData.idleAnimation, controller);
        AddStateToStateMachine(rootStateMachine, "jump", characterData.jumpAnimation, controller);
        AddStateToStateMachine(rootStateMachine, "crouch", characterData.crouchAnimation, controller);
        AddStateToStateMachine(rootStateMachine, "walk", characterData.walkAnimation, controller);
        AddStateToStateMachine(rootStateMachine, "running", characterData.runAnimation, controller);
        AddStateToStateMachine(rootStateMachine, "damage", characterData.damageAnimation, controller);

        // Añadir estados para los ataques
        for (int i = 0; i < characterData.attacks.Length; i++)
        {
            string attackStateName = $"Attack{i}";
            AddStateToStateMachine(rootStateMachine, attackStateName, characterData.attacks[i].attackAnimation, controller);
        }

        // Guardar y refrescar la base de datos de assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Animator creado para {characterData.characterName} en {animatorPath}");
    }

private static void AddStateToStateMachine(AnimatorStateMachine stateMachine, string stateName, AnimationClip clip, AnimatorController controller)
{
    if (clip == null)
    {
        Debug.LogWarning($"AnimationClip for state '{stateName}' is null. Skipping.");
        return;
    }

    // Crear un nuevo estado en el AnimatorController
    var state = stateMachine.AddState(stateName);
    state.motion = clip;

    // Crear una transición desde AnyState hacia este estado
    var anyStateTransition = stateMachine.AddAnyStateTransition(state);
    anyStateTransition.hasExitTime = false;
    anyStateTransition.hasFixedDuration = true;
    anyStateTransition.duration = 0.1f;

    // Agregar una condición de trigger a la transición
    anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, stateName);

    Debug.Log($"Estado '{stateName}' añadido con el clip '{clip.name}' y condición de trigger '{stateName}'.");
}


}
