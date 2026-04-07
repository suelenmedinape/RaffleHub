# ⚙️ RaffleHub Backend - O Motor Operacional (ASP.NET Core 9 / C#)

![Backend Header](/img-site/home.png)

A artilharia pesada e o orquestrador silencioso do ecossistema RaffleHub. Esta camada RESTful foi construída sob os princípios de **Clean Architecture**, com foco total em integridade de dados e processamento assíncrono, garantindo que a regra de negócio seja independente de frameworks externos.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)]()
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)]()
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-336791?style=for-the-badge&logo=postgresql)]()

---

## 🏛️ Arquitetura e Padrões de Projeto

O backend segue uma versão eficiente da Clean Architecture, priorizando a testabilidade e o isolamento das complexidades operacionais.

### 💎 Result Pattern (FluentResults)
Diferente de sistemas que utilizam `Exceptions` para controle de fluxo (o que é computacionalmente caro), utilizamos o **Result Pattern**.
- **O que é:** Toda operação de serviço retorna um objeto `Result<T>`.
- **Vantagem:** Evita o custo de gerar *Stack Traces* para erros de domínio (ex: "Bilhete já reservado") e torna o código extremamente legível.
- **Middleware Global:** As `Exceptions` reais (infraestrutura/inesperadas) são capturadas por um Middleware unificado que devolve um `ErrorResponse` padronizado.

### 🧹 Simplicidade e Injeção de Dependência
Eliminamos abstrações redundantes como o "Unit of Work" customizado. O **Entity Framework Core** (`DbContext`) já implementa nativamente o padrão UoW. Utilizamos injeção direta do contexto nos repositórios, garantindo clareza transacional e performance.

---

## 🚀 Processamento Assíncrono e Real-time

### 🔄 Background Jobs com Hangfire
Tarefas pesadas ou agendadas são movidas para o **Hangfire** para não bloquear a thread principal:
- **Expiração de Reservas:** Uma tarefa recorrente limpa reservas pendentes não pagas após o tempo limite.
- **Notificações:** Disparos de mensagens e processamentos de webhooks financeiros.

### 🚥 Comunicação Real-time com SignalR
Utilizamos o `PaymentNotificationHub` para notificar o frontend instantaneamente:
- **Sincronização:** Assim que o Webhook do Mercado Pago confirma o pagamento, o SignalR avisa a interface para atualizar o status do bilhete sem necessidade de refresh.

---

## 🏗️ Endpoints da API (RaffleHub.Api)

Abaixo estão os principais recursos expostos pela API:

### 🎟️ Rifas (`/Raffle`)
- **`GET /Raffle`**: Lista todas as rifas disponíveis.
- **`GET /Raffle/names`**: Lista apenas os nomes das rifas para filtros.
- **`GET /Raffle/{id}`**: Obtém detalhes específicos de uma rifa.
- **`POST /Raffle` [ADMIN]**: Cria uma nova rifa (suporta upload de imagem via `IFormFile`).
- **`PUT /Raffle/{id}` [ADMIN]**: Atualiza dados e imagem de uma rifa existente.
- **`PATCH /Raffle/ChangeStatus/{id}` [ADMIN]**: Altera o status (Ativo/Inativo/Finalizado).

### 👥 Participantes (`/Participant`)
- **`GET /Participant/Raffle/{raffleId}` [ADMIN/OPERATOR]**: Lista participantes de uma rifa específica (Paginado).
- **`POST /Participant`**: Registra um novo participante em uma rifa.
- **`DELETE /Participant/{participantId}/{raffleId}` [ADMIN]**: Remove um participante e limpa suas reservas associadas (Cascade).

### 💳 Reservas e Pagamentos (`/Booking`)
- **`GET /Booking/my-bookings` [AUTH]**: Lista as reservas do usuário autenticado.
- **`GET /Booking/pending/{participantId}`**: Verifica se há reserva pendente para o participante.
- **`POST /Booking/generate-pix/{participantId}`**: Gera o QR Code e a chave Pix via integração **Mercado Pago**.

### 🔐 Autenticação (`/Auth`)
- **`POST /Auth/login`**: Realiza o login e retorna o Token JWT.
- **`POST /Auth/register`**: Registra um novo usuário no sistema.
- **`POST /Auth/refresh-token`**: Atualiza o token expirado.

### 🖼️ Galeria (`/Gallery` & `/CategoriesGallery`)
- **`GET /Gallery`**: Lista imagens da galeria de ganhadores/eventos.
- **`POST /Gallery` [ADMIN/OPERATOR]**: Adiciona nova foto à galeria.
- **`GET /CategoriesGallery`**: Lista categorias da galeria.

### 🔗 Webhooks (`/api/Webhook`)
- **`POST /api/Webhook/mercadopago`**: Listener para notificações de pagamento do Mercado Pago. Valida a transação e confirma a reserva automaticamente.

---

## 🐳 Configuração e Execução

### Opção 1: Via Docker Compose (Recomendado)
Sobe a API e o banco PostgreSQL sincronizados.
1. Na raiz do projeto, execute:
   ```bash
   docker compose up --build
   ```
2. **Acesso:**
   - **API:** `http://localhost:8080`
   - **Swagger:** `http://localhost:8080/swagger/index.html`

### Opção 2: Execução Local (.NET CLI)
1. Ajuste a Connection String no `appsettings.Development.json`.
2. Aplique as migrações:
   ```bash
   dotnet ef database update
   ```
3. Execute a aplicação:
   ```bash
   dotnet run --project RaffleHub.Api
   ```
