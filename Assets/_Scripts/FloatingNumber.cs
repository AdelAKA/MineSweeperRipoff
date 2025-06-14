using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class FloatingNumber : MonoBehaviour
    {
        [SerializeField] float duration = 1;
        [SerializeField] Vector2 speedRange;
        [SerializeField] Image iconImage;

        private Vector3 direction;
        private float speed;
        private const float SCALE_SPEED_MODIFIER = 0.005f;

        public void Initialize(Sprite icon)
        {
            iconImage.sprite = icon;
            speed = Random.Range(speedRange.x, speedRange.y);
            direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            //if (Random.value < 0.5f) direction.x *= -1;
            //if (Random.value < 0.5f) direction.y *= -1;
            direction.Normalize();
            transform.localScale = Vector2.zero;
        }

        // Update is called once per frame
        void Update()
        {
            duration -= Time.deltaTime;
            if (duration <= 0) Destroy(gameObject);
            transform.position += direction * speed * Time.deltaTime;
            Vector3 newScale = transform.localScale + Vector3.one * speed * SCALE_SPEED_MODIFIER * Time.deltaTime;
            newScale.x = Mathf.Clamp(newScale.x, 0f, 2f);
            newScale.y = Mathf.Clamp(newScale.y, 0f, 2f);
            newScale.z = 1;
            transform.localScale = newScale;
        }
    }
}
