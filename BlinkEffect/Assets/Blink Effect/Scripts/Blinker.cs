using UnityEngine;
using UnityEngine.Rendering.Universal;


/// <summary>
/// Controls the blink effect
/// </summary>
public class Blinker : MonoBehaviour
{
    [Tooltip("Custom material with _Open property")]
    public Material eyelidMaterial;

    [Tooltip("Property to control eyelids")]
    public string propertyName = "_Open";

    [Tooltip("Must match name given to RenderFeature")]
    public string renderFeatureName = "BlinkBlit";
    public ForwardRendererData rendererData;

    Blit blitFeature;

    [Tooltip("0 - closed, 1 - opened, time axis determines duration in seconds")]
    public AnimationCurve closeLidCurve;

    [Tooltip("0 - closed, 1 - opened, time axis determines duration in seconds")]
    public AnimationCurve openLidCurve;

    const float openValue = 1f;
    const float closeValue = 0f;
    float openProgress = openValue;

    bool closeAndOpen = true;

    [Tooltip("Wait time after eyes closed and before opening")]
    [Range (0f, 10f)]
    public float closedDuration = 0.0f;

    float closeTime = 1f;
    float openTime = 1f;

    System.Action onClose;
    System.Action onOpen;

    float localTime;

    enum State
    {
        Closing,
        Opening,
        WaitingForOpen,
        Idle
    }

    State state = State.Idle;

    private void Awake() {
        GetBlitFeature();
        SetUpEyelidMaterial();

        Debug.Assert(closeLidCurve != null);
        Debug.Assert(openLidCurve != null);
    }

    void SetUpEyelidMaterial(){
        Debug.Assert(eyelidMaterial != null);
        SetMaterialState();
        blitFeature.settings.blitMaterial = eyelidMaterial;
    }

    void GetBlitFeature(){
        foreach(ScriptableRendererFeature feature in rendererData.rendererFeatures){
            if(feature.name == renderFeatureName){
                Debug.Log("Found " + renderFeatureName);
                blitFeature = (Blit) feature;
            }
        }

        Debug.Assert(blitFeature != null);
    }

    void SetMaterialState(){
        eyelidMaterial.SetFloat (propertyName, openProgress);
    }

void Update ()
    {
        if (state == State.Idle)
            return;

        localTime += Time.deltaTime;

        switch (state){
            case State.Closing:
                Closing();
                break;
            case State.Opening:
                Opening();
                break;
            case State.WaitingForOpen:
                Waiting();
                break;
        }
    }

    void Closing(){
            openProgress = closeLidCurve.Evaluate (localTime);   
            SetMaterialState();         

            if (localTime > closeTime) {
                openProgress = closeValue;
                SetMaterialState();
                localTime = 0f;

                if (closeAndOpen) {
                    if (closedDuration == 0f) {
                        state = State.Opening;
                    } else 
                        state = State.WaitingForOpen;
                } else {
                    state = State.Idle;
                }
                
                if (onClose != null) {
                    onClose ();
                    onClose = null;
                }
            }
    }

    void Opening(){
            openProgress = openLidCurve.Evaluate (localTime);
            SetMaterialState();
            
            if (localTime > openTime) {
                openProgress = openValue;
                SetMaterialState();

                localTime = 0f;
                state = State.Idle;
                
                
                if (onOpen != null) {
                    onOpen ();
                    onOpen = null;
                }
            }
    }

    void Waiting(){
        if (localTime > closedDuration) {
            localTime = 0f;
            state = State.Opening;
        }
    }

    /// <summary>
    /// Run the blink effect once
    /// </summary>
    /// <param name="onComplete">Called once the blink is complete</param>
    /// <param name="onClose">Called after eyes are closed</param>
    public void Blink (System.Action onComplete = null, System.Action onClose = null)
    {
        Debug.Log("Blink");
        closeAndOpen = true;
        this.onOpen = onComplete;
        this.onClose = onClose;
        openProgress = openValue;
        localTime = 0f;
        closeTime = closeLidCurve [closeLidCurve.length - 1].time;
        openTime = openLidCurve [openLidCurve.length - 1].time;
        state = State.Closing;
    }
    
    /// <summary>
    /// Close eyelids effect
    /// </summary>
    /// <param name="onComplete">Called once eyes are fully closed</param>
    public void Close (System.Action onComplete = null)
    {
        Debug.Log("Blinker: close");
        this.onClose = onComplete;
        this.onOpen = null;
        state = State.Closing;
        closeAndOpen = false;
        closeTime = closeLidCurve [closeLidCurve.length - 1].time;
        openProgress = openValue;
        localTime = 0f;
    }
    
    /// <summary>
    /// Open eyelids effect
    /// </summary>
    /// <param name="onComplete">Called once eyes are fully opened</param>
    public void Open (System.Action onComplete = null)
    {
        Debug.Log("Blinker: open");
        this.onClose = null;
        this.onOpen = onComplete;
        state = State.Opening;
        closeAndOpen = false;
        openTime = openLidCurve [openLidCurve.length - 1].time;
        openProgress = closeValue;
        localTime = 0f;
    }
}