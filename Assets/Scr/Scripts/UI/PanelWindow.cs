using UnityEngine;

public class PanelWindow : MonoBehaviour
{
    public bool on = false;

    [Space]
    /// <summary>
    /// true: tiene que haber un animator asignado al mismo objeto con el parametro "On"
    /// false: controlará si el primer objeto hijo (index 0) está activo o no
    /// </summary>
    public bool useAnimator;
    private bool wasUsingAnimator;

    private Animator anim;
    private Transform child;

    private void Awake()
    {
        GetReferences();
    }

    public void SetState(bool state)
    {
        on = state;
    }

    private void GetReferences()
    {
        if (useAnimator)
        {
            anim = GetComponent<Animator>();
        }
        else
        {
            child = transform.GetChild(0);
        }
    }

    private void Update()
    {
        #region CheckUseAnimatorChanges
        if (useAnimator != wasUsingAnimator)
        {
            GetReferences();
        }

        wasUsingAnimator = useAnimator; 
        #endregion
    
        if (useAnimator)
        {
            UseAnimatorLogic();
        }
        else
        {
            UseChildLogic();
        }
    }

    private void UseAnimatorLogic()
    {
        if (anim == null) return;

        anim.SetBool("On", on);
    }

    private void UseChildLogic()
    {
        if (child == null) return;

        child.gameObject.SetActive(on);
    }
}
