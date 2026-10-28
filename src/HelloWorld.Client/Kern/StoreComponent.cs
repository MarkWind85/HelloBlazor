using Microsoft.AspNetCore.Components;

namespace HelloWorld.Client.Kern;

/// <summary>
/// Basis voor pagina's die hertekenen als een store verandert: abonneert StateHasChanged
/// op de notifiers van de subklasse en meldt af bij Dispose. De pagina hoeft alleen te
/// zeggen naar welke stores hij luistert. (Echt: <c>Kern/StoreComponent.cs</c>, identiek idee.)
/// </summary>
public abstract class StoreComponent : ComponentBase, IDisposable
{
    private IChangeNotifier[] _subscribed = [];

    protected abstract IEnumerable<IChangeNotifier> Notifiers { get; }

    protected override void OnInitialized()
    {
        _subscribed = Notifiers.ToArray();
        foreach (var notifier in _subscribed) notifier.OnChange += OnStoreChanged;
    }

    // StateHasChanged hoort op de render-thread; InvokeAsync borgt dat ook na een await.
    private void OnStoreChanged() => InvokeAsync(StateHasChanged);

    public virtual void Dispose()
    {
        foreach (var notifier in _subscribed) notifier.OnChange -= OnStoreChanged;
    }
}
