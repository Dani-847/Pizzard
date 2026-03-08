// Assets/Scripts/Interfaces/ITokenSource.cs
public interface ITokenSource
{
    int GetTokens();
    bool SpendTokens(int amount);
}
