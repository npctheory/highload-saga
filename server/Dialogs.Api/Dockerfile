FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet tool install --global dotnet-ef

ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR ./Dialogs.Api
RUN dotnet restore
RUN dotnet publish Dialogs.Api.csproj -c Release -o /app/publish

RUN dotnet ef database update --project ./Dialogs.Infrastructure/Dialogs.Infrastructure.csproj --startup-project ./Dialogs.Api/Dialogs.Api.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Ansible
# RUN apt-get update && \
#     apt-get install -y python3 python3-venv python3-pip && \
#     python3 -m venv /opt/venv && \
#     /opt/venv/bin/python -m pip install --upgrade pip && \
#     /opt/venv/bin/python -m pip install psycopg2-binary requests==2.31.0
# ENV VIRTUAL_ENV=/opt/venv
# ENV PATH="$VIRTUAL_ENV/bin:$PATH"

ARG UID=10001
RUN adduser --disabled-password --gecos "" --home "/nonexistent" --shell "/sbin/nologin" --no-create-home --uid "${UID}" appuser
USER appuser
EXPOSE 80
ENTRYPOINT ["dotnet", "Dialogs.Api.dll"]
