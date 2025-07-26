using UnityEngine;

public class TwoStepAnimation : MonoBehaviour
{
    [SerializeField] float stepDuration = 0.1f;

    [SerializeField] bool EnablePositionAnimation;
    [SerializeField] Vector3 StartPosition = Vector3.zero;
    [SerializeField] Vector3 EndPosition = Vector3.zero;

    [SerializeField] bool EnableRotationAnimation;
    [SerializeField] Vector3 StartRotation = Vector3.zero;
    [SerializeField] Vector3 EndRotation = Vector3.zero;

    [SerializeField] bool EnableScaleAnimation;
    [SerializeField] Vector3 StartScale = Vector3.one;
    [SerializeField] Vector3 EndScale = Vector3.one;

    private bool isStartState;
    private float timer;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= stepDuration)
        {
            if (EnablePositionAnimation)
            {
                transform.localPosition = isStartState ? StartPosition : EndPosition;
            }
            if (EnableRotationAnimation)
            {
                transform.localRotation = isStartState ? Quaternion.Euler(StartRotation) : Quaternion.Euler(EndRotation);
            }
            if (EnableScaleAnimation)
            {
                transform.localScale = isStartState ? StartScale : EndScale;
            }

            isStartState = !isStartState;
            timer = 0;
        }
    }
}
