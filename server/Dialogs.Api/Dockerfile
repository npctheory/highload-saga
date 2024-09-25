FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app
COPY . .

ENV PATH="$PATH:/root/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet tool install --global dotnet-ef

RUN dotnet restore ./Dialogs.Api/Dialogs.Api.csproj
RUN dotnet publish ./Dialogs.Api/Dialogs.Api.csproj -c Release -o /app/publish

WORKDIR /app/publish
EXPOSE 80
ENTRYPOINT ["dotnet", "Dialogs.Api.dll"]
