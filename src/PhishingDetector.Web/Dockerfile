FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/PhishingDetector.Core/ ./PhishingDetector.Core/
COPY src/PhishingDetector.Web/ ./PhishingDetector.Web/
RUN dotnet restore PhishingDetector.Web/PhishingDetector.Web.csproj
RUN dotnet publish PhishingDetector.Web/PhishingDetector.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "PhishingDetector.Web.dll"]
