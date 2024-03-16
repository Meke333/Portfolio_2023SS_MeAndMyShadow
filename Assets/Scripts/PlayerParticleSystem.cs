
using UnityEngine;

public class PlayerParticleSystem : MonoBehaviour
{
    private ParticleSystem _landingParticleSystem;
    private Vector3 _positionOnPlayer;
    private Transform _playerPosition;
    [Header("Animation Curve for Particle Size per Airborne Time")]
    [Tooltip("Set the Intensity of the Particle, dependent on the time, between Jumping & Landing; MAX: 1")]
    [SerializeField] private AnimationCurve particleSize; //max
    
    private void Awake()
    {
        _landingParticleSystem = GetComponent<ParticleSystem>();
        _positionOnPlayer = this.gameObject.transform.localPosition;

        GameObject newParent = GameObject.Find("Jump&Run Layer").gameObject;
        
        if (newParent != null)
            transform.SetParent(newParent.transform,true);

        _playerPosition = GameObject.FindWithTag("Player").transform;

    }

    private void OnEnable()
    {
        FigureCharacterController.OnLanding += PlayLandingParticle;
    }

    private void OnDisable()
    {
        FigureCharacterController.OnLanding -= PlayLandingParticle;
    }

    void PlayLandingParticle(float airBorneTime)
    {
        //transform max: 0.2, min: 0
        float size = particleSize.Evaluate(airBorneTime);


        Vector3 newSize = Vector3.one * (size * 0.2f);
        transform.localScale = newSize;
        transform.GetChild(0).transform.localScale = newSize;
        
        transform.position = _playerPosition.position + _positionOnPlayer;
        _landingParticleSystem.Play();
    }
}
