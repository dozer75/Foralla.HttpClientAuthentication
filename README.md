# KISS.HttpClientAuthentication NuGet package

## ABOUT

This NuGet package enables configuring authentication for HttpClients using the ASP.NET Core Configuration 
system.

The package currently supports the following authentication methods:
- Api key
- Basic
- OAuth2
  - Client credentials

## USAGE

Add the NuGet package `KISS.HttpClientAuthentication` to your project and whenever a 
`AddHttpClient` injection is done that needs authentication chain a call to 
`AddAuthenticatedHttpMessageHandler` for the `AddHttpClient` request and set the configuration accordingly.

There are two different `AddAuthenticatedHttpMessageHandler` extension methods. One without any parameter
and one with a `string configSection` parameter. The first one will use the `IHttpClientBuilder.Name`
as the name of the configuration to read from the ASP.NET Core Configuration system, while the latter gives
you the opportunity to specify a specific configuration setting.

### Configuration

Various authentication methods are supported by the following configuration settings.

#### None

No authentication.

```
"<Section name>": {
  "AuthenticationProvider": "None"
}
```

#### Api Key

Authentication using API key, both `Header` and `Value` must be set.

```
"<section name>": {
  "AuthenticationProvider": "ApiKey",
  "ApiKey": {
    "Header": "<API KEY HEADER>",
    "Value": "<API KEY VALUE>"
  }
}
```

#### Basic

Authentication using username/password authentication, both `Username` and `Password` must be set.

```
"<section name>": {
  "AuthenticationProvider": "Basic",
  "Basic": {
    "Username": "<username>",
    "Password": "<password>"
  }
}
```

#### OAuth 2

Authentication using OAuth2.

##### Client credentials

Using OAuth2 client credentials, all settings except `DisableTokenCache` and `Scope` is required.

```
"<section name>": {
  "AuthenticationProvider": "OAuth2",
  "OAuth2": {
    "DisableTokenCache": false,
    "GrantType": "ClientCredentials",
    "Scope": "<Optional scopes separated by space>",
    "TokenEndpoint": "<OAuth2 token endpoint>",
    "ClientCredentials": {
        "ClientId": "<Unique client id>",
        "ClientSecret": "<Secret connected to the client id>"
    }
  }
}
```

> **NOTE**: The previous `AuthorizationEndpoint` is replaced by `TokenEndpoint`. It still exists,
but is obsoleted and will be removed in a later version.

### Examples

#### Example 1 - Uses the name of the type that is used in `AddHttpClient`

MyClass.cs
```
public class MyClass
{
}
```

```
// When configuring Dependency Injection
services.AddHttpClient<MyClass>().AddAuthenticatedHttpMessageHandler();
```

appsettings.json
```
{
    "MyClass": {
        // This configuration is used since the class name is MyClass
    }
}
```

#### Example 2 - Uses the configSection specified when calling `AddAuthenticatedHttpMessageHandler`

MyClass.cs
```
public class MyClass
{
}
```
```
// When configuring Dependency Injection
services.AddHttpClient<MyClass>().AddAuthenticatedHttpMessageHandler("MyConfiguration");
```

appsettings.json
```
{
    "MyConfiguration": {
        // This configuration is used since the HttpClient Name is MyConfiguration
    }
}
```

#### Example 3 - Uses the name specified when calling `AddHttpClient`

```
// When configuring Dependency Injection
services.AddHttpClient("MyClient").AddAuthenticatedHttpMessageHandler();
```

appsettings.json
```
{
    "MyClient": {
        // This configuration is used since the configSection is set to MyConfiguration
    }
}
```
