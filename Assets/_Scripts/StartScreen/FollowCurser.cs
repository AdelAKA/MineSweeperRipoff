using UnityEngine;

namespace MineSweeperRipeoff
{
    public class FollowCurser : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = 10;
            transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }
    }
}