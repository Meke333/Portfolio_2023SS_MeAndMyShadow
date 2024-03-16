
using UnityEngine;
using UnityEngine.Rendering;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class AutoLevelBuilder : MonoBehaviour
{
    private GameObject[] foreground;
    private GameObject foregroundObject_graphic;

    [SerializeField] private Material shadow_material;
    [SerializeField] private Material indicator_material;

    [SerializeField] private bool isUseLightProbesOn = false;

    [SerializeField] private LayerMask shadow_layerMask;

    [SerializeField] private Transform zPos;

    private void Start()
    {
        foreground = GameObject.FindGameObjectsWithTag("ForegroundObjects");

        foreach (GameObject foregroundObject in foreground)
        {
            foregroundObject_graphic = foregroundObject.transform.GetChild(0).transform.GetChild(0).gameObject; //In '-Graphic' the GameObject

            //     IsOverrideThere?
            IOverrideAutoLevelBuilderSettings overrideAutoLevelBuilderSettings = foregroundObject_graphic.GetComponent<IOverrideAutoLevelBuilderSettings>();

            
            
            //real Gameobjects
            GameObject shadowObject = new GameObject("ShadowObject_Graphic", typeof(MeshFilter), typeof(MeshRenderer));
            GameObject indicatorObject = new GameObject("IndicatorObject_Graphic",typeof(MeshFilter), typeof(MeshRenderer));
            
            //emptys
            GameObject empty_shadowObject = new GameObject("ShadowObject");
            GameObject empty_indicatorObject = new GameObject("indicatorObject");
            
            empty_indicatorObject.SetActive(false);
            
            //Rotation & Scale
            Quaternion rotation = foregroundObject.transform.rotation;
            Vector3 localScale = foregroundObject_graphic.transform.localScale;
            shadowObject.transform.rotation = rotation;
            shadowObject.transform.localScale = localScale;
            indicatorObject.transform.rotation = rotation;
            indicatorObject.transform.localScale = localScale;

            //SetParent
            shadowObject.transform.SetParent(empty_shadowObject.transform);
            indicatorObject.transform.SetParent(empty_indicatorObject.transform);
            
            empty_shadowObject.transform.SetParent(foregroundObject.transform);
            empty_indicatorObject.transform.SetParent(foregroundObject.transform);

            //Transform to Parent
            Vector3 foreGroundObjectPosition = foregroundObject.transform.position;
            empty_shadowObject.transform.position = new Vector3(foreGroundObjectPosition.x, foreGroundObjectPosition.y, zPos.position.z);
            empty_indicatorObject.transform.position = new Vector3(foreGroundObjectPosition.x, foreGroundObjectPosition.y, zPos.position.z);
            
            //Tags & Layers
            shadowObject.tag = "ShadowObjects";
            indicatorObject.tag = "IndicatorObjects";
            
            empty_shadowObject.tag = "ShadowObjects";
            empty_indicatorObject.tag = "IndicatorObjects";

            shadowObject.layer = LayerMask.NameToLayer("Interactable_Shadow");
            indicatorObject.layer = LayerMask.NameToLayer("Interactable_Indicator");
            
            empty_shadowObject.layer = LayerMask.NameToLayer("Interactable_Shadow");
            empty_indicatorObject.layer = LayerMask.NameToLayer("Interactable_Indicator");
            
            
            
            indicatorObject.GetComponent<MeshFilter>().mesh = foregroundObject_graphic.GetComponent<MeshFilter>().mesh;
            
            //MeshFilter + MeshRenderer
            MeshRenderer meshRenderer_foreground = foregroundObject_graphic.GetComponent<MeshRenderer>();
            
            
                //NO MESHRENDERER FOR GLASS!!!
            if (overrideAutoLevelBuilderSettings == null)
            {
                shadowObject.GetComponent<MeshFilter>().mesh = foregroundObject_graphic.GetComponent<MeshFilter>().mesh;
                
                MeshRenderer meshRenderer_shadow = shadowObject.GetComponent<MeshRenderer>();
                
                meshRenderer_shadow.material = shadow_material;
                meshRenderer_shadow.shadowCastingMode = meshRenderer_foreground.shadowCastingMode;
                if (!isUseLightProbesOn)
                    meshRenderer_shadow.lightProbeUsage = LightProbeUsage.Off;
            }
            
            MeshRenderer meshRenderer_indicator = indicatorObject.GetComponent<MeshRenderer>();
            
            meshRenderer_indicator.material = indicator_material;
            meshRenderer_indicator.shadowCastingMode = meshRenderer_foreground.shadowCastingMode;
            if (!isUseLightProbesOn)
                meshRenderer_indicator.lightProbeUsage = LightProbeUsage.Off;

            
                //NO COLLISION FOR GLASS!!!
            if (overrideAutoLevelBuilderSettings != null)
            {
                Destroy(shadowObject.GetComponent<MeshFilter>());
                
                continue;
            }
            
            //Debug.Log("No overrideAutoBuilderSettings Found!!!");
            
            //Death Collider for Shadow Objects
            //GameObject DeathCollider = new GameObject("DeathTrigger", typeof(MeshFilter));
            
            
            //DeathCollider.transform.SetParent(shadowObject.transform);
            //DeathCollider.transform.position = shadowObject.transform.position;
            //DeathCollider.transform.rotation = shadowObject.transform.rotation;
            //DeathCollider.transform.localScale = shadowObject.transform.localScale;
            
            //MeshFilter death_meshFilter = DeathCollider.GetComponent<MeshFilter>();
            //death_meshFilter.mesh = shadowObject.GetComponent<MeshFilter>().mesh;
            
            //DeathCollider.AddComponent<TriggerEventBehavior>();
            //TriggerEventBehavior triggerEventBehavior = DeathCollider.GetComponent<TriggerEventBehavior>();
            
            //Redundant because Implementation not placing Block if Player is intersecting with Object during Unselecting 
            //triggerEventBehavior.triggeringGameState = GameState.PlayerDead;
            //triggerEventBehavior.comparableTag = ComparableTag.Player;
            
            //Debug.Log("AAAAAA" + overrideAutoLevelBuilderSettings == null + " " + foregroundObject_graphic.name);
            
            //Death
            
            /*BoxCollider Death_BoxCollider = DeathCollider.AddComponent<BoxCollider>();
            
            
            Death_BoxCollider.isTrigger = true;
            DeathCollider.transform.localScale = Vector3.one;
            */

            //Collider

            if (foregroundObject_graphic.GetComponent<BoxCollider>() != null)
            {
                shadowObject.AddComponent<BoxCollider>();
                BoxCollider foreGroundCollider = foregroundObject_graphic.GetComponent<BoxCollider>();
                BoxCollider shadowCollider = shadowObject.GetComponent<BoxCollider>();
                shadowCollider.excludeLayers = shadow_layerMask;

                //BoxCollider indicatorCollider = indicatorObject.AddComponent<BoxCollider>();
                //indicatorCollider.isTrigger = true;


                //Death_BoxCollider.size = shadowCollider.size;

            }
            else if (foregroundObject_graphic.GetComponent<CapsuleCollider>() != null)
            {
                shadowObject.AddComponent<CapsuleCollider>();
                CapsuleCollider foregroundCollider = foregroundObject_graphic.GetComponent<CapsuleCollider>();
                CapsuleCollider shadowCollider = shadowObject.GetComponent<CapsuleCollider>();
                shadowCollider.excludeLayers = shadow_layerMask;
                

            }
            else //MeshCollider
            {
                shadowObject.AddComponent<MeshCollider>();
                MeshCollider foregroundCollider = foregroundObject_graphic.GetComponent<MeshCollider>();
                MeshCollider shadowCollider = shadowObject.GetComponent<MeshCollider>();
                shadowCollider.convex = true;
                shadowCollider.excludeLayers = shadow_layerMask;
                
                //Destroy(Death_BoxCollider);

            }
            
            //Add Script to Indicator
            //indicatorObject.AddComponent<SelectedObjectTrigger>();

            //DeathCollider.transform.localScale *= 0.5f;

        }
        
        Destroy(this.gameObject);
    }
    
}
