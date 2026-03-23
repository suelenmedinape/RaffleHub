# 🖥️ Aplicação Cliente: RaffleHub Frontend (Angular 21)

![Frontend Core Engine](https://via.placeholder.com/150?text=Angular+21)

A porta de entrada visual do sistema de Rifas. Arquitetura SPA Frontend planejada para escalar, performar e ser estritamente fácil de ler com reatividade moderna do ecossistema v21.

[![Build Status](https://img.shields.io/badge/Angular-v21-red)]()
[![Tailwind](https://img.shields.io/badge/CSS-Tailwind-cyan)]()
[![RxJS Signals](https://img.shields.io/badge/Reactivity-Signals-purple)]()

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
Bem-vinda(o)! Este projeto não é apenas "uma casca HTML", abraçando integralmente a filosofia moderna de reatividade da Web.

### A Arquitetura Reativa (Signals vs RxJS)
Para construir o seu "segundo cérebro" mental, imagine que adotamos uma divisão inteligente do fluxo temporal:

- **RxJS (Correntes do Rio)**: Usamos os Observables do RxJS como *tubos de conexão assíncrona*. Eles descrevem o trânsito da "água" que chega da API ao nosso sistema de forma canalizada.
- **Angular Signals (Painel de Status Cristalino)**: Para o *estado visual*. Pense nos Signals como num semáforo. Se a "luz" estiver vermelha, os componentes das Rifas reagem instantaneamente (com zero perdas de ciclo do motor JavaScript V8). O RxJS joga a água no reservatório do Signal, e o DOM apenas observa o reservatório da forma mais rápida!

### Validação com Zod
Formulários são inspecionados em Runtime via **Zod**. As regras interceptam ruídos e tipam os envios de formulário de contato PIX pro Servidor perfeitamente — a garantia C# e TS unida.

## ⚡ Usage
Servirá nossa plataforma amigável web rodando ao lado do TailwindCSS e provendo as imagens das Rifas do Supabase.

### 🔌 Installation
Acesse a pasta interna contendo `package.json` em `RaffleHub/`. Se todos os requisitos constam, utilize seu client de Node Package para injetar o `node_modules`.

### 📦 Commands
```bash
# Rodar Desenvolvimento Nativo
npm start

# Testes de Estresse Angular / Components
npm test
```

## 🔧 Development
### 📓 Pre-Requisites
- Node.js (`v20+`).
- NPM package manager (`> v11`).
- Angular CLI global sugerido e ativo (`npm i -g @angular/cli@21`).

### 🔩 Development Environment
No ambiente de clone, ao executar `npm install`, a configuração primária consiste apenas em dizer com qual roteador API conversar.

`src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### 📁 File Structure
```
.
├── src
│   ├── app
│   │   ├── core             # Módulos vitais, Tokens (A alma imutável).
│   │   ├── shared           # Botões utilitários Globais, pipes, Zod.
│   │   ├── features         # As fatias do bolo de Domínio (Galeria, Pagamento, Autenticação).
│   │   └── app.component.ts # Bandeja que segura e roteia os Standalone Features.
│   ├── assets               # Mosaicos Fixos (Imagens padrão de carregamentos)
│   └── environments         # Enclaves locais x Koyebe Cloud.
└── angular.json
```

### 🔨 Build
Gera as dist minimalistas ES para produção, minificado com standalone:
```bash
npm run build
```

### 🚀 Deployment
Qualquer serviço de CDN passivo ou Plataformas SSR Angular (Vercel/Netlify) podem orquestrar perfeitamente a pasta `/dist`.

## 🌸 Community
O frontend ainda possui um escopo privado de atuação UI. Solicições de layouts são revisadas a Nível de Trello.

## ❓ FAQ
**Porquê meu dropdown do Angular não está funcionando?**
Verifique na injeção de classe Tailwind CSS se os modais via `@tailwindplus/elements` não estão quebrando compatibilidades visuais da div Pai — evite manipulações com DOM Elements via querySelector! 

## 📄 Resources
- [Angular RxJS + Signals](https://angular.dev/guide/signals)
- [Zod TypeScript Validator](https://zod.dev/)

## 📷 Gallery
*(Screenshots contendo o painel interativo da seleção dos Números).*

## 🌟 Credit/Acknowledgment
Desenhado do zero pelos guidelines limpos voltados à UX sem engasgos do V8.

## 🔒 License
Todos os Direitos de Interface e Design Reservados de Acordo com a marca.
