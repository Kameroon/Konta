# src/Konta.Billing/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Konta.Billing/Konta.Billing.csproj", "src/Konta.Billing/"]
COPY ["src/Konta.Shared/Konta.Shared.csproj", "src/Konta.Shared/"]
RUN dotnet restore "src/Konta.Billing/Konta.Billing.csproj"
COPY . .
WORKDIR "/src/src/Konta.Billing"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Install dependencies for QuestPDF if needed (libskia)
# QuestPDF usually carries its own, but sometimes on Linux Alpine/Debian needs fonts.
# Using standard aspnet image which is Debian based usually works fine for skia.
RUN apt-get update && apt-get install -y libfontconfig1
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Konta.Billing.dll"]
