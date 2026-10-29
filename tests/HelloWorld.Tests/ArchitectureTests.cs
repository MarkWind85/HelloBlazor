using System.Reflection;
using System.Runtime.CompilerServices;
using HelloWorld.Client.Kern;

namespace HelloWorld.Tests;

// Een grens van de indeling als test in plaats van als afspraak. Reflectie ziet declaraties
// (velden, properties, constructorparameters), geen methodelichamen: daarom valt Program.cs,
// dat zijn HttpClients in een methodelichaam aanmaakt, hier niet op. EC's ArchitectureTests
// heeft dezelfde grens en leest daarvoor de .razor-bestanden.
public class ArchitectureTests
{
    private static readonly Assembly Client = typeof(ApiClient).Assembly;

    [Fact]
    public void Alleen_Kern_kent_de_HttpClient()
    {
        // In EC: Alleen_Kern_en_Stores_kennen_de_ApiClient, dezelfde grens één laag hoger.
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var offenders = Client.GetTypes()
            // Program en de top-level statements zijn compilerwerk: de compositieroot maakt de HttpClients bewust aan.
            .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) && !t.Name.Contains('<'))
            .Where(t => t.Namespace != "HelloWorld.Client.Kern")
            // Niet-publieke velden tellen mee: daar landt een primary-constructorparameter.
            .Where(t => t.GetFields(All).Any(f => f.FieldType == typeof(HttpClient))
                || t.GetProperties(All).Any(p => p.PropertyType == typeof(HttpClient))
                || t.GetConstructors(All).Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(HttpClient))))
            .Select(t => t.FullName)
            .Order()
            .ToList();

        Assert.True(offenders.Count == 0, $"HttpClient hoort in Kern, niet in: {string.Join(", ", offenders)}");
    }
}
