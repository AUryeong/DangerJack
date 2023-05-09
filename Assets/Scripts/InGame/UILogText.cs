using Photon.Pun;
using UnityEngine;

public class UILogText : MonoBehaviourPun
{
    [SerializeField] private float duration;
    private float time;
    private Animator animator;
    private static readonly int textAnimationHash = Animator.StringToHash("Text");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        time = 0;
        animator.SetTrigger(textAnimationHash);
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= duration)
            gameObject.SetActive(false);
    }
}