using HelloWorld.Shared;

namespace HelloWorld.Api.Procedures;

/// <summary>Een overtreden business rule. De Api vertaalt dit naar een 400 met de melding.</summary>
public sealed class RuleViolation(string message) : Exception(message);

/// <summary>
/// Vervangt de database uit het echte project: een lijst in het geheugen, met twee
/// "stored procedures" erboven. De business rules wonen hier, niet in de Api en niet in de
/// client. (Echt: <c>Api/Sql/procedures/01-applications.sql</c>.) Weg na een herstart.
/// </summary>
public sealed class GreetingProcedures
{
    private readonly List<Greeting> _table =
    [
        new() { Id = Guid.NewGuid().ToString(), Text = "Hallo wereld", Language = "nl" },
        new() { Id = Guid.NewGuid().ToString(), Text = "Hello world", Language = "en" },
    ];
    private readonly object _lock = new();

    public IReadOnlyList<Greeting> sp_greetings_list()
    {
        lock (_lock) return _table.ToList();
    }

    public Greeting sp_greetings_create(Greeting input)
    {
        // De guard: zonder tekst geen begroeting. Dit is de regel die je live kunt laten falen.
        if (string.IsNullOrWhiteSpace(input.Text))
            throw new RuleViolation("Een begroeting heeft tekst nodig.");

        // Zoals ISNULL(@id, NEWID()) in de echte procedure.
        if (string.IsNullOrEmpty(input.Id)) input.Id = Guid.NewGuid().ToString();

        lock (_lock)
        {
            _table.Add(input);
            return input;
        }
    }
}
