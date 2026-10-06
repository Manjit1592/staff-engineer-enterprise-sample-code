FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore src/Services/InventoryService/Enterprise.InventoryService.API/Enterprise.InventoryService.API.csproj

RUN dotnet publish src/Services/InventoryService/Enterprise.InventoryService.API/Enterprise.InventoryService.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Enterprise.InventoryService.API.dll"]