namespace HelloWorld.Client.Kern;

/// <summary>Iets dat meldt dat zijn state veranderd is: een store. (Echt: <c>Kern/IChangeNotifier.cs</c>.)</summary>
public interface IChangeNotifier
{
    event Action? OnChange;
}
