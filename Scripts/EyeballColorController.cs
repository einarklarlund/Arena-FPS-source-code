using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeballColorController : MonoBehaviour
{ 
    public Material outerMaterial;
    public Material irisMaterial;
    public Material pupilMaterial;
    public Material skinMaterial;

    public MeshRenderer meshRenderer;
    public Color damageColor;
    public float damageDuration;

    private List<Color> _originalColors;
    private float damageTime;

    // Start is called before the first frame update
    void Start()
    {
        _originalColors = new List<Color>();

        outerMaterial = meshRenderer.materials[0];
        irisMaterial = meshRenderer.materials[1];
        pupilMaterial = meshRenderer.materials[2];
        skinMaterial = meshRenderer.materials[3];

        _originalColors.Add(outerMaterial.GetColor("_Color1"));
        _originalColors.Add(irisMaterial.GetColor("_Color1"));
        _originalColors.Add(pupilMaterial.GetColor("_Color2"));
        _originalColors.Add(skinMaterial.GetColor("_Color1"));

        damageDuration = Time.fixedDeltaTime * 6;
    }

    // Update is called once per frame
    void Update()
    {
        if(damageTime != 0f && Time.time - damageTime >= damageDuration)
        {
            outerMaterial.SetColor("_Color1", _originalColors[0]);
            irisMaterial.SetColor("_Color1", _originalColors[1]);
            pupilMaterial.SetColor("_Color2", _originalColors[2]);
            skinMaterial.SetColor("_Color1", _originalColors[3]);

            damageTime = 0f;
        }
    }

    public void TakeDamage()
    {
        outerMaterial.SetColor("_Color1", damageColor);
        irisMaterial.SetColor("_Color1", damageColor);
        pupilMaterial.SetColor("_Color2", damageColor);
        skinMaterial.SetColor("_Color1", damageColor);

        damageTime = Time.time;
    }
}
