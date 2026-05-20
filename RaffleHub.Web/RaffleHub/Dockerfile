# Estágio único para execução de testes unitários em ambiente isolado
FROM node:20-alpine AS test-runner

WORKDIR /app

# Copia arquivos de configuração de pacotes para otimização de cache do Docker
COPY package*.json ./

# Instala todas as dependências (incluindo devDependencies necessárias para teste)
RUN npm ci

# Copia o restante do código fonte do projeto
COPY . .

# Comando de execução padrão para rodar a suíte de testes do Vitest através do Angular CLI
CMD ["npm", "run", "test"]
