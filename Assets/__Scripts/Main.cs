using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement; // Enables the loading & reloading of scenes

public class Main : MonoBehaviour
{
    static private Main S; // A private singleton for Main
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Inscribed")]
    public bool spawnEnemies = true;
    public GameObject[] prefabEnemies; // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f; // # Enemies spawned/second
    public float enemyInsetDefault = 1.5f; // Inset from the sides
    public float gameRestartDelay = 2;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[]{
        eWeaponType.blaster, eWeaponType.blaster, eWeaponType.spread, eWeaponType.shield
    };
    private BoundsCheck bndCheck;
    private AudioSource audioSource;

    void Start(){
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Ensure the audio is playing
        if (audioSource && !audioSource.isPlaying){
            audioSource.Play();
        }
    }
    
    void Awake(){
        S = this;
        // Set bndCheck to reference the BoundsCheck component on this
        // GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        Invoke(nameof(SpawnEnemy), 1f/enemySpawnPerSecond);

        // A generic Dictionary with eWeaponType as the key
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions){
            WEAP_DICT[def.type] = def;
        }
    }

    public void SpawnEnemy(){
        // If spawnEnemies is false, skip to the next invoke of SpawnEnemy()
        if (!spawnEnemies){
            Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);
            return;
        }
        // Pick a random Enemy prefab to instantiate
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;
        if (go.GetComponent<BoundsCheck>() != null){
            enemyInset = Mathf.Abs(go.AddComponent<BoundsCheck>().radius);
        }

        // Set the initial position for the spawned Enemy
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camHeight - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        // Invoke SpawnEnemy() again
        Invoke(nameof(SpawnEnemy), 1f/enemySpawnPerSecond);
    }

    void DelayedRestart(){
        // Invoke the Restart() method in gameRestartDelay seconds
        Invoke(nameof(Restart), gameRestartDelay);
    }

    void Restart(){
        // Reload __Scene_0 to restart the game
        // "__Scene_0" below starts with 2 underscores and ends with a zero
        SceneManager.LoadScene("__Scene_0");
    }

    static public void HERO_DIED(){
        S.DelayedRestart();
    }

    /// <summary>
    /// Static function that gets a WeaponDefinition from the WEAP_DICT static
    /// protected field of the Main Class
    /// </summary>
    /// <returns>The WeaponDefiniton, or if there is no WeaponDefinition with
    /// the eWeaponType passed in, returns a new WeaponDefinition with a
    /// eWeaponType of WeaponType.none.</return>
    /// <param name="wt">The eWeaponType of the desired WeaponDefiniton</param>
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt){
        if (WEAP_DICT.ContainsKey(wt)){
            return(WEAP_DICT[wt]);
        }
        // If no entry of the correct type exists in WEAP_DICT, return a new
        // WeaponDefinition with a type of eWeaponType.none (the default value)
        return(new WeaponDefinition());
    }

    /// <summary>
    /// Called by an Enemy ship whenever it is destroyed. It sometimes creates
    /// a PowerUp in place of the destroyed ship.
    /// </summary>
    /// <param name="e">The Enemy that was destroyed</param>
    static public void SHIP_DESTROYED(Enemy e){
        // Potentially generate a PowerUp
        if (Random.value <= e.powerUpDropChance){ // Underlined red for now
            // Choose a PowerUp from the possibilities in powerUpFrequency
            int ndx = Random.Range(0, S.powerUpFrequency.Length);
            eWeaponType pUpType = S.powerUpFrequency[ndx];

            // Spawn a PowerUp
            GameObject go = Instantiate<GameObject>(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            // Set it to the proper WeaponType
            pUp.SetType(pUpType);

            // Set it to the position of the destroyed ship
            pUp.transform.position = e.transform.position;
        }
    }


}
