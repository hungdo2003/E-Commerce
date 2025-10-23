# ShoppeClone.Api (ASP.NET Core 8 Web API)

## Run
```bash
dotnet restore
dotnet ef migrations add Init
dotnet ef database update
dotnet run
```
Swagger sẽ ở `/swagger`.
Cập nhật `appsettings.json` (ConnectionStrings, Jwt, ZaloPay, Chat) trước khi chạy.
