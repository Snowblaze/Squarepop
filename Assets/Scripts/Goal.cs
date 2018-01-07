using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Goal : MonoBehaviour, IObserver<TileScript>
{
    [SerializeField]
    private Image background;
    [SerializeField]
    private Text countText;
    private GoalData data;

    public delegate void MethodContainer();
    public event MethodContainer OnFinish;

    public void Init(GoalData goalData)
    {
        background = GetComponentInChildren<Image>();
        countText = GetComponentInChildren<Text>();
        data = goalData;
        background.color = data.TargetColor;
        countText.text = data.Count.ToString();
    }

    public void DecreaseCount(int quantity)
    {
        data.Count -= quantity;
        countText.text = data.Count.ToString();
    }

    #region Observer
    private IDisposable unsubscriber;

    public virtual void Subscribe(IObservable<TileScript> provider)
    {
        if (provider != null)
            unsubscriber = provider.Subscribe(this);
    }

    public void OnCompleted()
    {
        //Debug.Log("Goal: completed working with tile.");
    }

    public void OnError(Exception exception)
    {
        //Debug.Log("Goal: error happened.");
    }

    public void OnNext(TileScript value)
    {
        if(value.GetMaterialColor() == data.TargetColor)
            DecreaseCount(1);
        if (data.Count <= 0)
        {
            OnFinish();
            this.Unsubscribe();
        }
    }

    public virtual void Unsubscribe()
    {
        unsubscriber.Dispose();
    }
    #endregion
}