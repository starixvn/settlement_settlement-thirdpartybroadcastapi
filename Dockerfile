FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder

COPY ./Settlement.ThirdPartyBroadcastApi/Settlement.ThirdPartyBroadcastApi.csproj ./Settlement.ThirdPartyBroadcastApi/Settlement.ThirdPartyBroadcastApi.csproj
COPY ./ThirdPartyBroadcast ./ThirdPartyBroadcast

WORKDIR ./Settlement.ThirdPartyBroadcastApi
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore  -r linux-x64 -v n

COPY ./Settlement.ThirdPartyBroadcastApi .

RUN --mount=type=cache,target=/root/.nuget/packages \ 
	dotnet publish -r linux-x64 --self-contained false -c Release -o /app --no-restore

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENV COMPlus_EnableDiagnostics=0

# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser

ENTRYPOINT ["dotnet", "Settlement.ThirdPartyBroadcastApi.dll"]
