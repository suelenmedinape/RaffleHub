# Rifa C# Backend

Este é o repositório para o backend do sistema de Rifas, desenvolvido utilizando ASP.NET Core 9, Entity Framework Core e outras tecnologias modernas do ecossistema .NET. O projeto segue uma arquitetura limpa e padrões de design como Repositório e Unidade de Trabalho (Unit of Work).

## 🚀 Sobre o Projeto

O objetivo deste projeto é fornecer uma API robusta e segura para gerenciar múltiplas rifas online. Ele permite a criação de rifas, gerenciamento de participantes, venda e controle de bilhetes, autenticação de usuários e uma galeria de imagens para os prêmios.

## ✨ Funcionalidades Principais

- **Gerenciamento de Rifas**: CRUD completo para rifas, com status (Aberta, Finalizada, etc.).
- **Gerenciamento de Participantes**: Cadastro e controle de informações dos participantes.
- **Controle de Bilhetes**: Lógica para reserva e marcação de bilhetes como pagos.
- **Autenticação e Autorização**: Sistema de login baseado em JWT (JSON Web Tokens) com papéis de usuário (Roles).
- **Galeria de Imagens**: Upload e gerenciamento de imagens para as rifas, com integração a um serviço de armazenamento (como Supabase).
- **Consultas e Relatórios**: Endpoints para sumarizar dados, como detalhes de uma rifa específica e listagem de todas as rifas.

## 🛠️ Tecnologias Utilizadas

- **Framework**: .NET 9 / ASP.NET Core 9
- **ORM**: Entity Framework Core 8+
- **Banco de Dados**: Projetado para ser usado com PostgreSQL (mas pode ser adaptado para outros bancos suportados pelo EF Core).
- **Autenticação**: ASP.NET Core Identity e JWT Bearer Tokens.
- **Mapeamento de Objetos**: AutoMapper
- **Testes**: xUnit
- **Containerização**: Docker
- **Documentação da API**: Swagger (OpenAPI)
- **Armazenamento de Arquivos**: Integração com [Supabase Storage](https://supabase.com/docs/guides/storage) para a galeria de imagens.

## ✅ Pré-requisitos

Antes de começar, você precisará ter o seguinte instalado em sua máquina:
- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) ou superior.
- [Docker](https://www.docker.com/get-started) (Opcional, para execução em contêiner).
- Um editor de código de sua preferência (e.g., VS Code, JetBrains Rider).
- Uma instância de banco de dados PostgreSQL.

## ⚙️ Como Executar o Projeto

1. **Clone o repositório:**
   ```bash
   git clone <url-do-seu-repositorio>
   cd rifa-csharp/rifa-csharp
   ```

2. **Configure as Variáveis de Ambiente:**
   Renomeie ou crie o arquivo `appsettings.Development.json`. Você precisará configurar a string de conexão com o banco de dados e as credenciais do Supabase.

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=RifasDB;Username=postgres;Password=your_password"
     },
     "Supabase": {
       "Url": "YOUR_SUPABASE_URL",
       "ApiKey": "YOUR_SUPABASE_API_KEY"
     },
     "JWT": {
        "Secret": "YOUR_SUPER_SECRET_KEY_HERE_WITH_AT_LEAST_32_CHARS",
        "Issuer": "https://localhost:7123",
        "Audience": "https://localhost:7123"
     }
   }
   ```

3. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```

4. **Aplique as Migrations do Banco de Dados:**
   O Entity Framework Core usará as migrations para criar o schema do banco de dados para você.
   ```bash
   dotnet ef database update
   ```
   *Nota: Se o comando `dotnet ef` não for encontrado, instale-o globalmente com `dotnet tool install --global dotnet-ef`.*

5. **Execute a aplicação:**
   ```bash
   dotnet run
   ```
   A API estará disponível em `https://localhost:7123` (ou a porta configurada em `Properties/launchSettings.json`).

### Executando com Docker

1. Certifique-se de que sua `appsettings.json` esteja configurada para se conectar a uma instância do PostgreSQL acessível pelo contêiner (pode ser necessário usar o endereço de rede do host em vez de `localhost`).

2. Construa a imagem Docker:
   ```bash
   docker build -t rifa-csharp -f Dockerfile .
   ```

3. Execute o contêiner:
   ```bash
   docker run -p 8080:8080 -e "ASPNETCORE_URLS=http://+:8080" rifa-csharp
   ```
   A API estará disponível em `http://localhost:8080`.

## 🗂️ Estrutura do Projeto

A solução está dividida em pastas que separam as responsabilidades:

- `Controller/`: Contém os endpoints da API (Controllers).
- `Data/`: Configuração do `AppDbContext` do Entity Framework.
- `DTO/`: Data Transfer Objects, usados para modelar os dados que entram e saem da API.
- `Entities/`: As classes de domínio que são mapeadas para as tabelas do banco de dados.
- `Enums/`: Enumerações utilizadas no projeto.
- `Interface/`: Contratos para os serviços e repositórios (Injeção de Dependência).
- `Migrations/`: Arquivos de migração do banco de dados gerados pelo EF Core.
- `Repositories/`: Implementação dos padrões de design Repositório e Unit of Work.
- `Service/`: Onde reside a lógica de negócios da aplicação.
- `Utils/`: Classes utilitárias, como perfis de mapeamento do AutoMapper.

## 📄 Endpoints da API

Após executar a aplicação, a documentação completa e interativa da API estará disponível via Swagger UI.

Acesse: **[https://localhost:5209/swagger/index.html](https://localhost:5209/swagger/index.html)**

Lá você poderá ver todos os endpoints, seus parâmetros, e até mesmo testá-los diretamente pelo navegador.
