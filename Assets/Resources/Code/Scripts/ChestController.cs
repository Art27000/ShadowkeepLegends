using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer highlightEffect;
    [SerializeField] private SpriteRenderer textSprite; // Спрайт с текстом "Press E"

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 2f;

    private Transform player;
    private bool isPlayerNear = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Debug.Log(player);
        HideEffects();
    }

    void Update()
    {
        CheckPlayerDistance();

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool newState = distance <= interactionDistance;

        if (isPlayerNear != newState)
        {
            isPlayerNear = newState;
            UpdateEffects();
        }
    }
    void UpdateEffects()
    {
        if (highlightEffect != null)
            highlightEffect.enabled = isPlayerNear;

        if (textSprite != null)
            textSprite.enabled = isPlayerNear;
    }

    void HideEffects()
    {
        if (highlightEffect != null)
            highlightEffect.enabled = false;

        if (textSprite != null)
            textSprite.enabled = false;
    }

    void OpenChest()
    {
        if (TryGetComponent<Animator>(out var animator))
        {
            animator.SetBool("IsOpened", true);
        }
        HideEffects();
        enabled = false;
    }
}