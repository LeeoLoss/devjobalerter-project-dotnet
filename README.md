# DevJobAlerter 🚀

O **DevJobAlerter** é um serviço em segundo plano (Worker Service) construído em **.NET 10** que busca vagas de desenvolvimento de software em APIs externas (como a Adzuna) e envia alertas em tempo real diretamente para o WhatsApp utilizando a Evolution API e ambiente Docker.

---

## 🏗️ Arquitetura do Projeto

O ecossistema está estruturado seguindo os princípios de **Clean Architecture**, dividido em três camadas principais:

| Camada | Descrição | Componentes Principais |
| :--- | :--- | :--- |
| `DevJobAlerter.Domain` | Contém as regras de negócio, entidades e contratos fundamentais do sistema. | `JobVacancy`, `SentJob`, `IJobService`, `INotificationService`, `IJobRepository` |
| `DevJobAlerter.Infrastructure` | Implementação das integrações externas, banco de dados e serviços de infraestrutura. | `AdzunaJobService`, `ApiWhatsAppNotificationService`, `AppDbContext`, `JobRepository` |
| `DevJobAlerter.Worker` | O ponto de entrada da aplicação que orquestra o ciclo de monitoramento e execução. | `Program.cs`, `Worker.cs`, `JobSearchSettings` |

---

## 🛠️ Tecnologias e Ferramentas

- **.NET 10 (Worker Service)**: Motor de execução e orquestração do serviço em segundo plano.
- **Entity Framework Core & SQLite**: Armazenamento persistente das vagas enviadas para evitar notificações duplicadas.
- **Evolution API (Serviço de WhatsApp)**: API REST para envio automatizado de mensagens no WhatsApp.
- **PostgreSQL**: Banco de dados para persistência de estado da Evolution API.
- **Docker & Docker Compose**: Containerização que garante a comunicação isolada entre os serviços.

---

## 🔄 Fluxo de Execução

1. O **Worker Service** é executado periodicamente com base nos termos de busca configurados.
2. O **Adzuna Job Service** consulta novas oportunidades de trabalho na API externa.
3. O **Repositório SQLite** verifica se a URL da vaga já foi enviada anteriormente.
4. O **Serviço de Notificação** envia os detalhes da vaga para o WhatsApp através da **Evolution API**.
5. As novas vagas enviadas são salvas no SQLite para impedir alertas repetidos.

---

## 🚀 Como Executar o Projeto

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução.
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (opcional, apenas para desenvolvimento local fora do Docker).

### Configuração

1. Clone o repositório:
   ```bash
   git clone [https://github.com/seu-usuario/DevJobAlerter.git](https://github.com/seu-usuario/DevJobAlerter.git)
   cd DevJobAlerter