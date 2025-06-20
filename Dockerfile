﻿
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN     apt-get  update -y \
&& apt-get upgrade -y \
&& apt-get install iputils-ping -y \
&& apt-get install net-tools -y 
USER $APP_UID
WORKDIR /app
EXPOSE 5035

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["api.csproj", "./"]
RUN dotnet restore "api.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.dll"]
