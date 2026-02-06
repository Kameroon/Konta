# src/Konta.Finance/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Konta.Finance/Konta.Finance.csproj", "src/Konta.Finance/"]
COPY ["src/Konta.Shared/Konta.Shared.csproj", "src/Konta.Shared/"]
RUN dotnet restore "src/Konta.Finance/Konta.Finance.csproj"
COPY . .
WORKDIR "/src/src/Konta.Finance"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Konta.Finance.dll"]
