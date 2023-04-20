FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
 
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["webHttpTest.csproj", ""]
RUN dotnet restore "./webHttpTest.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "webHttpTest.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "webHttpTest.csproj" -c Release -o /app

FROM base AS final

RUN apt update && \
    apt install --assume-yes \
    # build/code
    git bash bash-completion vim tmux jq \
    # network
    net-tools tcpdump curl wget nmap tcpflow iftop net-tools mtr netcat-openbsd bridge-utils iperf ngrep \
    # certificates
    ca-certificates openssl \
    # processes/io
    htop atop strace iotop sysstat ltrace ncdu logrotate hdparm pciutils psmisc tree pv 

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "webHttpTest.dll"]