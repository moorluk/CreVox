using UnityEngine;
using System.Collections;
namespace Invector.CharacterController
{
    public class TopDownCursor : MonoBehaviour
    {
        public vThirdPersonInput tpInput;
        public GameObject cursorObject;
        private Vector3 _scale, currentScale;
        public float scale, speed;
        private float time;
        private bool enableCursor;

        void Start()
        {
            if (!tpInput) Destroy(gameObject);
            tpInput.onEnableCursor = Enable;
            tpInput.onDisableCursor = Disable;
            _scale = cursorObject.transform.localScale;
        }

        void Update()
        {
            if (enableCursor)
            {
                time += speed * Time.deltaTime;
                currentScale.x = Mathf.PingPong(time, _scale.x + scale);
                currentScale.x = Mathf.Clamp(currentScale.x, _scale.x, _scale.x + scale);
                currentScale.y = Mathf.PingPong(time, _scale.y + scale);
                currentScale.y = Mathf.Clamp(currentScale.y, _scale.y, _scale.y + scale);
                currentScale.z = Mathf.PingPong(time, _scale.z + scale);
                currentScale.z = Mathf.Clamp(currentScale.z, _scale.z, _scale.z + scale);
                cursorObject.transform.localScale = currentScale;
            }
        }

        public bool Near(Vector3 pos, float dst)
        {
            var a = new Vector3(pos.x, 0, pos.z);
            var b = new Vector3(transform.position.x, 0, transform.position.z);
            return (Vector3.Distance(a, b) < dst);
        }

        public void Enable(Vector3 position)
        {
            transform.position = position;
            cursorObject.SetActive(true);
            enableCursor = true;
        }

        public void Disable()
        {
            cursorObject.SetActive(false);
            enableCursor = false;
        }
    }
}
