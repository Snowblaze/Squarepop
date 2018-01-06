using System;

public interface IObservable<TType>
{
    IDisposable Subscribe(IObserver<TType> observer);
}
