using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsManager : MonoBehaviour
{
// Con questo script controllo i materiali che emettono luce e le luci presenti nel livello.
    public Material[] alarmMaterials;
    // Sono i materiali che lampeggiano.
    public Material[] alarmFakeMaterials;
    // Siccome usare delle vere luci che proiettano ombre è troppo dispendioso, per rappresentare le zone illuminate dalla luce uso dei semplici pannelli
    // con applicata una texture.
    public Material[] navigationMaterials;

    public bool useFakeAlarmLights;
    // Con questa variabile si può decidere se usare le vere luci o la soluzione illustrata sopra.

    public float alarmLightIntensity;
    public float alarmLightRange;
    public float alarmLightSpotAngle;
    // Queste tre variabili modificano la luminosità e la distanza delle vere luci di emergenza.

    [Range(0.0f, 1.0f)]
    public float alarmFakeLightIntensity;
    // Questo valore controlla la trasparenza del materiale della falsa luce d'emergenza.

    public float navigationLightIntensity;
    public float navigationLightRange;
    // Per le luci bianche presenti nello scenario uso sempre delle point light che non proiettano ombre.

    public float onOffTime;
    // Velocità del lampeggio delle luci d'emergenza.

    public bool lightsOff;

    private bool on;
    private float timer;
    private float originalNavLightIntensity;
    private float originalAlLightIntensity;
    private float originalAlLightSpotAngle;
    private float originalNavLightRange;
    private float originalAlLightRange;
    private float originalFakeAlLightIntensity;
    // Siccome le luci lampeggiano, quando si "spengono" semplicemente variano questi valori, quindi li salvo in queste variabili.

    private bool resetLights;

    void Start()
    {
        originalNavLightIntensity = navigationLightIntensity;
        originalAlLightIntensity = alarmLightIntensity;
        originalFakeAlLightIntensity = alarmFakeLightIntensity;

        originalAlLightSpotAngle = alarmLightSpotAngle;

        originalNavLightRange = navigationLightRange;
        originalAlLightRange = alarmLightRange;

        if(useFakeAlarmLights)
        {
            foreach(GameObject alarmLight in GameObject.FindGameObjectsWithTag("Alarm Light"))
            {
                alarmLight.SetActive(false);
                // Se sto usando le luci false devo disattivare quelle vere,
            }
        }
        else
        {
            foreach(GameObject alarmFakeLight in GameObject.FindGameObjectsWithTag("Alarm Fake Light"))
            {
                alarmFakeLight.SetActive(false);
                // altrimenti disattivo quelle false.
            }
        }

        UpdateAlarmLightsSettings();
        UpdateNavigationLightsSettings();
        // Queste funzioni permettono di cambiare i valori di luminosità dall'editor e vederne gli effetti in realtime.

        if(lightsOff)
        {
            on = false;
        }
        else
        {
            on = true;
        }

        resetLights = true;
        timer = 0.0f;
    }
    void LateUpdate()
    {

        if(!lightsOff)
        {
            if(resetLights)
            {
                navigationLightIntensity = originalNavLightIntensity;
                UpdateNavigationLightsSettings();

                foreach(Material navigationMaterial in navigationMaterials)
                {
                    navigationMaterial.EnableKeyword("_EMISSION");
                }

                foreach(Material alarmMaterial in alarmMaterials)
                {
                    alarmMaterial.EnableKeyword("_EMISSION");
                }

                alarmLightIntensity = originalAlLightIntensity;
                alarmLightRange = originalAlLightRange;
                alarmFakeLightIntensity = originalFakeAlLightIntensity;
                UpdateAlarmLightsSettings();

                resetLights = false;
            }

            timer += Time.deltaTime;
            // Conto quanto tempo è passato tra un frame e l'altro.

            if(timer > onOffTime)
            {
            // Una volta che il timer supera il tempo di accensione o spegnimento
                if(on)
                {
                    foreach(Material alarmMaterial in alarmMaterials)
                    {
                        alarmMaterial.DisableKeyword("_EMISSION");
                        // disattivo l'emissione dei materiali
                    }

                    if(useFakeAlarmLights)
                    {
                        foreach(Material alarmFakeMaterial in alarmFakeMaterials)
                        {
                            alarmFakeMaterial.color = new Vector4(alarmFakeMaterial.color.r, alarmFakeMaterial.color.g, alarmFakeMaterial.color.b, 0.0f);
                            // e, se sto usando le luci false, (texture) le rendo trasparenti;
                        }
                    }
                    else
                    {
                        foreach(GameObject alarmLight in GameObject.FindGameObjectsWithTag("Alarm Light"))
                        {
                            alarmLight.GetComponent<Light>().intensity = 0.0f;
                            // altrimenti porto a zero la luminosità delle luci vere.
                        }
                    }

                    on = false;
                }
                else
                {
                    foreach(Material alarmMaterial in alarmMaterials)
                    {
                        alarmMaterial.EnableKeyword("_EMISSION");
                    }

                    if(useFakeAlarmLights)
                    {
                        foreach(Material alarmFakeMaterial in alarmFakeMaterials)
                        {
                            alarmFakeMaterial.color = new Vector4(alarmFakeMaterial.color.r, alarmFakeMaterial.color.g, alarmFakeMaterial.color.b, alarmFakeLightIntensity);
                        }
                    }
                    else
                    {
                        foreach(GameObject alarmLight in GameObject.FindGameObjectsWithTag("Alarm Light"))
                        {
                            alarmLight.GetComponent<Light>().intensity = alarmLightIntensity;
                        }
                    }
                    // Quando bisogna riaccendere le luci, riporto i valori modificati in precedenza a quelli originali.

                on = true;
                }

                timer = 0.0f;

                if(originalAlLightIntensity != alarmLightIntensity || originalAlLightRange != alarmLightRange || originalAlLightSpotAngle != alarmLightSpotAngle)
                {
                // Se ho cambiato i parametri dello script dall'editor (ma la stessa funzione può essere utilizzata da altri script per aggiungere ulteriori effetti al gioco,
                // come ad esempio lo sfarfallio delle luci poste entro una certa distanza dei nemici) mentre sono in gioco. 
                    UpdateAlarmLightsSettings();
                    originalAlLightIntensity = alarmLightIntensity;
                    originalAlLightRange = alarmLightRange;
                    originalAlLightSpotAngle = alarmLightSpotAngle;
                }
                if(originalNavLightIntensity != navigationLightIntensity || originalNavLightRange != navigationLightRange)
                {
                    UpdateNavigationLightsSettings();
                    originalNavLightIntensity = navigationLightIntensity;
                    originalNavLightRange = navigationLightRange;
                }
            }
        }

        else
        {
            alarmLightIntensity = 0.0f;
            alarmFakeLightIntensity = 0.0f;
            UpdateAlarmLightsSettings();
            foreach(Material alarmMaterial in alarmMaterials)
            {
                alarmMaterial.DisableKeyword("_EMISSION");
            }

            navigationLightIntensity = 0.0f;
            UpdateNavigationLightsSettings();
            foreach(Material navigationMaterial in navigationMaterials)
            {
                    navigationMaterial.DisableKeyword("_EMISSION");
            }

            resetLights = true;
        }
    }

    private void UpdateNavigationLightsSettings()
    {
        foreach(GameObject navigationLight in GameObject.FindGameObjectsWithTag("Navigation Light"))
        {
            Light nL = navigationLight.GetComponent<Light>();
            nL.type = LightType.Point;

            nL.GetComponent<Light>().intensity = navigationLightIntensity;
            nL.GetComponent<Light>().range = navigationLightRange;
        }
    }

    private void UpdateAlarmLightsSettings()
    {
        foreach(GameObject alarmLight in GameObject.FindGameObjectsWithTag("Alarm Light"))
        {
            Light aL = alarmLight.GetComponent<Light>();
            aL.type = LightType.Spot;

            aL.intensity = alarmLightIntensity;
            aL.range = alarmLightRange;
            aL.spotAngle = alarmLightSpotAngle;
        }
    }

    public void LightsOff(bool lightsSwitch)
    {
        lightsOff = lightsSwitch;
    }
}
