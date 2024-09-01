using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;
using System.ComponentModel;

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

//Adding IDtailedStoreListener derived from IStoreListener for callbacks
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    //creating instance of IStoreController
    IStoreController m_StoreContoller;

//creating instance of consumable product item
    public ConsumableItem cItem;

//creating instance of nonconsumable product item
    public NonConsumableItem ncItem;

    //creating instance of subscription product item
    public SubscriptionItem sItem;

    public TMP_InputField input;


    //public Data data;
    //public Payload payload;
    //public PayloadData payloadData;
    private void Start()
    {
        int coins = PlayerPrefs.GetInt("totalCoins");
        //PlayerPrefs.SetInt("totalCoins", 0);
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
        CheckNonConsumable(ncItem.Id);
        CheckSubscription(sItem.Id);
    }
    #endregion


    #region button clicks 
    public void Consumable_Btn_Pressed()
    {
        //AddCoins(50);
        m_StoreContoller.InitiatePurchase(cItem.Id);
    }

    public void NonConsumable_Btn_Pressed()
    {
        //RemoveAds();
        m_StoreContoller.InitiatePurchase(ncItem.Id);

    }

    public void Subscription_Btn_Pressed()
    {
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
            //string receipt = product.receipt;
            //data = JsonUtility.FromJson<Data>(receipt);
            //payload = JsonUtility.FromJson<Payload>(data.Payload);
            //payloadData = JsonUtility.FromJson<PayloadData>(payload.json);

            //int quantity = payloadData.quantity;

            //for (int i = 0; i < quantity; i++)
           // {
            //    AddCoins(50);
            //}

            AddCoins(50);
            
        }
        else if (product.definition.id == ncItem.Id)//non consumable
        {
            RemoveAds();
        }
        else if (product.definition.id == sItem.Id)//subscribed
        {
            ActivateElitePass();
        }

        return PurchaseProcessingResult.Complete;
    }
    #endregion




    void CheckNonConsumable(string id) {
        if (m_StoreContoller!=null)
        {
            var product = m_StoreContoller.products.WithID(id);
            if (product!=null)
            {
                if (product.hasReceipt)//purchased
                {
                    RemoveAds();
                }
                else {
                    ShowAds();
                }
            }
        }
    }

    void CheckSubscription(string id) {

        var subProduct = m_StoreContoller.products.WithID(id);
        if (subProduct != null)
        {
            try
            {
                if (subProduct.hasReceipt)
                {
                    var subManager = new SubscriptionManager(subProduct, null);
                    var info = subManager.getSubscriptionInfo();
                    /*print(info.getCancelDate());
                    print(info.getExpireDate());
                    print(info.getFreeTrialPeriod());
                    print(info.getIntroductoryPrice());
                    print(info.getProductId());
                    print(info.getPurchaseDate());
                    print(info.getRemainingTime());
                    print(info.getSkuDetails());
                    print(info.getSubscriptionPeriod());
                    print(info.isAutoRenewing());
                    print(info.isCancelled());
                    print(info.isExpired());
                    print(info.isFreeTrial());
                    print(info.isSubscribed());*/


                    if (info.isSubscribed() == Result.True)
                    {
                        print("We are subscribed");
                        ActivateElitePass();
                    }
                    else {
                        print("Un subscribed");
                        DeActivateElitePass();
                    }

                }
                else{
                    print("receipt not found !!");
                }
            }
            catch (Exception)
            {

                print("It only work for Google store, app store, amazon store, you are using fake store!!");
            }
        }
        else {
            print("product not found !!");
        }
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


[Serializable]
public class SkuDetails
{
    public string productId;
    public string type;
    public string title;
    public string name;
    public string iconUrl;
    public string description;
    public string price;
    public long price_amount_micros;
    public string price_currency_code;
    public string skuDetailsToken;
}

[Serializable]
public class PayloadData
{
    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
    public int quantity;
    public bool acknowledged;
}

[Serializable]
public class Payload
{
    public string json;
    public string signature;
    public List<SkuDetails> skuDetails;
    public PayloadData payloadData;
}

[Serializable]
public class Data
{
    public string Payload;
    public string Store;
    public string TransactionID;
}


