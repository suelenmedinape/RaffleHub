# ⚙️ RaffleHub Backend - O Motor Operacional (ASP.NET Core 9 / C#)

![Backend Header](/img-site/home.png)

A artilharia pesada e o orquestrador silencioso do ecossistema RaffleHub. Esta camada RESTFUL foi forjada escapando da mediocridade do CRUD primitivo e desenhada usando práticas exigentes de Clean Architecture reavaliadas, injetando segurança rigorosa, integridade condicional e independência da engine de frameworks engessados que acoplavam domínios no passado.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)]()
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)]()
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-336791?style=for-the-badge&logo=postgresql)]()

---

## 🏛️ A Mente por Trás do Código (Práticas Arquiteturais)

O refinamento e a eficiência são priorizados na arquitetura implementada. Uma releitura de domínios garante a alta compatibilidade, testabilidade limpa, bem como o isolamento das complexidades operacionais do cotidiano que muitas vezes acabam afogadas em uma sopa de códigos imperativos. 

### 💎 Descartando Exceções "Caras" (The Result Pattern)
Na concepção mais engessada do C#, muitos sistemas disparam `Exceptions` para situações comuns como um login renegado, email repetido ou saldo esgotado. Essa abordagem trava threads, incha o lixo de memória com logs imensos (Stack Traces massivos) ativando mais varreduras do sistema.
Neste projeto adotou-se o modelo puro de **Retornos de Fluxo Via Padrão Result** (`Result<T>`).
- Quando há violação de regra de negócio corriqueira, o serviço lida polidamente encapsulando a falha explícita no encapsulador tipado do Result e reporta de forma fluida. O servidor não precisa construir *Exceptions* gigantes para avisar que aquele número exato já havia sido pago.
- O Middleware Extremo lida apenas e unicamente de fato com falhas catastróficas inesperadas no servidor (conexões rompidas abruptamente, transbordos e etc). Tudo retornando invariavelmente mapeado num molde visual simétrico `ErrorResponse` garantindo a comunicação inquebrantável entre back e o parseamento Zod/Typescript Frontend.

### 🧹 Simplicidade do Setup (O Fim do Falso UnitOfWork Genérico)
Ao longo das gerações observamos redundâncias criando acoplamentos sombrios em aplicações sem necessidade.
Neste C#, eliminamos a "Casca Genérica de Repositórios Globais" por cima do DB. O Entity Framework Core subjacente (`DbContext`) já opera sobre o design pattern de Transaction Commit (`SaveChangesAsync`).  
Adotamos então a **Injeção de Dependências Direta do EF Context** nos Repositórios Restritos Especializados, ganhando previsibilidade cirúrgica. E as nomenclaturas são sagradas: Controllers pluralizados (`/Categories`, `/Raffles`) refletindo recursos unicamente sob Request/Responses seladas com escopos rígidos expostas a fora (As sagradas transições via DTOs puritanos PascalCase isolados sem corromper entidades nobres locais). 

---

## 🚀 Engrenagens de Concorrência Assíncrona e Trabalhadores

O projeto afasta lentidões causadas por excessos imperativos engolindo a rede vital e principal da Thread HTTP via desmembramentos modulares.

### 🔄 Hangfire e a Coréia de Trabalhos Off-Thread 
Existem fardos demorados que o usuário final no salão não tem porque encarar com "loaders" eternos girando na UI:
- **Expiração Condicional (Reciclagem Lógica):** De tempos em tempos minunciosamente calculados, Hangfire limpa registros bloqueados que a reserva pix jamais adentrou no fluxo vital de forma satisfatória temporal.  
- **Desencadeamento por Terceiros Estáticos (Extensibilidade WAHA):** Após a notificação do provedor financeiro da estabilização da compra, esse mensageiro injeta no sub-fluxo para notificar em delay as caixas de conversas WAHA mantendo o Gateway leve e responsabilidades distanciadas umas da outras.


### 🚥 SignalR Embutido: O Bloqueio Visceral Visual de Múltiplas Chamadas
Para transmistir alterações atómicas em alta prioridade nas páginas dinâmicas, utilizamos Websockets vivos do SignalR para atuar não no DB mas no próprio barramento da sessão conectada, travando visivelmente, espelhando "Em Reserva" instantaneamente pelo painel de controle antes do processamento rígido do Postgre sequer findar um escopo, e extinguindo RaceConditions da interface em milissegundos limpos.

---

## 🏗️ Estrutura Tátil de Endpoints

#### Serviço Financeiro & Conexões Nativas (`/RequisitoDeAPI`)
- **`POST /Transactions`**: Ingestor principal de liquidez. Checa disponibilidade imediata, integra-se agressivamente à conta sandbox via bibliotecas MercadoPago injetadas de fora, salva preliminar garantindo `Guid` base, e finalizado emite objeto QRCode base-64. Regras exclusivas atuam (checa idade do emissor conforme enum de finalidade de Receita ou Gasto vs Identidade no payload antes mesmo da montagem).
- **`POST /Api/Webhook`**: Rota perfeitamente configurada via IPN Listener; decodifica verificação para autenticidade em HMAC, espelha o status interno e empurra SignalR + Hangfire Broadcast.

#### Entidades Controladas Plurais (`/Persons`, `/Categories`, `/Raffles`)
- Controllers atômicos provendo deleções pesadas lógicas limpas que refletem no EF Core por intermédio das tabelas dependentes (Evita orfans indesejados). Validação dupla através da infra das Regras Negocio injetadas em Services limpos sobre DataAnnotations básicas prévias.

---

## 🐳 Ambientação Tática, Migrations Estritas e Execução Diária

Os fluxos são concebidos com orquestradores amigáveis. Escolha seu caminho de Setup baseado na facilidade nativa. Serilog está parametrizado por trás para varrer via console logs estruturados permitindo investigação aprofundada pós-eventos de transações caso queira olhar os rastros.

#### Módulo Orquestrado via Contêiner Isométrico (A Receita Inquebrável)
A maneira isenta de falhas ambientais para levantar a base persistente unida à imagem executável e enxuta do seu SDK .NET para homologação espelhada:

1. **Gatilho Único e Montagem Transversal:** Acesse a raiz principal interna e elimine os volumes residuais antes da ignição:
   ```bash
   docker compose down -v
   docker compose up --build
   ```
2. **Reconhecimento Estrito Automático:** O sistema efetuará e garantirá a injeção migrativa de sub tabelas, instanciando os serviços unificados escutando a sua porta `:8080` de base (Verificar reflexos em bindings).

#### Módulo Experimental Rápido Local 
Tradições de CLI .NET nativo sem orquestração cruzada para exploração aprimorada interativa: 
1. Sublinhar adequadamente com as conexões e credencial via `appsettings` focado.
2. Aplicar alinhamento das amarras do Postgre SQL:
   ```bash
   dotnet ef database update
   ```
3. Subida primária do host:
   ```bash
   dotnet run --urls "http://localhost:5124"
   ```
Documentação visual nativa viva operante sob os indexadores nativos em Swagger disponível para consumo analítico restrito de modelagens HTTP sob `/swagger/`.
