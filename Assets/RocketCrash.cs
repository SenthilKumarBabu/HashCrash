using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RocketCrash : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI crashText;
    public Button cashOutButton;
    public LineRenderer lineRenderer;

    [Header("Rocket")] 
    public Transform rocketTransform;

    [Header("Settings")] 
    public float multiplierIncreaseInterval = 0.5f; // Seconds for +1 multiplier
    public float crashPoint = 10f; // Randomized in Start()
    public float rocketMoveSpeed = 1f; // Units per multiplier step
    public Vector2 rocketMovementMultiplier;
    
    private float currentMultiplier = 1f;
    private bool crashed = false;
    private bool cashedOut = false;

    private Vector3 startPos = Vector3.zero;

    void Start()
    {
        crashText.gameObject.SetActive(false);
        //crashPoint = Random.Range(2f, 15f); // Random crash value
        cashOutButton.onClick.AddListener(CashOut);

        lineRenderer.positionCount = 0;
        //rocketTransform.position = startPos;
        
        crashed = false;
        cashedOut = false;
        currentMultiplier = 1.00f;
        
        multiplierText.text = $"{currentMultiplier}x";

        RocketStart();
    }

    private void RocketStart()
    {
        float steps = (crashPoint - currentMultiplier) / multiplierIncreaseInterval;

        DOTween.Sequence()
            .AppendInterval(rocketMoveSpeed)
            .AppendCallback(() =>
            {
                currentMultiplier += multiplierIncreaseInterval;
                
                var nextPoint = new Vector3(
                    currentMultiplier * rocketMoveSpeed * rocketMovementMultiplier.x,
                    currentMultiplier * rocketMovementMultiplier.y,
                    0
                );
                
                Vector3 target = rocketTransform.position;
                target.z = nextPoint.z;
                //rocketTransform.DOLookAt(target, multiplierIncreaseInterval);
                rocketTransform.DOMove(nextPoint, multiplierIncreaseInterval).SetEase(Ease.Linear);

                Debug.Log($"{currentMultiplier} {rocketTransform.position}");
            })
            .SetLoops(Mathf.CeilToInt(steps), LoopType.Restart)
            .OnComplete(Crash);
    }
    
    /*
    lineRenderer.positionCount++;
    lineRenderer.SetPosition(lineRenderer.positionCount - 1, rocketTransform.position);*/
    private void CashOut()
    {
        if (!crashed && !cashedOut)
        {
            cashedOut = true;
            crashText.gameObject.SetActive(true);
            crashText.text = $"Cashed Out at {currentMultiplier:0.00}x ✅";
            crashText.color = Color.green;
        }
    }

    private void Crash()
    {
        crashed = true;
        crashText.gameObject.SetActive(true);
        crashText.text = $"Crashed at {currentMultiplier:0.00}x 💥";
        crashText.color = Color.red;
    }
}