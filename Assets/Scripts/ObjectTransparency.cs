using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectTransparency : MonoBehaviour
{
    private GameObject _player;
    private GameObject _objectToTransparent;
    private Ray _cameraToPlayer;
    private RaycastHit _raycastHit;
    private LayerMask _objectsToChecklayersWith;
    private Camera mainCamera;

    #region Material

    private Material _objectMaterial;
    private Color _objectColor;
    private float _targetTransparency;

    #endregion

    #region Adjustable

    [SerializeField] private AnimationCurve _transparencyAnimationCurve;
    [SerializeField] private float minAlpha;
    private float maxAlpha = 255;

    #endregion

    private bool isFinishedAnimating;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
        _objectsToChecklayersWith = LayerMask.GetMask("Interactable_Mid_Foreground");
        mainCamera = Camera.main;
        isFinishedAnimating = false;
    }

    
    
    private void FixedUpdate()
    {
        _cameraToPlayer.direction = mainCamera.transform.position - _player.transform.position;
        if (Physics.Raycast(_cameraToPlayer, out _raycastHit, 100, _objectsToChecklayersWith))
        {
            if (_raycastHit.transform.gameObject.Equals(_objectToTransparent))
                return;
            else
            {
                if (_objectToTransparent != null)
                {
                    isFinishedAnimating = true;
                    ChangeTransparencyOfColor(minAlpha,maxAlpha, true);
                }
                
            }

        }
        
        
    }

    async void ChangeTransparencyOfColor(float startAlpha, float targetAlpha, bool isgettingNonTransparent)
    {
        float changeSpeed = 2f;
        float currentStep = 0; // da stimmt nicht alles
        float stepsToTargetAlpha = startAlpha - targetAlpha;
        int direction = isgettingNonTransparent? 1 : -1;
        stepsToTargetAlpha = Math.Abs(stepsToTargetAlpha);
        float currentTransparency = startAlpha;
        

        while (currentStep <= stepsToTargetAlpha)
        {
            currentTransparency = _transparencyAnimationCurve.Evaluate(currentStep);
            

            currentStep += Time.deltaTime * changeSpeed;
            await Task.Yield();
        }

        isFinishedAnimating = true;
    }

    void GetMaterialOfObject(GameObject gameObject)
    {
        _objectMaterial = gameObject.GetComponent<Material>();
        _objectColor = _objectMaterial.color;

    }
}
