FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY backend.sln ./
COPY src/CashewNuts.API/CashewNuts.API.csproj             src/CashewNuts.API/
COPY src/CashewNuts.Application/CashewNuts.Application.csproj   src/CashewNuts.Application/
COPY src/CashewNuts.Domain/CashewNuts.Domain.csproj         src/CashewNuts.Domain/
COPY src/CashewNuts.Infrastructure/CashewNuts.Infrastructure.csproj src/CashewNuts.Infrastructure/

RUN dotnet restore

COPY src/ src/

RUN dotnet publish src/CashewNuts.API/CashewNuts.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CashewNuts.API.dll"]