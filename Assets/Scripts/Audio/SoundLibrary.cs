using UnityEngine;

public enum SoundID
{
    AddToppings,
    BellRing,
    ButtonClick,
    Cleaning,
    ComputerButtonClick,
    CustomerEnter,
    Eating,
    GrabItem,
    Jump,
    StarRatingDecrease,
    Mumbling,
    OpenDoor,
    OvenCooking,
    PizzaBaseSpawn,
    PizzaDoneCooking,
    PoisonBottleShatter,
    PurchaseSuccessful,
    PurchaseFailed,
    RatDeath,
    RatEating,
    RatNoise,
    RatSpray,
    RatSteal,
    RatTrap,
    ReceiveMoney,
    Running,
    ShotgunFire,
    ShotgunReload,
    StarRatingIncrease,
    Walking,
    WashingHands,
    WritingInNotepad
}

public enum MusicID
{
    MainMenu,
    Gameplay
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public SoundID id;
        public AudioClip clip;
    }

    [System.Serializable]
    public class MusicEntry
    {
        public MusicID id;
        public AudioClip clip;
    }

    [Header("Sound Effects")]
    public SoundEntry[] sounds;

    [Header("Music")]
    public MusicEntry[] music;

    public AudioClip GetSound(SoundID id)
    {
        foreach (SoundEntry entry in sounds)
        {
            if (entry.id == id)
                return entry.clip;
        }

        Debug.LogWarning("Sound not found: " + id);
        return null;
    }

    public AudioClip GetMusic(MusicID id)
    {
        foreach (MusicEntry entry in music)
        {
            if (entry.id == id)
                return entry.clip;
        }

        Debug.LogWarning("Music not found: " + id);
        return null;
    }
}