# DevJobAlerter 🚀📱

**DevJobAlerter** é um serviço em segundo plano (Worker Service) desenvolvido em **.NET 10** e containerizado com **Docker** que monitora o mercado de trabalho, captura vagas de tecnologia e envia alertas instantâneos diretamente para o seu **WhatsApp**.

O projeto foi construído seguindo boas práticas de arquitetura de software, garantindo separação de conceitos, resiliência, persistência de dados com **EF Core + SQLite** (evitando avisos duplicados) e facilidade de implantação.

---

## 🛠️ Funcionalidades

- **Busca Automatizada de Vagas:** Integração com a API da **Adzuna** para buscar oportunidades no mercado.
- **Alertas no WhatsApp:** Disparo de notificações estruturadas via **Twilio API**.
- **Deduplicação Inteligente:** Armazenamento em banco de dados **SQLite** via **Entity Framework Core** para evitar alertas repetidos.
- **Containerização Total:** Pronto para rodar via **Docker** em qualquer ambiente sem necessidade de setup local do .NET.
- **Filtros Dinâmicos:** Suporte a múltiplos termos de busca para desenvolvedores (ex: `.NET Júnior`, `C# Júnior`).

---

## 🏗️ Arquitetura do Projeto

O ecossistema está dividido em três camadas principais:

| Camada | Descrição | Componentes Principais |
| :--- | :--- | :--- |
| **`DevJobAlerter.Domain`** | Contém as regras de negócio, entidades e contratos fundamentais do sistema. | `JobVacancy`, `SentJob`, `IJobService`, `INotificationService` |
| **`DevJobAlerter.Infrastructure`** | Implementação das integrações externas, banco de dados e serviços de infraestrutura. | `AdzunaJobService`, `WhatsAppNotificationService`, `AppDbContext` |
| **`DevJobAlerter.Worker`** | O ponto de entrada da aplicação que orquestra o ciclo de monitoramento e execução. | `Program.cs`, `Worker.cs` |

---

## 📋 Pré-requisitos

Antes de rodar a aplicação, certifique-se de ter instalado/configurado:

* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* Uma conta de desenvolvedor na [Adzuna](https://developer.adzuna.com/)
* Uma conta no [Twilio](https://www.twilio.com/) com a Sandbox do WhatsApp ativa

---

## 🔑 Configuração das Variáveis de Ambiente

Crie um arquivo `.env` na raiz do projeto contendo suas credenciais de acesso (utilize o modelo abaixo):

```env
# Adzuna API
Adzuna__AppId=SEU_ADZUNA_APP_ID
Adzuna__AppKey=SUA_ADZUNA_APP_KEY

# Twilio API
Twilio__AccountSid=SEU_TWILIO_ACCOUNT_SID
Twilio__AuthToken=SEU_TWILIO_AUTH_TOKEN
Twilio__FromPhoneNumber=whatsapp:+14155238886
Twilio__ToPhoneNumber=whatsapp:+5511999999999