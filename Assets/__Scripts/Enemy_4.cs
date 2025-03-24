using UnityEngine;

[RequireComponent(typeof(EnemyShield))]
public class Enemy_4 : Enemy { // Enemy_4 also extends the Enemy class
    [Header("Enemy_4 Inscribed Fields")]
    public float duration = 4; // Duration of interpolation movement
    
    private EnemyShield[] allShields;
    private EnemyShield thisShield; 
    private Vector3 p0, p1; // The two points to interpolate
    private float timeStart; // Birth time for this Enemy_4  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allShields = GetComponentsInChildren<EnemyShield>();
        thisShield = GetComponent<EnemyShield>();

        // Initially set p0 & p1 to the current position (from Main.SpawnEnemy())
        p0 = p1 = pos;
        InitMovement();
    }

    void InitMovement(){
        p0 = p1; // Set p0 to the old p1
        // Assign a new on-screen location to p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Make sure that it moves to a different quadrant of the screen
        if (p0.x * p1.x > 0 && p0.y * p1.y > 0){
            if (Mathf.Abs(p0.x) > Mathf.Abs(p0.y)){
                p1.x *= -1;
            }
            else{
                p1.y *= -1;
            }
        }

        // Reset the time
        timeStart = Time.time;
    }

    public override void Move()
    {
        // You'll add much more here shortly. For now, its easier to test if
        // Enemy_4 doesn't move
        // This completely overrides Enemy.Move() with a linear interpolation
        float u = (Time.time - timeStart) / duration;

        if (u >= 1){
            InitMovement();
            u = 0;
        }

        u = u - 0.15f * Mathf.Sin(u * 2 * Mathf.PI); // Easing: Sine -0.15
        transform.position = (1 - u) * p0 + u * p1; // Simple linear interpolation
    }

    /// <summary>
    /// Enemy_4 Collisions are handled differently
    /// </summary>
    /// <param name="coll"></param>
    void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;

        // Make sure this was hit by a ProjectileHero
        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if (p != null){
            // Destroy the ProjectileHero regardless of bndCheck.isOnScreen
            Destroy(otherGO);

            // Only damage this Enemy if its on screen
            if (bndCheck.isOnScreen){
                // Find the GameObject of this Enemy_4 that was actually hit
                GameObject hitGO = coll.contacts[0].thisCollider.gameObject;
                if (hitGO == otherGO){
                    hitGO = coll.contacts[0].otherCollider.gameObject;
                }

                // Get the damage amount from the Main WEAP_DICT
                float dmg = Main.GET_WEAPON_DEFINITION(p.type).damageOnHit;

                // Find the EnemyShield that was hit (if there was one)
                bool shieldFound = false;
                foreach (EnemyShield es in allShields){
                    if (es.gameObject == hitGO){
                        es.TakeDamage(dmg);
                        shieldFound = true;
                    }
                }
                if (!shieldFound) thisShield.TakeDamage(dmg);

                // If thisShield is still active, then it has not been destroyed
                if (thisShield.isActive) return;

                // This ship was destroyed so tell Main about it
                if (!calledShipDestroyed){
                    Main.SHIP_DESTROYED(this);
                    calledShipDestroyed = true;
                }

                // Destroy this Enemy_4
                Destroy(gameObject);
            }
        }
        else {
            Debug.Log("Enemy_4 hit by non-ProjectileHero: " + otherGO.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
