namespace JLib.TypeSystem.Abstractions;

public interface ISourceCodeWriter : IDisposable
{
    ISourceCodeWriter Write(string text);
    ISourceCodeWriter Write(char character);
    ISourceCodeWriter EndLine();
    /// <summary>
    /// adds ';\n'
    /// </summary>
    ISourceCodeWriter EndStatement();
    ISourceCodeWriter WriteBlock(Action<ISourceCodeWriter> block);
}
[Input] -> [Transformer] -> [TypeSystem]-> [SourceCodeGenerator] -> [Output]