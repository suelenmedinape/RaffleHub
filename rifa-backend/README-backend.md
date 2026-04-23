# ⚙️ RaffleHub Backend - O Coração Lógico (ASP.NET Core 9)

![Backend Header](file:///home/suelen/Documents/Baja/rifas/rifas/rifa-backend/docs/assets/backend_header.png)

A API REST centralizada do ecossistema RaffleHub. Esta camada não é apenas um CRUD; é um sistema resiliente construído sob os princípios de **Clean Architecture**, com foco total em integridade de dados e processamento assíncrono.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)]()
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)]()
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-336791?style=for-the-badge&logo=postgresql)]()

---

## 🏛️ Arquitetura e Padrões de Projeto

O backend segue uma versão simplificada e eficiente da Clean Architecture, garantindo que a regra de negócio seja independente de frameworks externos.

### 💎 Result Pattern (FluentResults)
Diferente de sistemas legados que usam `Exceptions` para controle de fluxo, utilizamos o **Result Pattern**.
- **O que é:** Toda operação de serviço retorna um objeto `Result<T>`.
- **Vantagem:** Evita o custo computacional de gerar *Stack Traces* para erros comuns (ex: "Bilhete esgotado") e torna o código extremamente legível e fácil de testar.

---

## 🚀 Infraestrutura e Resiliência

### 🐳 Docker & Containerização
O backend está pronto para ser orquestrado via Docker. O arquivo `compose.yaml` na raiz do backend permite subir o serviço com todas as configurações de ambiente injetadas de forma segura.

### 🔄 Background Jobs com Hangfire
Para garantir que a API responda instantaneamente, movemos processos lentos para o **Hangfire**:
- **Expiração de Bilhetes:** Uma tarefa recorrente limpa reservas não pagas a cada minuto.
- **Relatórios:** Processamento de dados pesados sem travar a thread da requisição.

### 🚦 Rate Limiting & Segurança
Implementamos políticas agressivas de **Rate Limiting** para proteger endpoints sensíveis contra ataques de força bruta ou bots, garantindo a disponibilidade para usuários reais.

---

## 🛰️ Real-time e Integrações

- **SignalR Hub:** Gerencia o `PaymentNotificationHub`, notificando o frontend em milissegundos assim que um pagamento PIX é confirmado.
- **Mercado Pago:** Integração nativa para geração de QR Codes e recebimento de webhooks.
- **WAHA Connectivity:** Disparo de comprovantes via WhatsApp de forma direta, garantindo que o cliente receba seu bilhete instantaneamente.

---

## 📁 Estrutura do Projeto (RaffleHub.Api)

```
├── Controllers/   # Endpoints REST purificados.
├── Services/      # Orquestração da lógica de negócio.
├── Repositories/  # Abstração de acesso a dados (EF Core).
├── Entities/      # Modelos de domínio.
├── Hubs/          # Hubs SignalR para comunicação bidirecional.
└── Utils/         # Exception Handlers, Mappings e Jobs.
```

---

## 🔨 Setup Inicial (Modo Dev)

1. **Configuração:** Ajuste o `appsettings.Development.json` com suas chaves de banco e Mercado Pago.
2. **Migrations:**
   ```bash
   dotnet ef database update
   ```
3. **Run:**
   ```bash
   dotnet run --project RaffleHub.Api
   ```

---

## 🐳 Execução via Docker (Recomendado)

Para rodar o backend de forma isolada e rápida utilizando Docker, siga os passos abaixo:

1. **Navegue até a raiz do projeto backend:**
   ```bash
   cd rifa-backend/RaffleHub
   ```
2. **Suba o container:**
   ```bash
   docker compose up -d --build
   ```
3. **Verifique o status:**
   O container `rafflehub.api` estará rodando e a API ficará acessível em `http://localhost:5000`.
4. **Logs em tempo real:**
   ```bash
   docker compose logs -f rafflehub.api
   ```

> [!NOTE]
> O Docker utiliza a porta interna `8080` mapeada para a `5000` na sua máquina. Certifique-se de que a porta `5000` esteja livre.

---
> [!IMPORTANT]
> O sistema utiliza **Serilog** para logs estruturados, permitindo rastrear o ciclo de vida completo de cada transação financeira no console ou via Seq.
