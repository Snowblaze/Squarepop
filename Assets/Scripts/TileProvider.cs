using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TileProvider : IObservable<TileScript>
{
    private List<IObserver<TileScript>> observers;

    public TileProvider()
    {
        observers = new List<IObserver<TileScript>>();
    }

    public IDisposable Subscribe(IObserver<TileScript> observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
        return new Unsubscriber(observers, observer);
    }

    private class Unsubscriber : IDisposable
    {
        private List<IObserver<TileScript>> _observers;
        private IObserver<TileScript> _observer;

        public Unsubscriber(List<IObserver<TileScript>> observers, IObserver<TileScript> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }

    public void ObserverUpdate(TileScript data)
    {
        foreach (var observer in observers.ToList())
        {
            if (data == null)
                observer.OnError(new Exception());
            else
                observer.OnNext(data);
        }
    }

    public void EndTransmission()
    {
        foreach (var observer in observers.ToArray())
            if (observers.Contains(observer))
                observer.OnCompleted();

        observers.Clear();
    }
}
