using UnityEngine;
using UnityEditor;
using Invector;

// BASIC FEATURES
public partial class vMenuComponent
{
    [MenuItem("Invector/Basic Locomotion/Actions/Generic Action")]
    static void GenericActionMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<Invector.CharacterController.Actions.vGenericAction>();
        else
            Debug.Log("Please select the Player to add this component.");
    }

    [MenuItem("Invector/Basic Locomotion/Actions/Generic Animation")]
    static void GenericAnimationMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<Invector.CharacterController.Actions.vGenericAnimation>();
        else
            Debug.Log("Please select the Player to add this component.");
    }

    [MenuItem("Invector/Basic Locomotion/Actions/Ladder Action")]
    static void LadderActionMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<Invector.CharacterController.Actions.vLadderAction>();
        else
            Debug.Log("Please select the Player to add this component.");
    }

    [MenuItem("Invector/Basic Locomotion/Components/HitDamageParticle")]
    static void HitDamageMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<vHitDamageParticle>();
        else
            Debug.Log("Please select a vCharacter to add the component.");
    }

    [MenuItem("Invector/Basic Locomotion/Components/MoveSetSpeed")]
    static void MoveSetMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<vMoveSetSpeed>();
        else
            Debug.Log("Please select the Player to add the component.");
    }

    [MenuItem("Invector/Basic Locomotion/Components/HeadTrack")]
    static void HeadTrackMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<vHeadTrack>();
        else
            Debug.Log("Please select a vCharacter to add the component.");
    }

    [MenuItem("Invector/Basic Locomotion/Components/FootStep")]
    static void FootStepMenu()
    {
        if (Selection.activeGameObject)
            Selection.activeGameObject.AddComponent<vFootStepFromTexture>();
        else
            Debug.Log("Please select a GameObject to add the component.");
    }

    [MenuItem("Invector/Resources/New AudioSurface")]
    static void NewAudioSurface()
    {
        vScriptableObjectUtility.CreateAsset<vAudioSurface>();
    }
}