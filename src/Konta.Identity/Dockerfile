# src/Konta.Identity/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Konta.Identity/Konta.Core.API.csproj", "src/Konta.Identity/"]
COPY ["src/Konta.Tenant/Konta.Tenant.csproj", "src/Konta.Tenant/"]
COPY ["src/Konta.Billing/Konta.Billing.csproj", "src/Konta.Billing/"]
COPY ["src/Konta.Shared/Konta.Shared.csproj", "src/Konta.Shared/"]
RUN dotnet restore "src/Konta.Identity/Konta.Core.API.csproj"
COPY . .
WORKDIR "/src/src/Konta.Identity"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
RUN apk add --no-cache icu-libs fontconfig ttf-dejavu
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Konta.Core.API.dll"]
