using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using Random = System.Random;

public class RocketManager : MonoBehaviour
{
    [Header("References")]
    public Transform rocketTransform;
    public Transform cameraTransform;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI crashText;
    public Button cashOutButton;
    public GameObject explosionPrefab;

    [Header("Gameplay")]
    public float initialMultiplier = 1.0f;
    public float crashPoint = 7.3f;
    public float multiplierIncreaseInterval = 0.1f;
    public float timePerStep = 0.1f;
    public float xUnitsPerSecond = 1.0f;
    public float yUnitsPerMultiplier = 1.0f;
    public float cameraZ = -10f;
    public Ease stepEase = Ease.Linear;
    
    [Header("Offsets")]
    public float startOffsetX = 0f;
    public float startOffsetY = 0f;
    public float startMovementScaleX = 1f;
    public float startMovementScaleY = 1f;

    private float currentMultiplier;
    private bool crashed = false;
    private bool cashedOut = false;
    private Sequence rocketSequence;

    [SerializeField] private LineRenderer lineRenderer;
    
    private void Start()
    {
        currentMultiplier = initialMultiplier;
        crashText.gameObject.SetActive(false);
        cashOutButton.onClick.AddListener(OnCashOutClicked);

        crashPoint = UnityEngine.Random.Range(1f, 7.5f);

        Vector3 startPos = MultiplierToWorldPosition(currentMultiplier, 0);
        rocketTransform.position = startPos;
        //cameraTransform.position = new Vector3(startPos.x, startPos.y, cameraZ);

        StartRocketSequence();
    }

    Vector3 MultiplierToWorldPosition(float multiplier, float elapsedSeconds)
    {
        float x = (startOffsetX + elapsedSeconds * xUnitsPerSecond) * startMovementScaleX;
        float y = (startOffsetY + multiplier * yUnitsPerMultiplier - 1) * startMovementScaleY;
        return new Vector3(x, y, 0);
    }

    void StartRocketSequence()
    {
        float totalDuration = ((crashPoint - currentMultiplier) / multiplierIncreaseInterval) * timePerStep;
        float elapsedTime = 0f;

        rocketSequence = DOTween.Sequence();

        rocketSequence.Append(
            DOTween.To(() => currentMultiplier, m => currentMultiplier = m, crashPoint, totalDuration)
                .SetEase(Ease.Linear) // keep multiplier linear
                .OnUpdate(() =>
                {
                    // Normalized time 0..1
                    float t = Mathf.Clamp01(elapsedTime / totalDuration);

                    // Apply cubic ease-in
                    float easedT = 1 - Mathf.Pow(1 - t, 3);

                    // Map eased time to world position
                    float easedElapsedSeconds = easedT * totalDuration;
                    Vector3 rocketPos = MultiplierToWorldPosition(currentMultiplier, easedElapsedSeconds);
                    
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, rocketTransform.position);

                    // Move rocket
                    rocketTransform.position = rocketPos;

                    // Rotate toward movement direction
                    Vector3 dir = MultiplierToWorldPosition(currentMultiplier, easedElapsedSeconds + 0.01f) - rocketPos;
                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        rocketTransform.rotation = Quaternion.Euler(0, 0, angle);
                    }

                    // Update UI
                    multiplierText.text = $"{currentMultiplier:0.00}x";

                    // Check crash
                    if (currentMultiplier >= crashPoint && !crashed && !cashedOut)
                    {
                        HandleCrash(rocketPos);
                    }

                    // Advance manual time
                    elapsedTime += Time.deltaTime;
                })
        );
    }

    void HandleCrash(Vector3 crashPos)
    {
        crashed = true;
        rocketSequence.Kill();

        crashText.gameObject.SetActive(true);
        crashText.text = $"Crashed at {currentMultiplier:0.00}x 💥";
        crashText.color = Color.red;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, crashPos, Quaternion.identity);
        }

        cameraTransform.DOShakePosition(0.6f, new Vector3(0.3f, 0.3f, 0), 10, 90, true);
    }

    void OnCashOutClicked()
    {
        if (crashed || cashedOut) return;
        cashedOut = true;
        rocketSequence.Kill();

        crashText.gameObject.SetActive(true);
        crashText.text = $"Cashed Out at {currentMultiplier:0.00}x ✅";
        crashText.color = Color.green;
    }
}
