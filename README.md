# DevJobAlerter 🚀📱

**DevJobAlerter** é um serviço em segundo plano (Worker Service) desenvolvido em .NET 10 que monitora o mercado de trabalho em tempo real, captura vagas de tecnologia e envia alertas formatados diretamente para o seu WhatsApp.

O projeto foi construído seguindo boas práticas de arquitetura de software, garantindo separação de conceitos, resiliência e facilidade de manutenção.

---

## 🛠️ Funcionalidades

- **Busca Automatizada de Vagas:** Integração direta com a API da Adzuna para buscar oportunidades no mercado brasileiro.
- **Alertas no WhatsApp:** Disparo de notificações estruturadas via Twilio Sandbox.
- **Filtros Dinâmicos por Terminal:** Permite que o usuário informe palavras-chave personalizadas diretamente no comando de execução (ex: `junior .net`, `estagio c#`).
- **Resiliência a Falhas:** Tratamento robusto de codificação de dados brutos (bypassing de encodings malformados da API externa).

---

## 🏗️ Arquitetura do Projeto

O ecossistema está dividido em três camadas principais:

| Camada | Descrição | Componentes Principais |
| :--- | :--- | :--- |
| **`DevJobAlerter.Domain`** | Contém as regras de negócio, entidades e contratos fundamentais do sistema. | `JobVacancy`, `IJobService`, `INotificationService` |
| **`DevJobAlerter.Infrastructure`** | Implementação das integrações externas e serviços de infraestrutura. | `AdzunaJobService`, `WhatsAppNotificationService` |
| **`DevJobAlerter.Worker`** | O ponto de entrada da aplicação que orquestra o ciclo de monitoramento por hora. | `Program.cs`, `Worker.cs` |

---

## 🔑 Configuração

Antes de rodar a aplicação, você precisa configurar as suas chaves de acesso no arquivo `appsettings.json` localizado no projeto **DevJobAlerter.Worker**:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AdzunaApi": {
    "AppId": "SEU_APP_ID_AQUI",
    "AppKey": "SUA_APP_KEY_AQUI"
  },
  "Twilio": {
    "AccountSid": "SEU_ACCOUNT_SID_AQUI",
    "AuthToken": "SEU_AUTH_TOKEN_AQUI",
    "FromPhoneNumber": "whatsapp:+14155238886"
  }
}