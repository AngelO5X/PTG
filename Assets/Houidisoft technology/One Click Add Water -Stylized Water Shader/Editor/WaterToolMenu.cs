using UnityEditor;
using UnityEngine;

namespace YourNamespace.Tools
{
    public class WaterToolMenu : MonoBehaviour
    {
        private const string DefaultMaterialName = "water";
        private const string WaterShaderPath = "Shader Graphs/water shader ocean";

        [MenuItem("Tools/Add Water Surface")]
        public static void AddWaterSurface()
        {
            Material defaultWaterMaterial = Resources.Load<Material>(DefaultMaterialName);
            if (defaultWaterMaterial == null)
            {
                Debug.LogError($"Default material '{DefaultMaterialName}' not found in Resources folder. Please add it.");
                return;
            }

            Shader waterShader = Shader.Find(WaterShaderPath);
            if (waterShader == null)
            {
                Debug.LogError($"Shader Graph '{WaterShaderPath}' not found! Please ensure it's correctly set up in the project.");
                return;
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                Debug.LogError("No active Scene View found! Please open the Scene View.");
                return;
            }

            Camera sceneCamera = sceneView.camera;
            Vector3 cameraPosition = sceneCamera.transform.position;
            Vector3 cameraForward = sceneCamera.transform.forward;
            Vector3 waterPosition = cameraPosition + cameraForward * 10f;

            GameObject waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane.name = "Water Surface";
            waterPlane.transform.position = waterPosition;
            waterPlane.transform.localScale = new Vector3(10, 1, 10);

            Material newMaterial = new Material(defaultWaterMaterial)
            {
                name = "Water Material " + System.DateTime.Now.Ticks
            };

            newMaterial.shader = waterShader;

            Renderer renderer = waterPlane.GetComponent<Renderer>();
            renderer.material = newMaterial;

            Selection.activeGameObject = waterPlane;

            Debug.Log($"Water surface added at {waterPosition} with new material: {newMaterial.name}, using Shader Graph: {waterShader.name}");
        }
    }
}