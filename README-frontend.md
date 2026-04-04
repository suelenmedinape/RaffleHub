# 🖥️ RaffleHub Frontend - O Motor de Experiência Reativa (Angular 21)

![Frontend Header](file:///home/suelen/Documents/Baja/rifas/rifas/rifa-frontend/docs/assets/frontend_header.png)

A face visível do ecossistema RaffleHub. Esta SPA (Single Page Application) foi construída para desafiar os limites da reatividade web moderna, garantindo que o usuário tenha uma experiência instantânea, sem recarregamentos e com total integridade de dados.

[![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?style=for-the-badge&logo=angular)]()
[![Signals](https://img.shields.io/badge/Reactivity-Signals-purple?style=for-the-badge)]()
[![Tailwind](https://img.shields.io/badge/CSS-Tailwind_4-38B2AC?style=for-the-badge&logo=tailwind-css)]()

---

## 🔰 A Filosofia de Reatividade

Para este projeto, adotei um modelo híbrido de gerenciamento de estado que aproveita o melhor do ecossistema Angular:

### ⚡ Signals: O Coração da Interface
Diferente do modelo tradicional baseado apenas em Observables, os **Angular Signals** são usados para o estado visual síncrono. Isso significa:
- **Zero Inconsistência:** Se o número foi clicado, o botão reflete a mudança no mesmo ciclo do microtask.
- **Performance V8:** Menos verificações de mudança (Change Detection) globais, focando apenas no componente que realmente mudou.

### 🌊 RxJS + SignalR: O Fluxo em Tempo Real
Enquanto os Signals cuidam do "agora", o **RxJS** e o **SignalR** cuidam do "remoto":
- **Websockets:** O frontend mantém uma conexão viva com o `PaymentNotificationHub` via SignalR.
- **Notificações:** Quando um ticket é pago, a interface recebe um evento e brilha o número em verde instantaneamente para todos os usuários logados.

---

## ✨ Demonstração da Experiência

<carousel>
![Exploração](file:///home/suelen/Documents/Baja/rifas/rifas/rifa-frontend/docs/assets/homepage.png)
<!-- slide -->
![Seleção de Números](file:///home/suelen/Documents/Baja/rifas/rifas/rifa-frontend/docs/assets/raffle_details.png)
<!-- slide -->
![Login Seguro](file:///home/suelen/Documents/Baja/rifas/rifas/rifa-frontend/docs/assets/login.png)
</carousel>

---

## 🛠️ Detalhes de Engenharia Frontend

### 1. Integridade de Dados com Zod
Não confiamos apenas no TypeScript. Utilizamos **Zod** para validar schemas de formulários em tempo de execução. Isso impede que dados "sujos" cheguem à nossa API, servindo como uma primeira linha de defesa robusta.

### 2. Standalone & Modern Control Flow
- O projeto é **Standalone First**, eliminando a verbosidade dos `NgModules`.
- Utilizamos a nova sintaxe de **Control Flow** (`@if`, `@for`, `@switch`), que é nativamente mais rápida e legível.

---

## 📁 Arquitetura de Pastas (Clean Frontend)

```
src/app
├── core/         # Singleton Services (Auth, Interceptors, HubConnection).
├── shared/       # Components reutilizáveis (Input, Button) e Schemas Zod.
├── features/     # Módulos de domínio (Gallery, RaffleDetails, MyBookings).
├── services/     # Camada de comunicação HTTP pura.
└── pages/        # View Components que agrupam os features.
```

---

## 📦 Comandos e Setup

1. **Instalação Profissional:**
   ```bash
   npm install --frozen-lockfile
   ```
2. **Launch Server:**
   ```bash
   npm start
   ```
3. **Build for Production:**
   ```bash
   npm run build -- --configuration production
   ```

---
> [!NOTE]
> Configurado para comunicar-se dinamicamente com o backend via `environments/environment.ts`, suportando deploys em Vercel e Koyeb.
