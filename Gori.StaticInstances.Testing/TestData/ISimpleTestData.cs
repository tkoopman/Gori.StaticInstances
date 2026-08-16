namespace Gori.StaticInstances.Testing.TestData;

public interface ISimpleTestData
{
    string Input { get; set; }

    void Assert(string? actual);
}
