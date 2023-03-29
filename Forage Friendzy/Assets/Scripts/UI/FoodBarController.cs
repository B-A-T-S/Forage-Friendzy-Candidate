using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBarController : MonoBehaviour
{
    //public GameObject bankedFoodBar;
    //public GameObject heldFoodBar;

    //private Vector3 originalBankedScale;
    //private Vector3 originalHeldScale;

    int currentBankedFood, currentHeldFood, foodMax;

    float newCollectedX, newHeldX, maximumHeldBarScale;

    float sunValue;
    [SerializeField]
    private UnityEngine.UI.Slider sunSlider, heldFoodSlider, bankedFoodSlider;

    void Start()
    {
        /*
        originalBankedScale = bankedFoodBar.transform.localScale;
        originalHeldScale = heldFoodBar.transform.localScale;

        bankedFoodBar.transform.localScale = new Vector3(0, 1, 1);
        heldFoodBar.transform.localScale = new Vector3(0, 1, 1);
        */
        if(heldFoodSlider != null)
            heldFoodSlider.maxValue=GameManager.Instance.foodLimit;
        if (bankedFoodSlider != null)
            bankedFoodSlider.maxValue = GameManager.Instance.foodLimit;

        GameManager.Instance.sliderValue.OnValueChanged += GMSliderValue_OVC;

    }

    private void GMSliderValue_OVC(float previousValue, float newValue)
    {
        sunSlider.value = newValue;
    }

    // Update is called once per frame
    void Update()
    {

        currentBankedFood = GameManager.Instance.matchFoodCollected.Value;
        currentHeldFood = GameManager.Instance.matchFoodHeld;
        /*
        foodMax = GameManager.Instance.foodLimit;

        newCollectedX = currentBankedFood / (float)foodMax * barMaxScale;
        bankedFoodBar.transform.localScale = new Vector3(newCollectedX, originalBankedScale.y, originalBankedScale.z);

        maximumHeldBarScale = (foodMax - currentBankedFood) / (float) foodMax * barMaxScale; 

        newHeldX = currentHeldFood / (float)foodMax * barMaxScale;
        newHeldX = Mathf.Clamp(newHeldX, 0f, maximumHeldBarScale);
        heldFoodBar.transform.localScale = new Vector3(newHeldX, originalHeldScale.y, originalHeldScale.z);
        */

        //sunSlider.value = GameManager.Instance.remainingDaytime / GameManager.Instance.totalDaytime;

        if (bankedFoodSlider != null)
        {
            bankedFoodSlider.value = currentBankedFood;
            if (bankedFoodSlider.value == 0)
                bankedFoodSlider.gameObject.SetActive(false);
            else
                bankedFoodSlider.gameObject.SetActive(true);
        }
            

        if(heldFoodSlider != null)
        {
            heldFoodSlider.value = bankedFoodSlider.value + currentHeldFood;
            if (heldFoodSlider.value == 0)
                heldFoodSlider.gameObject.SetActive(false);
            else
                heldFoodSlider.gameObject.SetActive(true);
        }
            
        
    }
}
