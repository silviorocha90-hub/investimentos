FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Investimentos.Api/Investimentos.Api.csproj Investimentos.Api/
COPY Investimentos.Application/Investimentos.Application.csproj Investimentos.Application/
COPY src/Investimentos.Domain/Investimentos.Domain.csproj src/Investimentos.Domain/
COPY src/Investimentos.Infrastructure/Investimentos.Infrastructure.csproj src/Investimentos.Infrastructure/

RUN dotnet restore Investimentos.Api/Investimentos.Api.csproj

COPY . .
RUN dotnet publish Investimentos.Api/Investimentos.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Investimentos.Api.dll"]
