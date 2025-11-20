# FiapSrvAuthManager - API de Autenticação e Registro

## 📖 Sobre o Projeto

**FiapSrvAuthManager** é um microserviço de autenticação e registro de usuários, desenvolvido em .NET 8. Ele serve como a porta de entrada para a plataforma de jogos, gerenciando o cadastro e o login de usuários com os papéis de *Player* e *Publisher*.

O projeto adota os princípios da arquitetura limpa, o que promove um código organizado, de baixo acoplamento e alta testabilidade, separando as responsabilidades em camadas bem definidas: Domínio, Aplicação, Infraestrutura e API.

## ✨ Funcionalidades Principais

  - **Registro de Usuários**: Endpoints para registrar novos *Players* e *Publishers* com validações de dados específicas para cada perfil.
  - **Autenticação**: Endpoint de login que valida as credenciais do usuário e retorna um JSON Web Token (JWT) para acesso aos demais serviços.
  - **Segurança de Senhas**: Utilização do algoritmo **BCrypt** para armazenar o hash das senhas, garantindo que elas não sejam guardadas em texto plano.
  - **Geração de Token JWT**: Criação de tokens seguros contendo as claims do usuário (ID e Role), assinados com uma chave simétrica.
  - **Integração com AWS**: Utiliza serviços da AWS para aumentar a segurança e a escalabilidade da aplicação em ambiente de produção.
  - **Estrutura de Microserviço**: Desenvolvido para operar de forma independente, focando exclusivamente na gestão de identidade e acesso.

## 🚀 Tecnologias Utilizadas

  - **.NET 8**: Framework principal para o desenvolvimento da API.
  - **ASP.NET Core**: Para a construção da API RESTful.
  - **MongoDB**: Banco de dados NoSQL para persistência dos dados de usuários.
  - **JWT (JSON Web Tokens)**: Para a geração de tokens de autenticação.
  - **BCrypt.Net-Next**: Biblioteca para hashing de senhas.
  - **AWS (Amazon Web Services)**:
      - **Parameter Store**: Para armazenar e gerenciar `secrets` como a chave de assinatura do JWT e a connection string do banco de dados em produção.
      - **S3 (Simple Storage Service)**: Utilizado para persistir as chaves de criptografia do Data Protection, garantindo a consistência entre múltiplas instâncias da aplicação.
      - **ECS (Elastic Container Service)**: Onde a aplicação é implantada e orquestrada em contêineres.
  - **Docker**: Para criar os contêineres da aplicação.
  - **Serilog**: Para logging estruturado, facilitando a monitoria e o debug.
  - **Swagger (OpenAPI)**: Para documentação interativa e testes dos endpoints da API.
  - **xUnit & Moq**: Para a escrita de testes unitários.

## 🏗️ Arquitetura

O projeto segue uma arquitetura limpa, dividida em quatro camadas principais:

  - **`FiapSrvAuthManager.Domain`**: O núcleo da aplicação, contendo as entidades de negócio (`User`, `Player`, `Publisher`) e os enums. Não possui dependências de outras camadas do projeto.
  - **`FiapSrvAuthManager.Application`**: Contém a lógica de negócio, DTOs, interfaces de serviços (`IAuthService`), e as implementações que orquestram as regras de autenticação e registro.
  - **`FiapSrvAuthManager.Infrastructure`**: Implementa as interfaces da camada de aplicação, lidando com preocupações externas como acesso ao banco de dados (repositório MongoDB), middlewares e configurações de serviços externos.
  - **`FiapSrvAuthManager.API`**: A camada de apresentação, que expõe os endpoints RESTful para o mundo exterior, recebe as requisições HTTP e retorna as respostas adequadas.

## ⚙️ CI/CD - Integração e Implantação Contínua

O projeto possui um pipeline de CI/CD totalmente automatizado com **GitHub Actions**, que cobre desde a compilação até o deploy em produção.

1.  **Orquestrador (`ci-cd.yml`)**: Este workflow é o ponto de partida, sendo acionado em pushes ou merges na branch `main`.
2.  **CI (`ci.yml`)**:
      - Executa o build da solução .NET.
      - Roda todos os testes unitários e calcula a cobertura de código.
      - Envia os resultados para o **SonarCloud** para uma análise estática de código, garantindo a qualidade e segurança.
3.  **CD (`cd.yml`)**:
      - Após a aprovação da etapa de CI, o workflow de CD é iniciado.
      - Realiza a autenticação no Docker Hub.
      - Constrói a imagem Docker da aplicação.
      - Publica a imagem no repositório do **Docker Hub**.
4.  **Deploy (`deploy-aws.yml`)**:
      - Com a nova imagem disponível, este workflow faz o deploy no ambiente da **AWS**.
      - Ele atualiza a definição da tarefa (task definition) no **AWS ECS** com a nova imagem e implanta a versão mais recente do serviço, sem causar indisponibilidade.

## Endpoints da API

Abaixo estão os endpoints públicos da API de autenticação.

### Auth (`/api/auth`)

  - `POST /register/player`: Registra um novo usuário com o perfil de `Player`.
  - `POST /register/publisher`: Registra um novo usuário com o perfil de `Publisher`.
  - `POST /authenticate`: Autentica um usuário existente (player ou publisher) e retorna um token JWT em caso de sucesso.

## 🏁 Como Executar Localmente

### Pré-requisitos

  - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  - [Docker Desktop](https://www.docker.com/products/docker-desktop)
  - Um editor de código de sua preferência (ex: VS Code, Visual Studio).

### 1\. Configuração do Ambiente

1.  **Clone o repositório:**

    ```bash
    git clone https://github.com/jpedroduarte23/fiap-srv-auth-manager.git
    cd fiap-srv-auth-manager
    ```

2.  **Inicie o MongoDB com Docker:**

    ```bash
    docker run -d -p 27017:27017 --name mongo mongo:latest
    ```

### 2\. Configuração da Aplicação

1.  **Configure a Connection String**:
    No arquivo `FiapSrvAuthManager.API/appsettings.Development.json`, verifique se a connection string do MongoDB está configurada corretamente:

    ```json
    "ConnectionStrings": {
      "MongoDbConnection": "mongodb://localhost:27017/"
    }
    ```

2.  **Restaure as dependências e execute a aplicação**:
    Navegue até a pasta raiz do projeto e execute o seguinte comando:

    ```bash
    dotnet run --project FiapSrvAuthManager.API/FiapSrvAuthManager.API.csproj
    ```

3.  **Acesse a API**:
    A aplicação estará disponível em `https://localhost:7208` ou `http://localhost:5271`.
    A documentação do Swagger pode ser acessada através da URL `https://localhost:7208/swagger`.
