using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;
using System.ComponentModel;
using UnityEngine.UI;

//Adding details for consumable product (id, name, price, etc)
[Serializable]
public class ConsumableItem {
    public string Name;
    public string Id;
    public string desc;
    public float price;
}

//Adding details for nonconsumable product (id, name, price, etc)
[Serializable]
public class NonConsumableItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
}

//Adding details for Subscription product (id, name, price,and time period)
[Serializable]
public class SubscriptionItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
    public int timeDuration;// in Days
}

[RequireComponent(typeof(AudioSource))]

//Adding IDtailedStoreListener derived from IStoreListener for callbacks
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{

    public AudioClip coinColletSFX;
    public AudioSource _source;

    // Add a reference to the Button component
    public Button consumableButton;
    public Button nonConsumableButton;
    public Button subscriptionButton;

    //creating instance of IStoreController
    IStoreController m_StoreContoller;

//creating instance of consumable product item
    public ConsumableItem cItem;

//creating instance of nonconsumable product item
    public NonConsumableItem ncItem;

//creating instance of subscription product item
    public SubscriptionItem sItem;
    private void Start()
    {
        int coins = PlayerPrefs.GetInt("totalCoins");
        //PlayerPrefs.SetInt("totalCoins", 0);  //To reset coins counter
        coinTxt.text = coins.ToString();
        SetupBuilder();
    }

    #region setup and initialize

    //Adding product data inside the builder
    void SetupBuilder()
    {

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(cItem.Id, ProductType.Consumable);
        builder.AddProduct(ncItem.Id, ProductType.NonConsumable);
        builder.AddProduct(sItem.Id, ProductType.Subscription);

        //Initializing unity purchasing
        UnityPurchasing.Initialize(this, builder);
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        print("Initialze Success");
        m_StoreContoller = controller;
    }
    #endregion


    #region button clicks 
    public void Consumable_Btn_Pressed()
    {
        consumableButton.interactable = false; // Disable the button

        //AddCoins(50);
        m_StoreContoller.InitiatePurchase(cItem.Id);
    }

    public void NonConsumable_Btn_Pressed()
    {
        nonConsumableButton.interactable = false; // Disable the button

        //RemoveAds();
        m_StoreContoller.InitiatePurchase(ncItem.Id);

    }

    public void Subscription_Btn_Pressed()
    {
        subscriptionButton.interactable = false; // Disable the button

        //ActivateElitePass();
        m_StoreContoller.InitiatePurchase(sItem.Id);
    }
    #endregion


    #region main
    //processing purchase
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        //Retrive the purchased product
        var product = purchaseEvent.purchasedProduct;

        print("Purchase Complete" + product.definition.id);

        if (product.definition.id == cItem.Id)//consumable item is pressed
        {
            AddCoins(50);
            consumableButton.interactable = true; // Re-enable the button        
        }
        else if (product.definition.id == ncItem.Id)//non consumable
        {
            RemoveAds();
            nonConsumableButton.interactable = true; // Re-enable the button
        }
        else if (product.definition.id == sItem.Id)//subscribed
        {
            ActivateElitePass();
            subscriptionButton.interactable = true; // Re-enable the button
        }

        return PurchaseProcessingResult.Complete;
    }
    #endregion

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    } 

    #region error handeling
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        print("failed" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        print("initialize failed" + error + message);
    }



    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        print("purchase failed" + failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        print("purchase failed" + failureDescription);
    }
    #endregion


    #region extra 

    [Header("Consumable")]
    public TextMeshProUGUI coinTxt;
    void AddCoins(int num)
    {
        _source.PlayOneShot(coinColletSFX);
        int coins = PlayerPrefs.GetInt("totalCoins");
        coins += num;
        PlayerPrefs.SetInt("totalCoins", coins);

        StartCoroutine(startCoinShakeEffect(coins - num, coins, .5f));
    }
    float val;
    IEnumerator startCoinShakeEffect(int oldValue, int newValue, float animTime)
    {
        float ct = 0;
        float nt;
        float tot = animTime;
        while (ct < tot)
        {
            ct += Time.deltaTime;
            nt = ct / tot;
            val = Mathf.Lerp(oldValue, newValue, nt);
            coinTxt.text = ((int)(val)).ToString();
            yield return null;
        }
    }

    [Header("Non Consumable")]
    public GameObject AdsPurchasedWindow;
    public GameObject adsBanner;
    void RemoveAds()
    {
        DisplayAds(false);
    }
    void ShowAds()
    {
        DisplayAds(true);

    }
    void DisplayAds(bool x)
    {
        if (!x)
        {
            AdsPurchasedWindow.SetActive(true);
            adsBanner.SetActive(false);
        }
        else
        {
            AdsPurchasedWindow.SetActive(false);
            adsBanner.SetActive(true);
        }
    }

    [Header("Subscription")]
    public GameObject subActivatedWindow;
    public GameObject premiumBanner;

    void ActivateElitePass()
    {
        setupElitePass(true);
    }
    void DeActivateElitePass()
    {
        setupElitePass(false);
    }
    void setupElitePass(bool x)
    {
        if (x)// active
        {
            subActivatedWindow.SetActive(true);
            premiumBanner.SetActive(true);
        }
        else
        {
            subActivatedWindow.SetActive(false);
            premiumBanner.SetActive(false);
        }
    }
    #endregion

}


