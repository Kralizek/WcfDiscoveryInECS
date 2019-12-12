# escape=`

FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2019 AS base
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019 AS build
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
WORKDIR /src
COPY ["WcfService/WcfService.csproj", "WcfService/"]
RUN dotnet restore "WcfService/WcfService.csproj"
COPY . .
WORKDIR "/src/WcfService"
RUN dotnet build "WcfService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WcfService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV Environment=Production
ENTRYPOINT ["EMG.Service.WcfService.exe"]