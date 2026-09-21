# DevJobAlerter 🚀

O **DevJobAlerter** é uma aplicação fullstack composta por um serviço em segundo plano (**Worker Service em .NET 10**) que busca vagas de desenvolvimento de software na API da **Adzuna**, envia alertas em tempo real para o WhatsApp via **Evolution API**, e um painel web em **React + Vite** para visualização e gerenciamento das vagas.

---

## 🏗️ Arquitetura do Projeto

O repositório está dividido de forma modular entre aplicações de Backend e Frontend:

| Diretório | Camada / App | Descrição | Componentes Principais |
| :--- | :--- | :--- | :--- |
| `backend/` | `DevJobAlerter.Domain` | Regras de negócio, entidades e interfaces de serviços. | `JobVacancy`, `SentJob`, `IJobService`, `INotificationService`, `IJobRepository` |
| `backend/` | `DevJobAlerter.Infrastructure` | Contextos de banco de dados, repositórios e integração com a API da Adzuna. | `AdzunaJobService`, `ApiWhatsAppNotificationService`, `AppDbContext`, `JobRepository` |
| `backend/` | `DevJobAlerter.Worker` | Ponto de entrada do serviço em segundo plano. | `Program.cs`, `Worker.cs`, `JobSearchSettings`, `Dockerfile` |
| `backend/` | `DevJobAlerter.Api` | Interface Web API para consulta de dados no frontend. | `Program.cs`, `JobsController` |
| `frontend/` | Painel Web | Interface visual para exibição de vagas e métricas. | React, Vite, TypeScript |

---

## 🛠️ Tecnologias e Ferramentas

- **.NET 10 (Worker Service & Web API)**: Motor principal de execução e busca de vagas em segundo plano.
- **API Adzuna**: API externa REST utilizada para consulta e agregação de oportunidades de trabalho.
- **React & Vite**: Interface web rápida para navegação e acompanhamento dos alertas.
- **Entity Framework Core & SQLite**: Armazenamento persistente das vagas enviadas para evitar notificações duplicadas.
- **Evolution API**: API REST para envio automatizado de mensagens no WhatsApp.
- **PostgreSQL**: Banco de dados relacional para persistência de estado da Evolution API.
- **Docker & Docker Compose**: Containerização completa da infraestrutura de backend, bancos e serviços.

---

## 🔄 Fluxo de Execução

1. O **Worker Service** é executado periodicamente com base nos termos de busca configurados.
2. O **Adzuna Job Service** consulta novas oportunidades de trabalho diretamente na API externa da **Adzuna**.
3. O **Repositório SQLite** verifica se a URL da vaga já foi processada anteriormente.
4. O **Serviço de Notificação** formata e envia os detalhes da vaga para o WhatsApp através da **Evolution API**.
5. As vagas notificadas são salvas no banco SQLite para evitar alertas repetidos.

---

## 🚀 Como Executar o Projeto

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução.
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (opcional, para desenvolvimento C# local).
- [Node.js](https://nodejs.org/) (opcional, para executar o frontend localmente fora do Docker).

### Configuração de Cargos e Parâmetros (`appsettings.json`)

Por boas práticas de segurança, o arquivo de configurações `appsettings.json` está oculto no `.gitignore` para evitar o envio acidental de credenciais para o repositório.

1. Navegue até a pasta do projeto Worker:
   ```bash
   cd backend/src/DevJobAlerter.Worker

2. Crie o seu arquivo appsettings.json local:
   ```bash
   touch appsettings.json   

3. Defina os cargos desejados e os parâmetros de execução no arquivo criado (dentro de SearchTerms):
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     },
     "JobSearchSettings": {
       "SearchTerms": [
         ".NET Developer",
         "C# Developer",
         "Fullstack"
       ],
       "SearchIntervalMinutes": 60
     }
   }

## Execução via Docker

1. Clone o repositório e acesse a pasta raiz:
   ```bash
   git clone [https://github.com/leoloss/devjobalerter-project-dotnet.git](https://github.com/leoloss/devjobalerter-project-dotnet.git)
   cd devjobalerter-project-dotnet

2. Execute a aplicação via Docker Compose: 
   ```bash
   docker compose up -d --build
   
