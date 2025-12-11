using System.Collections.Generic;
using UnityEngine;

namespace FoliageRenormalizer.Demo
{
    public class ToggleProxyMeshes : MonoBehaviour
    {
#if UNITY_EDITOR
        private bool ShowProxyMeshes = false;
        private FoliageRenormalizerUtility[] allRenormalizers;
        private List<GameObject> SpawnedProxyMeshes = new List<GameObject>();

        private void Awake()
        {
            allRenormalizers = FindObjectsByType<FoliageRenormalizerUtility>(FindObjectsSortMode.None);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                ShowProxyMeshes = !ShowProxyMeshes;
                SetProxyMeshes();
            }
        }

        void SetProxyMeshes()
        {
            Material mat = (Material)Resources.Load("Show Normals");
            if (SpawnedProxyMeshes.Count > 0)
            {
                foreach(GameObject obj in SpawnedProxyMeshes)
                    Destroy(obj);
                SpawnedProxyMeshes.Clear();
            }
            else
            {
                foreach (var utility in allRenormalizers)
                {
                    if (utility.GeneratedProxyMesh != null)
                    {
                        MeshRenderer newRenderer = new GameObject().AddComponent<MeshRenderer>();
                        MeshFilter newFilter = newRenderer.gameObject.AddComponent<MeshFilter>();
                        newFilter.sharedMesh = utility.GeneratedProxyMesh;
                        newRenderer.sharedMaterial = mat;
                        newRenderer.transform.position = utility.transform.position;
                        newRenderer.transform.rotation = utility.transform.rotation;
                        SpawnedProxyMeshes.Add(newRenderer.gameObject);
                    }
                }
            }
        }
#endif
    }
}