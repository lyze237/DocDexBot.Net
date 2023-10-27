FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DocDexBot.Net.csproj", "./"]
RUN dotnet restore "DocDexBot.Net.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DocDexBot.Net.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocDexBot.Net.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocDexBot.Net.dll"]
