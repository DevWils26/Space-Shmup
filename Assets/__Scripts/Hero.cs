using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S{get; private set;} // Singleton property

    [Header("Inscribed")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Dynamic")] [Range(0, 4)] [SerializeField]
    private float _shieldLevel = 1; // Remember the underscore
    // public float shieldLevel = 1; // and remove this old line
    [Tooltip("This field holds a reference to the last triggering GameObject")]
    private GameObject lastTriggerGo = null;
    // Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    // Create a WeaponFireDelegate event named fireEvent
    public event WeaponFireDelegate fireEvent;

    void Awake()
    {
        if (S == null){
            S = this; // Set the Singleton only if it's null
        }
        else{
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        // fireEvent += TempFire;

        // Reset the weapons to start _Hero wit 1 blaster
        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
    }

    // Update is called once per frame
    void Update()
    {
        //Pull in information from the Input class
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        // Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;

        // Rotate the ship to make it feel more dynamic
        transform.rotation = Quaternion.Euler(vAxis*pitchMult,hAxis*rollMult,0);

        // // Allow the ship to fire
        // if (Input.GetKeyDown(KeyCode.Space)){
        //     TempFire();
        // }

        // Use the fireEvent to fire Weapons when the Spacebar is pressed.
        if (Input.GetAxis("Jump") == 1 && fireEvent != null){
            fireEvent();
        }
    }

    // void TempFire(){
    //     GameObject projGO = Instantiate<GameObject>(projectilePrefab);
    //     projGO.transform.position = transform.position;
    //     Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
    //     // rigidB.linearVelocity = Vector3.up * projectileSpeed;

    //     ProjectileHero proj = projGO.GetComponent<ProjectileHero>();
    //     proj.type = eWeaponType.blaster;
    //     float tSpeed = Main.GET_WEAPON_DEFINITION(proj.type).velocity;
    //     rigidB.linearVelocity = Vector3.up * tSpeed;
    // }

    void OnTriggerEnter(Collider other){
        Transform rootT = other.gameObject.transform.root;
        GameObject go  = rootT.gameObject;
        // Debug.Log("Shield trigger hit by: " +go.gameObject.name);

        // Make sure it's not the same triggering go as last time
        if (go == lastTriggerGo) return;
        lastTriggerGo = go;

        Enemy enemy = go.GetComponent<Enemy>();
        PowerUp pUp = go.GetComponent<PowerUp>();
        if (enemy != null){ // If the shield was triggered by an enemy
            shieldLevel--;
            Destroy(go); // ... and Destroy the enemy
        }
        else if (pUp != null){  // If the shield hit a PowerUp
            AbsorbPowerUp(pUp);  // ... absorb the PowerUp
        }
        else{
            Debug.LogWarning("Shield trigger hit by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(PowerUp pUp) {

        Debug.Log("Absorbed PowerUp: " + pUp.type);

 

        if (pUp.type == eWeaponType.shield) {

            shieldLevel++;

        }

        else if (pUp.type == eWeaponType.blaster || pUp.type == eWeaponType.spread) {

            if (weapons[0].type != pUp.type) {
                // Switch to the new weapon type and reset to one gun
                ClearWeapons();
                weapons[0].SetType(pUp.type);
                Debug.Log("Switched to " + pUp.type + " with one gun.");

            } 
            else {
                // Already that weapon type; try to add an extra gun
                Weapon emptySlot = GetEmptyWeaponSlot();

                if (emptySlot != null) {

                    emptySlot.SetType(pUp.type);
                    Debug.Log("Added extra " + pUp.type + " gun.");

                }

            }

        }
    }



    public float shieldLevel{
        get {return (_shieldLevel);}
        private set{
            _shieldLevel = Mathf.Min(value, 4);
            // If the shield is going to be set to less than zero...
            if (value < 0){
                Destroy(this.gameObject); // Destroy the Hero
                Main.HERO_DIED();
            }
        }
    }

    /// <summary>
    /// Finds the first empty Weapon slot (i.e., type=none) and returns it.
    /// <\summary>
    /// <returns>The first empty Weapon slot or null if none are empty</return>
    Weapon GetEmptyWeaponSlot(){
        for (int i = 0; i < weapons.Length; i++){
            if (weapons[i].type == eWeaponType.none){
                return(weapons[i]);
            }
        }
        return(null);
    }

    /// <summary>
    /// Sets the type of all Weapon slots to none
    /// </summary>
    void ClearWeapons(){
        foreach (Weapon w in weapons){
            w.SetType(eWeaponType.none);
        }
    }
}
