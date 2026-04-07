# 🖥️ RaffleHub Frontend - O Motor de Experiência Reativa (Angular 21)

![Frontend Header](/img-site/home.png)

A face visível do ecossistema RaffleHub. Esta SPA (Single Page Application) foi construída para desafiar os limites da reatividade web moderna, garantindo que o usuário tenha uma experiência instantânea, segura e com total integridade de dados através de uma arquitetura limpa e performática.

[![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?style=for-the-badge&logo=angular)]()
[![Signals](https://img.shields.io/badge/Reactivity-Signals-purple?style=for-the-badge)]()
[![Tailwind](https://img.shields.io/badge/CSS-Tailwind_4-38B2AC?style=for-the-badge&logo=tailwind-css)]()

---

## 🔰 Filosofia de Reatividade e Estado

Adotamos o modelo de **Angular Signals** para o gerenciamento de estado síncrono da interface, o que permite:
- **Performance Granular:** Apenas o componente que depende de um sinal específico é renderizado novamente, economizando ciclos da Engine V8.
- **Zero Manipulação de DOM:** Eliminamos o uso de `document.getElementById` ou similares em favor de `Data-Binding` declarativo direto via `[checked]`, `[disabled]` e o novo `Modern Control Flow` (`@if`, `@for`).

---

## ✨ Principais Páginas e Funcionalidades

- **🏠 Home:** Landing page com destaques e informações sobre como participar.
- **🎫 Lista de Rifas:** Vitrine dinâmica de rifas ativas, buscando dados via `RaffleService`.
- **🔍 Detalhes da Rifa:** Seleção interativa de números (tickets) com bloqueio via SignalR.
- **🛒 Minhas Reservas:** Área restrita para o participante acompanhar o status de seus bilhetes.
- **💳 Página de Pagamento:** Exibição do QR Code Pix dinâmico gerado via Mercado Pago.
- **🔐 Autenticação:** Fluxo completo de Login e Registro com suporte a Refresh Tokens.
- **🖼️ Galeria:** Exibição de ganhadores e eventos anteriores, organizada por categorias.

---

## 🛠️ Detalhes de Engenharia Frontend

### 1. Validação Robusta com Zod
Não dependemos apenas da tipagem estática do TypeScript. Utilizamos **Zod** para validar schemas de formulários e payloads de API em tempo de execução, garantindo que nenhum dado inconsistente seja processado.

### 2. Standalone Architecture
O projeto é **100% Standalone**, eliminando a necessidade de `NgModules` verbosos e facilitando o Tree Shaking na compilação final.

### 3. Integração SignalR
O frontend mantém conexões vivas com o backend para atualizações em tempo real. Quando um ticket é reservado ou pago por outro usuário, a interface reflete a mudança instantaneamente sem necessidade de atualizar a página.

---

## 📁 Arquitetura de Pastas

```text
src/app/
├── core/         # Singleton Services (Auth, HubConnection, Interceptors)
├── shared/       # Components (Buttons, Modais) e Pipes reutilizáveis
├── pages/        # Views principais (Home, RaffleList, Auth)
├── participant/  # Módulos específicos (MyBookings, PaymentPage)
├── service/      # Camada de comunicação HTTP (RaffleService, BookingService)
└── models/       # Interfaces e Tipagens TypeScript
```

---

## 🚀 Setup e Desenvolvimento

1. **Instalação:**
   ```bash
   npm install
   ```
2. **Launch Server:**
   ```bash
   npm start
   ```
3. **Configuração de API:**
   A URL base da API é configurada em `src/core/url.ts`. Para desenvolvimento local, aponte para `http://localhost:5124`.

---
> [!NOTE]
> Estilizado com **Tailwind CSS v4** e componentes **Flowbite**, garantindo uma UI moderna, responsiva e acessível.
