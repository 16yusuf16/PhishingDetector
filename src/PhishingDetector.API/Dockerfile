FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/PhishingDetector.Core/ ./PhishingDetector.Core/
COPY src/PhishingDetector.API/ ./PhishingDetector.API/
RUN dotnet restore PhishingDetector.API/PhishingDetector.API.csproj
RUN dotnet publish PhishingDetector.API/PhishingDetector.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "PhishingDetector.API.dll"]
