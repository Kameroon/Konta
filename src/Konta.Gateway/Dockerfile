# src/Konta.Gateway/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Konta.Gateway/Konta.Gateway.csproj", "src/Konta.Gateway/"]
# Gateway usually doesn't depend on Shared, but adding just in case or if it does later. 
# If it has no reference to Shared, this line might fail if strict, but dotnet restore handles it.
# Actually, Ocelot gateways often don't need Shared unless custom logic.
# Let's check generally gateways are simple. I'll include Shared copy if it referenced, 
# but for safety I will copy it. The restore will just ignore it if unused.
COPY ["src/Konta.Shared/Konta.Shared.csproj", "src/Konta.Shared/"] 
RUN dotnet restore "src/Konta.Gateway/Konta.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Konta.Gateway"
RUN dotnet publish -c Release -o /app/publish
RUN ls -R /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Konta.Gateway.dll"]
