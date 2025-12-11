using UnityEngine;

namespace FoliageRenormalizer.Demo
{
    public class ToggleTerrain : MonoBehaviour
    {
        public GameObject RenormalizedFoliageTerrain;
        public GameObject ImportedFoliageTerrain;
        public GameObject RenormalizedFoliageText;
        public GameObject ImportedFoliageText;
        private bool ShowRenormalized = true;
        private bool highDensity = false;

        private void Start()
        {
            ToggleDensity();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ShowRenormalized = !ShowRenormalized;
                RenormalizedFoliageTerrain.SetActive(ShowRenormalized);
                RenormalizedFoliageText.SetActive(ShowRenormalized);
                ImportedFoliageTerrain.SetActive(!ShowRenormalized);
                ImportedFoliageText.SetActive(!ShowRenormalized);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                highDensity = !highDensity;
                ToggleDensity();
            }
        }

        void ToggleDensity()
        {
            RenormalizedFoliageTerrain.GetComponent<Terrain>().detailObjectDensity = highDensity ? 1f : .5f;
            ImportedFoliageTerrain.GetComponent<Terrain>().detailObjectDensity = highDensity ? 1f : .5f;
        }
    }
}