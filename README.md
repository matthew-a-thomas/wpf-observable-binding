# wpf-observable-binding

Demonstrate binding to [observables](https://docs.microsoft.com/en-us/dotnet/api/system.iobservable-1?view=net-6.0) in WPF

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public IObservable<string> Messages => // ...
}
```

```xaml
<TextBlock Text="{local:Observable {Binding Messages}}"/>
```

## Why?

Honestly it's probably not a good idea.

As this is written now there is nothing that unsubscribes from the observables when the destination object (the `<TextBlock>` in this case) goes away. So you'll have memory leaks.

Also the normal patterns in MVVM are usually more than adequate.

## How?

```
                              +--------------+
+------------+                | Trampoline   |
| observable | --(binding)--> |  .Observable | --(subscription)--+
+------------+                |              |                   |
+--------+                    | Trampoline   |                   |
| target | <-----(binding)--- |  .Value      | <-----------------+
+--------+                    +--------------+

```

The key to all this is the `Trampoline` class. When its `Observable` property gets an observable then it keeps its `Value` property up to date with a subscription. `Trampoline` is itself a `DependencyObject` so the target can be bound to its `Value`.
