using UnityEngine;
public abstract class BaseState : IState
{
    protected readonly PlayerController player;
    protected readonly Animator animator;
    protected static readonly int LocomotionHash = Animator.StringToHash(name: "Locomotion");
    protected static readonly int JumpHash = Animator.StringToHash(name: "Jump");

    protected const float crossFadeDuration = 0.1f;

    protected BaseState(PlayerController player, Animator animator) {
        this.player = player;
        // animator = player.GetComponent<Animator>();
    }

    public virtual void FixedUpdate()
    {
        // noop
    }

    public virtual void OnEnter()
    {
        // noop
    }

    public virtual void OnExit()
    {
        Debug.Log("BaseSate.OnExit");
    }

    public virtual void Update()
    {
        // noop
    }
}