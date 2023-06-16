FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder

COPY ./Settlement.ThirdPartyBroadcastApi/Settlement.ThirdPartyBroadcastApi.csproj ./Settlement.ThirdPartyBroadcastApi/Settlement.ThirdPartyBroadcastApi.csproj
COPY ./ThirdPartyBroadcast ./ThirdPartyBroadcast

WORKDIR ./Settlement.ThirdPartyBroadcastApi
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Settlement.ThirdPartyBroadcastApi .

RUN dotnet publish -r linux-x64 --self-contained false -c release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .


# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

ENTRYPOINT ["dotnet", "Settlement.ThirdPartyBroadcastApi.dll"]
