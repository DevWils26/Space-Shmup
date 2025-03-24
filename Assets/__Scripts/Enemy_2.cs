using UnityEngine;

public class Enemy_2 : Enemy{  // Enemy_2 also extends the Enemy class
    [Header("Enemy_2 Inscribed Fields")]
    public float lifeTime = 10;
    // Enemy_2 uses a Sine wave to modify a 2-point linear interpolation
    [Tooltip("Determines how much the Sine wave will ease the interpolation")]
    public float sinEccentricity = 0.6f;
    public AnimationCurve rotCurve;
    [Header("Enemy_2 Private Fields")]
    [SerializeField] private float birthTime; // Interpolation start time

    private Quaternion baseRotation;

    [SerializeField] private Vector3 p0, p1; // Lerp_points
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Pick any point on the left side of the screen
        p0 = Vector3.zero;
        p0.x = -bndCheck.camWidth - bndCheck.radius;
        p0.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

        // Pick any point on the right side of the screen
        p1 = Vector3.zero;
        p1.x = bndCheck.camWidth + bndCheck.radius;
        p1.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);
 
        // Possibly swap sides
        if (Random.value > 0.5f){
            // Setting the .x of each point to its negative will move it to
            // the other side of the screen
            p0.x *= -1;
            p1.x *= -1;
        }

        // Set the birthTime to the current time
        birthTime = Time.time;
    }

    public override void Move()
    {
        // Linear interpolations work based on a u value between 0 & 1
        float u = (Time.time - birthTime) / lifeTime;

        // If u>1, then it has been longer than lifeTime since birthTime
        if (u > 1){
            // This Enemy_2 has finished its life
            Destroy(this.gameObject);
            return;
        }

        // Use the AnimationCurve to set the rotation about Y
        float shipRot  = rotCurve.Evaluate(u) * 360;
        // if (p0.x > p1.x) shipRot = -shipRot;
        // transform.rotation = Quaternion.Euler(0, shipRot, 0);
        transform.rotation = baseRotation * Quaternion.Euler(-shipRot, 0, 0);

        // Adjust u by adding a U Curve based on a Sine wave
        u = u + sinEccentricity*(Mathf.Sin(u*Mathf.PI*2));

        // Interpolate the two linear interpolation points
        pos = (1-u)*p0 + u*p1;

        // Note that Enemy_2 does NOT call the base.Move() method
    }

}
