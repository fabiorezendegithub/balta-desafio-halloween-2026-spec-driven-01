# Histórico de alterações

Registro das alterações realizadas no projeto, organizado na ordem dos commits.

## `1194afe` — Iniciando repo

- Criou a estrutura inicial do repositório.
- Adicionou o `.gitignore`.
- Criou os documentos iniciais de constituição, especificação, plano e tarefas em `specs/`.
- Criou este arquivo para orientar o conteúdo da pasta `src/`.

## `c3bdb8e` — Atualizando o readme

- Criou o `readme.md` na raiz do projeto.
- Adicionou a apresentação inicial do desafio Halloween 2026 e do projeto.

## `c354703` — Add files via upload

- Adicionou o arquivo `desafio.md`.
- Registrou o enunciado e o contexto original do desafio.

## `0a28d2d` — feito constitution e spec

- Detalhou a constituição do projeto:
  - stack .NET, C#, Minimal API, SQLite, Entity Framework Core e xUnit;
  - separação entre endpoints, aplicação e persistência;
  - exigências de testes e tratamento de erros;
  - convenções de código, mensagens e commits.
- Expandiu a especificação funcional:
  - objetivo, usuários e histórias;
  - requisitos para geração, consulta, unicidade e expiração;
  - regras de negócio, casos de borda e critérios de aceite.

## `b5168d3` — Planejar e inicializar projeto: plano técnico, tarefas e estrutura .NET

- Atualizou o plano técnico com arquitetura, dependências, decisões de design,
  modelo de dados, contratos da API, estratégia de testes e riscos aceitos.
- Refinou a especificação, removendo expiração do escopo e definindo:
  - senha com exatamente 16 caracteres;
  - header `X-Guid` para consulta;
  - erros `400`, `404` e `500` com `ProblemDetails`;
  - persistência permanente e consultas repetíveis.
- Criou 10 tarefas rastreáveis em `specs/tasks.md`.
- Criou a solução .NET 10 `src/PasswordGenerator.slnx`.
- Implementou a Minimal API de geração e consulta de senhas.
- Implementou o serviço de aplicação, gerador de senha forte e geração de GUID.
- Implementou persistência SQLite com Entity Framework Core e migration inicial.
- Implementou tratamento global de exceções.
- Criou testes unitários e de integração com xUnit para aplicação, API e banco.

## `b31db1f` — atualizado documentacao

- Adicionou o banco SQLite `src/PasswordGenerator/passwords.db` ao projeto.

## `a54c713` — atualizado documentacoes

- Atualizou o `readme.md` principal com:
  - funcionalidades implementadas;
  - stack utilizada;
  - instruções de execução e testes;
  - endpoints, exemplos de requisição e respostas;
  - links para a documentação do projeto.
- Criou `docs/como-iniciar-e-testar.md` com instruções detalhadas para iniciar a
  API, testar os endpoints e validar respostas de erro.
