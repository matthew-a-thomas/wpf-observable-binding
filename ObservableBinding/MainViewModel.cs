namespace ObservableBinding;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    int _version;

    public MainViewModel()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(4.5));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));
            }
        });
    }

    public IObservable<string> Messages => Observable.Create<string>(observer =>
    {
        var version = Interlocked.Increment(ref _version);
        Trace.WriteLine($"SUBSCRIPTION {version} STARTED");
        return new CompositeDisposable(
            Disposable.Create(() =>
            {
                Trace.WriteLine($"SUBSCRIPTION {version} STOPPED");
            }),
            Observable.Interval(TimeSpan.FromSeconds(1)).Select(_ => $"#{version} {Guid.NewGuid()}").Subscribe(observer)
        );
    });
}