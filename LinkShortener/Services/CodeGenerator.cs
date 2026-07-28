namespace LinkShortener.Services;

public class CodeGenerator: ICodeGenerator
{
    private const int MIN_NUMBER_SYMBOLS = 6;
    private const int MAX_NUMBER_SYMBOLS = 7;
    
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    public string Generate()
    {   
        char[] symbols = new char[Random.Shared.Next(MIN_NUMBER_SYMBOLS, MAX_NUMBER_SYMBOLS+1)];
        
        for (int i = 0; i < symbols.Length; i++)
        {
            symbols[i] = Alphabet[Random.Shared.Next(0, Alphabet.Length)];
        }
        
        return new string(symbols);
    }
}