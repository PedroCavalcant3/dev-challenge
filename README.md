
# Desafio Umbler

Esta é uma aplicação web que recebe um domínio e mostra suas informações de DNS.

Este é um exemplo real de sistema que utilizamos na Umbler.

Ex: Consultar os dados de registro do dominio `umbler.com`

**Retorno:**
- Name servers (ns254.umbler.com)
- IP do registro A (177.55.66.99)
- Empresa que está hospedado (Umbler)

Essas informações são descobertas através de consultas nos servidores DNS e de WHOIS.

*Obs: WHOIS (pronuncia-se "ruís") é um protocolo específico para consultar informações de contato e DNS de domínios na internet.*

Nesta aplicação, os dados obtidos são salvos em um banco de dados, evitando uma segunda consulta desnecessaria, caso seu TTL ainda não tenha expirado.

*Obs: O TTL é um valor em um registro DNS que determina o número de segundos antes que alterações subsequentes no registro sejam efetuadas. Ou seja, usamos este valor para determinar quando uma informação está velha e deve ser renovada.*

Tecnologias Backend utilizadas:

- C#
- Asp.Net Core
- MySQL
- Entity Framework

Tecnologias Frontend utilizadas:

- Webpack
- Babel
- ES7

Para rodar o projeto você vai precisar instalar:

- dotnet Core SDK (https://www.microsoft.com/net/download/windows dotnet Core 6.0.201 SDK)
- Um editor de código, acoselhamos o Visual Studio ou VisualStudio Code. (https://code.visualstudio.com/)
- NodeJs v17.6.0 para "buildar" o FrontEnd (https://nodejs.org/en/)
- Um banco de dados MySQL (vc pode rodar localmente ou criar um site PHP gratuitamente no app da Umbler https://app.umbler.com/ que lhe oferece o banco Mysql adicionamente)

Com as ferramentas devidamente instaladas, basta executar os seguintes comandos:

Para "buildar" o javascript basta executar:

`npm install`
`npm run build`

Para Rodar o projeto:

Execute a migration no banco mysql:

`dotnet tool update --global dotnet-ef`
`dotnet tool ef database update`

E após: 

`dotnet run` (ou clique em "play" no editor do vscode)

# Objetivos:

Se você rodar o projeto e testar um domínio, verá que ele já está funcionando. Porém, queremos melhorar varios pontos deste projeto:

# FrontEnd

 - Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.
 - Não há validação no frontend permitindo que seja submetido uma requsição inválida para o servidor (por exemplo, um domínio sem extensão).
 - Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack. O ideal seria utilizar algum framework mais moderno como ReactJs ou Blazor.  

# BackEnd

 - Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).
 - A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.
 - O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente. Retornar uma ViewModel (DTO) neste caso seria mais aconselhado.

# Testes

 - A cobertura de testes unitários está muito baixa, e o DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura.
 - O Banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não. 

# Dica

- Este teste não tem "pegadinha", é algo pensado para ser simples. Aconselhamos a ler o código, e inclusive algumas dicas textuais deixadas nos testes unitários. 
- Há um teste unitário que está comentado, que obrigatoriamente tem que passar.
- Diferencial: criar mais testes.

# Entrega

- Enviei o link do seu repositório com o código atualizado.
- O repositório deve estar público para que possamos acessar..
- Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

# Modificações:

### 1. Arquitetura
O projeto foi reestruturado adotando uma **Clean Architecture Simplificada**.
Optei por não utilizar o *Repository Pattern* pois não havia necessidade. O acesso a dados é feito diretamente pelos Serviços de Aplicação, mantendo a separação de responsabilidades e reduzindo drasticamente a complexidade ciclomática.

Essa modificação resultou em um código altamente organizado, desacoplado e testável.

### Nova Estrutura da Solução

Divisão utilizando Class Library, boa pratica que evita acumalar packages e referenciar projetos sem necessidade:
* **`Desafio.Umbler.Domain` (Class Library)**
  * Contém a Entidade do negócio (`DomainInfo`).
* **`Desafio.Umbler.Application` (Class Library)**
  * Regras de Negócio de Aplicação e inteligência `DomainService`), DTOs, Interfaces e Exceções de negócio.
* **`Desafio.Umbler.Infrastructure` (Class Library)**
  * Implementações concretas. Inclui o `DatabaseContext`, acesso a dados(Get, Add, Save) (`DataAccess`), Migrations e Serviços Externos (`DnsService`, `WhoisService`).
* **`Desafio.Umbler.Web` (Blazor Server App)**
  * Camada de apresentação híbrida:
    * **API:** DomainController para consumo externo com os testes.
    * **UI:** Blazor Server Pages, requisitando o DomainService diretamente para maior eficiência.
* **`Desafio.Umbler.Tests` **
  * 9 Testes unitários(3 novos).

---

## 2. DataAccess e ExternalServices
As consultas de **DNS** e **WHOIS** e os comportamentos da applicação foram extraídas para serviços dedicados e abstraídas por interfaces (`DnsService`, `WhoisService`, `DomainDataAccess`).

**Benefícios:**
* **Desacoplamento:** A lógica de negócio não sabe qual biblioteca está sendo usada para buscar o DNS.
* **Testabilidade:** Permite o uso de **Mocks** (via biblioteca *Moq*) para simular cenários de rede, timeouts e falhas.

---

## 3. Testes Unitários
Os testes que ja existiam foram refatorados com base na nova arquitetura, toda a lógica foi preservada.

* **3 novos testes**
  1. **Domínio Inválido:** Deve lançar exceção de negócio.
  2. **Cache Válido:** Se o domínio existe e o TTL não expirou, **não** deve chamar serviços externos.
  3. **Cache Expirado:** Se o TTL expirou, **deve** chamar serviços externos para atualização (Refresh).
* **Ferramentas:** Uso de `Moq` para isolar dependências externas.

---

## 4. Controller
Por conta da reestruturação, houve uma redução drástica na complexidade e acoplamento do DomainController tornando ele muito eficiente e testavel.
O Controller agora apenas:
* Valida a entrada básica.
* Requisita o `IDomainService`.
* Traduz exceções de domínio para Status Codes HTTP (400, 500, 503).

---

## 5. DTOs
O backend deixou de expor a entidade de banco de dados diretamente. Foi introduzido o `DomainInfoDto` para:
* Segurança e performance: Oculta dados os internos `Id`, `UpdatedAt` e '`TTL`', trazendo apenas os dados necessários para a visualização.

---

## 6. Tratamento de Erros
Implementação de um fluxo robusto de exceções mapeadas para respostas HTTP adequadas:

| Exceção | Status HTTP | Significado |
| :--- | :--- | :--- |
| `InvalidDomainException` | **400 Bad Request** | Erro de validação do usuário. |
| `ExternalLookupException`, `DnsResponseException`, `OperationCanceledException` | **503 Service Unavailable** | Falha ou timeout em APIs externas (DNS/WHOIS). |
| `Exception` (Genérica) | **500 Internal Server Error** | Erro inesperado (sem vazar stack trace). |

---

## 7. Frontend (Blazor Server)
Utilização do framework **Blazor Server**, permitindo uma interface reativa e moderna.

### Fluxo Otimizado
O componente Blazor permitiu requisição direta ao serviço, sem precisar passar pelo Controller. Mantivemos o DomainController na raiz do projeto web para ser usado como API e consumida pelos testes unitários.
Com ele, melhoraramos facilmente o fluxo do sistema:
* **Antigo:** `JS/View` → `HTTP Request` → `Controller` → `Service` → Infra / DNS / WHOIS / DB
* **Novo:** `Blazor Page` → `requisição IDomainService` → `Service` → Infra / DNS / WHOIS / DB

### Melhorias de UX/UI
* **Validação Client-Side:** Regex instantâneo impede o envio de qualquer formato/caracter inválidos.
* Indicador de carregamento e mensagens de erro evidentes.
* **Design:** Formatação e disposição dos dados de forma organizada e legível com um básico BootStrap.

---

## 8. Configuração e Inicialização (Desafio.Umbler.Wev > `Program.cs`)

O arquivo `Program.cs` foi modificado para atuar como o ponto central de composição da aplicação, aplicando a injeção de dependências e a conexão com o banco de dados.

### Adaptações Realizadas:

* **Banco de Dados (EF Core):**
    Configuração do `DatabaseContext` utilizando o provider `Pomelo.EntityFrameworkCore.MySql`. Foi definido explicitamente a versão do servidor e, crucialmente, configurado o `MigrationsAssembly` para apontar para o projeto `Infrastructure`, garantindo que as migrações sejam localizadas corretamente fora do projeto Web.

* **Injeção de Dependência:**
    Todos os serviços e interfaces foram registrados no Program.cs, garantindo o isolamento de dados entre usuários:
    * `IDomainService` → `DomainService`
    * `IDnsService` → `DnsService`
    * `IWhoisService` → `WhoisService`
    * `IDomainDataAccess` → `DomainDataAccess`

* **Cliente DNS:**
    O `ILookupClient` (DnsClient) precisou ser registrado como Singleton para que funcionasse e garante otimização.
    
   ### Observações pessoais:
   Tive um timeout de DNS por conta do meu ambiente local (IPv6/DNS), pesquisando, foi-me recomendado forçar DNS público para estabilizar.
    Mas, a melhor solução foi modificar o parâmetro da função DnsClient.QueryType.ANY para 'DnsClient.QueryType.A na função GetARecordAsync da classe DnsService.cs, Essa modificação 
    resulta em uma consulta DNS mais rápida e específica, reduzindo a probabilidade de timeouts.

    Identifiquei que a versão original do projeto tratava o TTL do DNS em minutos(TotalMinutes), o que é errado pois o TTL é definido em segundos como informado no tópico retorno desse Readme. No DomainController o calculo ocorria desssa forma:
    DateTime.Now.Subtract(domain.UpdatedAt).**TotalMinutes** > domain.Ttl
    Corrigi, ficando assim: (DateTime.UtcNow - domain.UpdatedAt).**TotalSeconds** > domain.Tt
    Garantindo que o cache expire no momento exato determinado pela autoridade de DNS.

    Uma das dependências do projeto web inicial era o NodeJs v17.6.0 para  "buildar" o FrontEnd, mas essa versão não existe mais e, não está disponivel para download. Assim, testando com qualquer versão superior
    o front simplismente não builda, e por algum motivo particular com a versão anterior mais próxima tambem não.

---

## Como Rodar o projeto no (Visual Studio)
* Certifique-se de estar na branch **`challenge-sollution`**
* O projeto Razor Web 'Desafio.Umbler.Web' deve ser setado como StartupProject.

### Via Terminal/PowerShell:
* Certifique-se de estar com o **`.NET 6.0 SDK`** instalado: clique com o botão direito na Solution(raiz do projeto) > abrir com o terminal, utilize o comando: **`dotnet ef --version`**. Caso você esteja com uma versão maior que 6, recomendo que desinstale, usando: **`dotnet tool uninstall --global dotnet-ef`** e instale a seguinte versão: **`dotnet tool install --global dotnet-ef --version 6.0.0`**. Após isso, verifique a versão instalada com: **`dotnet ef --version`**, caso esteja correta, use: **`dotnet restore`** para baixar todas as dependências do projeto e, finalmente, execute o comando para rodar as migrations: **`dotnet ef database update --project Desafio.Umbler.Infrastructure --startup-project Desafio.Umbler.Web`**.

### Via Package Manager Console (Desafio.Umbler.Infraestructure):
* Para rodar as migrations, abra o Package Manager Console, selecione como Default Project: `Desafio.Umbler.Infraestructure`, execute o comando `dotnet restore` para baixar todas as dependências do projeto e, por fim dê o comando `update-database`.
---
* Após rodar as migrations, de o comando `dotnet run` no Terminal/PowerShell do projeto Desafio.Umbler.Web (ou clique em "play" no editor do Visual Studio).
* String de conexão referente ao MySql oferecido pelo site PHP gratuito no app da Umbler https://app.umbler.com/ que oferece o banco Mysql adicionamente), o meu banco permanecerá ligado até 05/02/26.




