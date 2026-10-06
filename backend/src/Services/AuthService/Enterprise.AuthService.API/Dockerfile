FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/AuthService/Enterprise.AuthService.API/Enterprise.AuthService.API.csproj
RUN dotnet publish src/Services/AuthService/Enterprise.AuthService.API/Enterprise.AuthService.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Enterprise.AuthService.API.dll"]