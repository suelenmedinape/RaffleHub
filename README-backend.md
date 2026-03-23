# ⚙️ Serviços de Back-Office: RaffleHub Backend (ASP.NET Core 9)

![Backend Core Engine](https://via.placeholder.com/150?text=ASP.NET+Core)

A API REST centralizada e implacável do ecossistema RaffleHub, que gerencia todo o modelo de integridade do Banco de dados e webhooks.

[![Build Status](https://img.shields.io/badge/.NET-9.0-purple)]()
[![EF Core](https://img.shields.io/badge/EF_Core-Latest-green)]()

## 📒 Index
- [🔰 About](#-about)
- [⚡ Usage](#-usage)
  - [🔌 Installation](#-installation)
  - [📦 Commands](#-commands)
- [🔧 Development](#-development)
  - [📓 Pre-Requisites](#-pre-requisites)
  - [🔩 Development Environment](#-development-environment)
  - [📁 File Structure](#-file-structure)
  - [🔨 Build](#-build)
  - [🚀 Deployment](#-deployment)
- [🌸 Community](#-community)
- [❓ FAQ](#-faq)
- [📄 Resources](#-resources)
- [📷 Gallery](#-gallery)
- [🌟 Credit/Acknowledgment](#-credit-acknowledgment)
- [🔒 License](#-license)

## 🔰 About
Bem-vinda(o) ao motor lógico do RaffleHub. Este não é um monolito legado ou de padrões espaguete; é uma API elegante desenvolvida sob as estritas e robustas regras de **Clean Architecture simplificada** para microprojetos em .NET 9.0 com C#.

A arquitetura não segue "Atochamento de Padrões" cegos. Focamos no domínio, injeção fluida e previsibilidade nos fluxos assíncronos (Afinal, lidar com pagamentos de Rifas de terceiros e Webhooks reais pressupõem precisão, caso falhe teríamos usuários não recompensados).

### Fim da "Gambiarra de Exceptions"
No nosso fluxo, usamos o **FluentResults `Result<T>`**. 
> Ao invés de lançar a caixa na parede por estar vazia (Throw Exception), sempre devolvemos uma caixa lacrada ("Result"). 

## ⚡ Usage
Utilize as rotas baseadas em REST purificado a partir do navegador (Swagger UI) ou de seus Postmans. Toda rota protegida exige portabilidade do JWT Token consumido nos endpoints de Auth.

### 🔌 Installation
Certifique-se que o motor do PostgreSQL está em execução na sua máquina. Vá ao caminho raiz `RaffleHub.Api` do código C# e deixe que a ferramente global restaure tudo.

### 📦 Commands
```bash
# Baixar dependências .NET
dotnet restore

# Migração de BD
dotnet ef database update

# Disparo para verificação local de rotinas de Run
dotnet run
```

## 🔧 Development
### 📓 Pre-Requisites
- SDK do .NET 9 -> `dotnet --info`
- Ferramentas EF Core instaladas -> `dotnet tool install --global dotnet-ef`
- Uma Cloud / Bucket Key do [Supabase].

### 🔩 Development Environment
Atrelamos um `appsettings.Development.json` no mesmo patamar do arquivo principal `Program.cs`. Esse arquivo JSON possui o esqueleto a seguir que reflete os secrets para compilação local (Estes que não sobem com as branches):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5445;Database=RaffleDb;Username=default;Password=default;"
  },
  "Supabase": {
    "Url": "https://seu-supabase-project.supabase.co",
    "Key": "CHAVE_SECRETA_SUPABASE"
  },
  "JwtSettings": { "Secret": "Sua_Chave_De_Protecao_32_Carac_+" }
}
```

### 📁 File Structure
```
.
├── RaffleHub.Api
│   ├── Controllers               // Roteadores puros Web.
│   ├── DTOs                      // Mapeamentos Seguros.
│   └── Program.cs                // Configurações mestre de infraestrutura e dependências.
├── RaffleHub.Tests.E2E           // Testes de estresse (Gateway e Requests Mockadas)
├── RaffleHub.Tests.Unit          // Unidade do fluent results
└── RaffleHub.sln                 // O arquivo chave de Solução .NET
```

### 🔨 Build
Compile a aplicação para binários independentes do sistema usando:
```bash
dotnet build --configuration Release
```

### 🚀 Deployment
Este software já é preparado contendo o `Dockerfile` isolando o publish final de C# `Release`. Pode comitar e aplicar deploy direto integrado pela imagem rodada na plataforma de orquestração como Koyeb (Atenção às env vars de Port Binding `ASPNETCORE_URLS`).

## 🌸 Community
Certas camadas ainda encontram-se dependentes estritamente de domínios restritos à proprietária. Pull-requests podem ser sugeridas visando refatorar performance em LINQ Query ou Otimizações EF Tracking.

## ❓ FAQ
**O que é o Hangfire citado logando no console?**
É o motor que fica operando transações pesadas numa Thread fantasma em fundo para não congelar o CancellationToken da Rota Web do Front. Ele usa a mesma conexão PostgreSQL!

## 📄 Resources
- [.NET 9 O que há de novo](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)

## 📷 Gallery
*(Aqui ficarão os Prints do nosso robusto painel do Swagger Web e Dashboards Serilog/Seq)*

## 🌟 Credit/Acknowledgment
Ao arquiteto de Software .NET e a persistência na busca pela fluidez no Result Pattern.

## 🔒 License
Propriedade Fechada de Sistema. Uso e venda estritos à dona orgânica proprietária.
