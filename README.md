# dotnet-7-webapi-with-jwt-auth

This project is a template for a .net 7 Web API with JWT Bearer authorization.

## App Settings

The appsettings.json file contains the following settings:
* LoginServer: The URL of the login server
* JWKSEndpoint: The URL of the JWKS endpoint
* ValidIssuer: The issuer of the JWT
* ValidAudience: The audience of the JWT

If you want to skip any of the validations, just leave the value empty.

```
    "AppSettings": {
        "LoginServer": "Replace with your login server URL",
        "JWKSEndpoint": "Replace with your JWKS endpoint",
        "ValidIssuer": "Replace with your issuer",
        "ValidAudience": "Replace with your audience, leave empty if you don't want to validate audience"
    },
```

## How to use

1. Clone this repository
2. Open the solution in your preferred IDE
3. Replace the appsettings.json values with your own values
4. Run the project with the following command:

    ```dotnet run --launch-profile https``` 
5. Open the Swagger UI in your browser: https://localhost:`project port`/swagger/index.html
6. Add your JWT token in the "Authorize" button in the top right corner. (Don't forget to add the "Bearer" prefix)
7. Click on the `GET /api/AuthTest` endpoint and click on the "Try it out" button. If you have a valid JWT token, you should see the following response:

    ```json
    {
    "is_authenticated": true,
    "user_id": "user id from the JWT token"
    }
    ```
