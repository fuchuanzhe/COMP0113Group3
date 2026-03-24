using UnityEngine;

public class FireworkController : MonoBehaviour
{
    public float launchHeight = 6f;
    public float launchDuration = 1.2f;
    public AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public ParticleSystem rocketTrail;
    public ParticleSystem explosion;

    public Gradient fireworkGradient;
    public float destroyDelayAfterExplode = 3f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer;
    private bool exploded;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * launchHeight;

        if (rocketTrail != null)
            rocketTrail.Play();

        ApplyColour();
    }

    void Update()
    {
        if (exploded) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / launchDuration);
        float curvedT = heightCurve.Evaluate(t);

        transform.position = Vector3.Lerp(startPos, targetPos, curvedT);

        if (t >= 1f)
            Explode();
    }

    void ApplyColour()
    {
        if (explosion == null) return;

        Color chosenColor = fireworkGradient != null
            ? fireworkGradient.Evaluate(Random.value)
            : Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);

        var main = explosion.main;
        main.startColor = chosenColor;
    }

    void Explode()
    {
        exploded = true;

        if (rocketTrail != null)
            rocketTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (explosion != null)
            explosion.Play();

        Destroy(gameObject, destroyDelayAfterExplode);
    }
}
