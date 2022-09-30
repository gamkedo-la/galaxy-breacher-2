using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TargettingHud : MonoBehaviour
{

    List<Target> activeTargets;

    Target selectedTarget;

    [SerializeField] Image targetPrefab;
    [SerializeField] Color inactiveTargetColor;
    [SerializeField] Color activeTargetColor;
    [SerializeField] GameObject playerGO;

    Dictionary<Target, Image> hudImages;

    [Tooltip("The min and max size of the target indicator.")]
    [SerializeField] Vector2 targetHudSizeRange;
    public float TargettingSystemRange { get; set; }

    int activeIndex = -1;

    // Start is called before the first frame update
    void Start()
    {

        activeTargets = new List<Target>();
        hudImages = new Dictionary<Target, Image>();
    }

    // Update is called once per frame
    void Update()
    {

        if (activeTargets.Count == 0)
        {
            activeIndex = -1;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            activeIndex++;
            if (activeIndex > activeTargets.Count - 1)
            {
                activeIndex = 0;
            }
            selectedTarget = activeTargets[activeIndex];
        }

        foreach (var target in activeTargets)
        {

            Image targetImage;

            if (!hudImages.TryGetValue(target, out targetImage))
            {
                targetImage = Instantiate(targetPrefab, Camera.main.WorldToScreenPoint(target.transform.position), Quaternion.identity, transform);
                hudImages.Add(target, targetImage);
            }

            targetImage.color = target == selectedTarget ? activeTargetColor : inactiveTargetColor;
            targetImage.transform.position = Camera.main.WorldToScreenPoint(target.transform.position);


            Vector3 vectorToTarget = target.transform.position - playerGO.transform.position;
            
            float actualScale = Mathf.Lerp(targetHudSizeRange.x, targetHudSizeRange.y, 1 - (vectorToTarget.magnitude / TargettingSystemRange));
            targetImage.transform.localScale = new Vector3(actualScale, actualScale, 1f);

            float dir = Vector3.Dot(playerGO.transform.forward.normalized, vectorToTarget.normalized);

            if (dir > 0)
            {
                hudImages[target].gameObject.SetActive(true);
            }
            else
            {
                hudImages[target].gameObject.SetActive(false);
            }
        }


    }

    public void AddTarget(Target target)
    {
        activeTargets.Add(target);
    }

    public void RemoveTarget(Target target)
    {

        Image imageTarget = hudImages[target];
        hudImages.Remove(target);
        Destroy(imageTarget.gameObject);
        activeTargets.Remove(target);
        if (selectedTarget == target)
        {
            activeIndex = -1;
            selectedTarget = null;
        }
    }
}
