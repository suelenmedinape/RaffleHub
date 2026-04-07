# 🎟️ RaffleHub - Ecossistema Digital de Rifas de Alta Performance

![RaffleHub Header](/img-site/home.png)

> [!TIP]
> **A Metáfora Pedagógica - "A Cozinha Digital Sincronizada"**
>
> Para visualizar a essência do RaffleHub, imagine um restaurante de altíssima demanda, onde não pode haver gargalos ou falhas de comunicação:
> - **O Salão e o Menu (Frontend Angular 21):** Onde o cliente interage. Rápido, visual e reativo. Através de *Signals* (como garçons com comunicação instantânea via rádio), o cliente vê imediatamente se um prato (bilhete) da mesa ao lado foi reservado de forma síncrona e fluida.
> - **O Cérebro Operacional e a Cozinha (Backend .NET 9):** O Chef manager e o seu ecossistema. Ele valida cada pedido com precisão matemática utilizando o *Result Pattern* e interceptadores/middlewares globais para não quebrar o fluxo com ruídos irrelevantes. Ele inspeciona a despensa e garante a transação do pagamento sem bloqueios de "deadlock".
> - **Os Auxiliares Dedicados (Hangfire & Serviços Terceiros):** Tarefas que não devem atrasar a experiência no Salão (como expirar bilhetes não pagos ou enviar recibos pelo WhatsApp via provedor independente) operam num fluxo de segundo plano. O Chef simplesmente despacha para o auxiliar, mantendo o serviço impecável.

---

## 🚀 Visão Geral do Projeto

O **RaffleHub** transcende a ideia de um simples sistema trivial de numeração paralela. É uma solução integral de software robusto (*Full Stack*), orquestrada para solucionar o clássico desafio de escalabilidade da alta concorrência digital e race conditions na compra simultânea. Ao integrar um backend resiliente projetado em .NET 9 de alta reatividade, com uma camada front-end avançada rodando Angular e comunicação de persistência direcional do SignalR, o sistema blinda a ocorrência de ambiguidades operacionais minimizando as frações de tempo nas seleções.

Ele viabiliza toda a cadeia produtiva digital: navegação, seleção, travamento atômico, transação transfronteiriça com a API do Mercado Pago via webhooks, e finalmente a expedição instantânea do comprovante automatizado e final via integração de WhatsApp.

### 🏗️ O Mapa da Mina (Integração Arquitetural)

```mermaid
graph TD
    User((Usuário Final))
    
    subgraph "Camada de Experiência (Angular 21)"
        UI[Standalone Components & Templates]
        Signals{Fluxo Reativo: RxJS + Signals}
        Zod[Scanner & Validador (Zod)]
    end
    
    subgraph "Motor Central C# (.NET 9 Web API)"
        Controllers[API REST & Middlewares]
        Business[Regras Estritas e Result Pattern]
        SignalR_Hub[Broadcaster Real-Time]
    end
    
    subgraph "Camada de Dados & Jobs"
        Postgres[(PostgreSQL)]
        Hangfire[Assincronia e Mensageria Background]
    end
    
    subgraph "Integrações Exteriores"
        MP[Mercado Pago Cloud]
        WAHA[Waha API Gateway]
    end

    User <-->|HTTPS Segura| UI
    UI --> Zod
    UI <-->|Bidirecional SignalR WebSocket| SignalR_Hub
    Zod --> Controllers
    Controllers <--> Business
    Business <--> Postgres
    Business --> Hangfire
    SignalR_Hub <--> Business
    
    Business --> MP
    MP -.->|Webhook de Confirmação| Controllers
    Hangfire --> WAHA
    WAHA -.->|Envio Passivo de Comprovante| User
```

---

## ⚡ Fluxo de Vida de uma Reserva (The Golden Path)

1. **A Escolha e o Bloqueio Imediato:** Assim que o toque/clique de fato acontece em nível UI, e transposto os interceptadores base, o **SignalR** replica instantaneamente aos pares o "trave" de um recurso numérico. Nenhuma recarga de página é requerida para os demais entenderem que um ticket deixou a prateleira da concorrência.
2. **Triagem de Requisito Operacional:** O endpoint C# inspeciona estaticamente. Validações estritas entre ecossistema Frontend via **Zod** complementam-se à estrutura de FluentValidation acoplada no pipeline de submissão do Back. Um novo registro ganha forma e assento no **PostgreSQL** com Status Primário Pendente.
3. **Resolução de Autenticidade Financeira:** Os DTOs trafegam até um agente parceiro (A infraestrutura do **Mercado Pago**). Retorna veloz com matriz de leitura de Pix e Copy-Paste parametrizado sem reenvio de payload massivo de domínios.
4. **Ciclo de Sobrevivência Condicional:** Fica latente sob o vigília do motor passivo **Hangfire**. Ele atua como um inspetor temporizador; se a submissão não encontrar êxito efetivo dentro da estipulação do limite temporal lógico (Ex: 15 min), a mesa sofre varredura e o registro limado na camada nativa sem sacrificar o Event Loop principal.
5. **Automação Via Confirmação e a Finalização Sensorial:** Responde-se mediante invocação indireta Webhook que reescreve os bancos para Pago. Dispara relâmpago de regresso pelo SignalR atualizando o nó DOM associado (brilho verde validado para todos os observadores passivos da sessão) e sinaliza o último componente.
6. **Desfecho Integrado e Elegante:** Uma tarefa assíncrona absorve o *ticket pass*, interage com a suíte de credenciais autorizada na API do WAHA devolvendo uma nota final e clara diretamente depositada na estrutura de mensageria da base do usuário/comprador final (O WhatsApp dele recebe a imagem/fatura via background job suave).

---

## 🛠️ Trilha Tecnológica e Requisitos de Alto Padrão

| Subdivisão do Ecossistema | A Lógica e Tecnologia Designada |
| :--- | :--- |
| **Experiência Limpa UI/UX** | **Angular 21 + Flowbite + Tailwind Moderno.** Eliminação do engessamento. Data-binding reativo avançado (Usando injeções por propriedades `[checked]`, `[disabled]`), fulminando completamente o antipattern da manipulação direta massiva da árvore do DOM. |
| **Estados Síncronos / Rastreio Reativo**| **Signals combinados a RxJS de Transição.** Preserva-se a sanidade do modelo de memória e ciclos do Garbage Collector garantindo repinturas localizadas rápidas (V8 Friendly), sem inflar os watchers sistêmicos globais desnecessários e verbosos. |
| **Arquitetura Resiliente do Backend** | **C# .NET 9 ASP.NET Core.** Separação tática. Controllers explícitos e semânticos (no plural) despachando Requests diretas ao domínio, formatadas estritamente com DTOs imutáveis (consequentemente com sintaxe nativa apropriada PascalCase).  |
| **Contrato Dinâmico & Abstração de Dados** | **Entity Framework Core (Npgsql) Injectado.** Adoção do pragmatismo DI. Injeção direta e simplificada do DbContext dispensando amarras teóricas extras do "UnitOfWork" customizado sob o próprio UoW nativo, promovendo clareza transacional em aplicações Cloud Native limpas. |
| **Engenharia de Erros Uniforme** | **Result Pattern sobre Exceptions Exaustivas.** Retira o acoplamento severo. Validações lógicas (email errado/reserva duplicada) não cospem "Exceptions" que quebram o fluxo normal na "Runtime". O erro é um elemento de devolução legível de domínios; as "Exceptions" de escopo verdadeiro (quedas e conexões corrompidas) recaem limpas num Handler de Middleware unificado HTTP global. |

---

## 📦 Gestão de Diretórios e Aceleração

A raiz do ecossistema trabalha segmentadamente baseada nas premissas contêiner da engenharia atual (ambos repositórios albergam compose scripts). Mova para dentro dos núcleos específicos de leitura para visualizar nuances:

- 🧠 **Camada Funcional Lógica:** [Explorar o README Arquitetural do Core Base (rifa-backend/README-backend.md)](README-backend.md)
- 🎨 **Camada de Visibilidade Dinâmica:** [Explorar o README Operacional Reativo Frontend (rifa-frontend/README-frontend.md)](README-frontend.md)

---

## 🎯 Avaliações de Manutenibilidade a Longo Prazo e Work-in-Progress

- **Relatórios Gerenciais Reativos:** Melhorias já elaboradas abrigam visões unificadas sobre Painel e Histórico do Administrador no Frontend mapeando as agregações cruéis que agora podem transitar sob endpoints focados em resumos parciais. Escalar fluxos informacionais da matriz PostgreSQL com view read-only sob memórias ativas. 

---

> [!CAUTION]
> **Notas do Cérebro Pessoal e Assinatura Intelectual**
>
> **Arquiteta:** Suelen, Engenheira que preza Fundamentos acima do Abstrato.
> Este repositório além de operar na fronteira Cloud atual de reatividade, comporta os devaneios e ensinadores de um vasto *Segundo Cérebro* (Obsidian) transmutado em software executável. O design repousa firmemente numa leitura afiada para com as Entranhas do JavaScript, o manuseio de Ciclos Limpos de Memória da V8 Machine, e uma devoção total aos moldes contemporâneos maduros que regem o ecossistema e pipelines rigorosos de APIs REST modeladas à perfeição. O código é uma métrica visual, desenhado contra eventuais "Puxadinhos" ou acoplamentos espúrios e estáticos irrelevantes.
