namespace Tests.Shared.Auth;

// https://josef.codes/using-records-when-implementing-the-builder-pattern-in-c-sharp/
public class LoginRequestBuilder
{
    private string _userNameOrEmail = "mehdi";
    private string _password = "123456";

    public LoginRequestBuilder WithUserNameOrEmail(string userNameOrEmail)
    {
        _userNameOrEmail = userNameOrEmail;
        return this;
    }

    public LoginRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public LoginUserRequestMock Build()
    {
        return new LoginUserRequestMock(_userNameOrEmail, _password);
    }
}
