# Imagem da API Em Dia para o servidor (Render). Build: docker build -t emdia-api ./api
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
# ReadyToRun: o código já sai compilado para Linux x64. Na CPU fraca do plano gratuito, a API sobe e responde às
# primeiras chamadas de cada tela bem mais rápido, sem esperar o JIT (DV23).
RUN dotnet publish src/EmDia.Api/EmDia.Api.csproj -c Release -r linux-x64 --self-contained false \
    -p:PublishReadyToRun=true -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
# Fuso America/Sao_Paulo para "hoje" e para a rotina de avisos das 8h
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080 \
    # 0,1 CPU e 512 MB: coletor de lixo simples, sem threads em segundo plano disputando a CPU
    DOTNET_gcServer=0 \
    DOTNET_gcConcurrent=0
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmDia.Api.dll"]
