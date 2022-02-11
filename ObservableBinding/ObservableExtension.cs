namespace ObservableBinding;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

public class ObservableExtension : MarkupExtension
{
    readonly BindingBase _binding;

    public ObservableExtension(BindingBase binding)
    {
        _binding = binding;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var trampoline = new Trampoline();
        trampoline.SetBinding(Trampoline.ObservableProperty, _binding);
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget)
        {
            var targetObject = provideValueTarget.TargetObject;
            if (targetObject is FrameworkElement frameworkElement)
            {
                trampoline.SetBinding(
                    FrameworkElement.DataContextProperty,
                    new Binding
                    {
                        Source = frameworkElement,
                        Path = new PropertyPath(nameof(frameworkElement.DataContext)),
                        Mode = BindingMode.OneWay
                    }
                );
            }
            else if (targetObject is FrameworkContentElement frameworkContentElement)
            {
                trampoline.SetBinding(
                    FrameworkElement.DataContextProperty,
                    new Binding
                    {
                        Source = frameworkContentElement,
                        Path = new PropertyPath(nameof(frameworkContentElement.DataContext)),
                        Mode = BindingMode.OneWay
                    }
                );
            }
        }
        var binding = new Binding
        {
            Source = trampoline,
            Path = new PropertyPath(nameof(trampoline.Value)),
            Mode = BindingMode.OneWay
        };
        return binding.ProvideValue(serviceProvider);
    }

    class Trampoline : FrameworkElement
    {
        public static readonly DependencyProperty ObservableProperty = DependencyProperty.Register(
            nameof(Observable),
            typeof(object),
            typeof(Trampoline),
            new PropertyMetadata(HandleObservableChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(Trampoline));

        IDisposable? _subscription;

        public object? Observable
        {
            get => GetValue(ObservableProperty);
            set => SetValue(ObservableProperty, value);
        }

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        static void HandleObservableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (Trampoline)d;
            me.Value = default;
            if (e.NewValue is { } newValue)
            {
                if (newValue
                        .GetType()
                        .GetInterfaces()
                        .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IObservable<>)) is
                    { } observableType)
                {
                    var eventType = observableType.GenericTypeArguments[0];
                    var subscribeMethod = typeof(Trampoline).GetMethod(nameof(Subscribe), BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(eventType);
                    subscribeMethod.Invoke(me, new [] { newValue });
                    return;
                }
            }
            Interlocked.Exchange(ref me._subscription, null)?.Dispose();
        }

        void Subscribe<T>(IObservable<T> observable) => Interlocked.Exchange(
            ref _subscription,
            observable.ObserveOn(SynchronizationContext.Current!).Subscribe(x => Value = x)
        )?.Dispose();
    }
}