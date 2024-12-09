using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character Data", order = 51)]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public float weight;
    public float speed;
    public int maxJumps;

    public AnimationClip idleAnimation;
    public AnimationClip jumpAnimation;
    public AnimationClip crouchAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip damageAnimation;

    public Sprite characterIcon;

    // Arrays de clips de audio
    public AudioClip[] jumpSounds;
    public AudioClip[] crouchSounds;
    public AudioClip[] walkSounds;
    public AudioClip[] runSounds;
    public AudioClip[] damageSounds;

    // Tiempos de espera entre audios
    public float jumpSoundDelay = 0.5f;
    public float crouchSoundDelay = 0.5f;
    public float walkSoundDelay = 0.5f;
    public float runSoundDelay = 0.3f; // Tiempo específico entre pasos al correr
    public float attackSoundDelay = 0.5f;

    // Referencia a los datos de ataques
    public AttackData[] attacks;

    // Método para obtener un clip de audio aleatorio
    public AudioClip GetRandomAudioClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("No audio clips available.");
            return null;
        }
        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }
}
