using UnityEngine;

namespace Growth
{
    [AddComponentMenu("Growth/Wind Controller")]
    [RequireComponent(typeof(WindZone))]
    [ExecuteAlways]
    public class WindController : MonoBehaviour
    {
        private WindZone zone;
        private WindZone Zone
        {
            get
            {
                zone ??= GetComponent<WindZone>();
                return zone;
            }
        }

        [SerializeField] private Texture2D noise;
        [SerializeField, Range(0, 5)] private float strength = .2f;
        [SerializeField, Range(0, 15)] private float speed = 5f;

        private void Update()
        {
            Vector3 fullDirection = Zone.transform.forward;
            Vector2 direction = new Vector2(fullDirection.x, fullDirection.z).normalized;
            Vector2 packedDirection = direction * strength;
            Shader.SetGlobalVector("_Growth_WindSettings", new Vector4(packedDirection.x, packedDirection.y, Zone.windMain * speed, Zone.windTurbulence));
            Shader.SetGlobalTexture("_Growth_WindNoise", noise);
        }
    }
}
