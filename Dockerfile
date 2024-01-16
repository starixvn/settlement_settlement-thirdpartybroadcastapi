FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder

COPY ./Settlement.ThirdPartyBroadcastApi/Settlement.ThirdPartyBroadcastApi.csproj ./Settlement.ThirdPartyBroadcastApi/Settlement.ThirdPartyBroadcastApi.csproj
COPY ./ThirdPartyBroadcast ./ThirdPartyBroadcast

WORKDIR ./Settlement.ThirdPartyBroadcastApi
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Settlement.ThirdPartyBroadcastApi .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine AS base

ENV COMPlus_EnableDiagnostics=0
WORKDIR /app
COPY --from=builder /app .


# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

ENTRYPOINT ["dotnet", "Settlement.ThirdPartyBroadcastApi.dll"]