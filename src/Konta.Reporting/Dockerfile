# src/Konta.Reporting/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Konta.Reporting/Konta.Reporting.csproj", "src/Konta.Reporting/"]
COPY ["src/Konta.Shared/Konta.Shared.csproj", "src/Konta.Shared/"]
RUN dotnet restore "src/Konta.Reporting/Konta.Reporting.csproj"
COPY . .
WORKDIR "/src/src/Konta.Reporting"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Reporting uses QuestPDF/Excel, generally needs font config.
RUN apt-get update && apt-get install -y libfontconfig1
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Konta.Reporting.dll"]
