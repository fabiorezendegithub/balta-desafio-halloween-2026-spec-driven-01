# Plano técnico

## Contexto
Implementa a especificação do gerador de senhas fortes, respeitando a constituição
do projeto.

## Arquitetura
- API HTTP em Minimal API, um arquivo de endpoints por recurso
- Camada de aplicação com PasswordService, dona das regras RN-01 a RN-04
- Camada de dados com EF Core (DbContext) acessada apenas pelo PasswordService
- Endpoint só traduz a requisição em chamada ao serviço e o resultado em
  resposta HTTP. Toda validação de negócio (unicidade, existência do GUID e
  validação do valor do header `X-Guid`: ausente, vazio ou GUID malformado, RF-06)
  vive no PasswordService. O endpoint repassa o valor bruto do header ao serviço e
  apenas mapeia o resultado para 400 ou 404
- Sem interface web: fora de escopo na spec, que define consumo apenas por API

## Dependências
Nenhuma dependência nova além da stack da constituição (.NET 10, EF Core com
SQLite, xUnit). Pacotes de apoio exigidos pela própria stack, com justificativa:
- `Microsoft.EntityFrameworkCore.Design` (e ferramenta `dotnet-ef`): necessário
  para gerar e aplicar a migration do EF Core (T-02), só em tempo de desenvolvimento
- `Microsoft.AspNetCore.Mvc.Testing`: necessário para os testes de integração de
  endpoint exigidos pela constituição (Qualidade), sem subir servidor externo

## Decisões
- D-01: SQLite como armazenamento, porque o RF-04 exige persistência
  entre reinícios
- D-02: a senha é gerada automaticamente, sem dado de entrada, nunca é devolvida
  na criação e fica vinculada a um GUID. Quem cria recebe apenas o GUID (RF-01)
- D-03: unicidade da senha e do GUID garantida por índice único no banco, além da
  checagem no serviço, porque a checagem sozinha tem corrida. Colisão detectada
  pela checagem ou pelo índice não tem nova tentativa: a criação falha com erro
  interno (D-05), como aceito na spec
- D-04: política de erros. Erro de validação (header ausente, vazio ou GUID
  malformado) retorna 400 com ProblemDetails e mensagem em português. Não existe
  401, pois a spec não tem autenticação. As respostas do GET são: 400 header
  ausente, vazio ou GUID malformado, e 404 GUID inexistente, ambas com
  ProblemDetails e mensagem distinta em português
- D-05: falha interna na criação (RN-03). Qualquer falha na criação (colisão de
  GUID ou senha, indisponibilidade do banco) retorna 500 com ProblemDetails e
  mensagem em português, sem expor detalhe interno. A gravação de GUID e senha é
  uma única operação atômica, então a falha não cria nem reserva GUID
- D-06: vocabulário. "GUID malformado" é erro de formato (400, RF-06). "GUID
  inexistente" é o GUID bem formado que não existe (404, RF-07). Não existe GUID
  sem senha (RN-04), então não há estado "GUID inválido". O formato aceito é
  somente o padrão com hífens (`Guid.TryParseExact` com formato "D"): valores sem
  hífens, com chaves ou parênteses são malformados e retornam 400
- D-07: o GUID é recebido no header `X-Guid` (RF-06), e não na URL, para não
  aparecer em histórico, logs de acesso ou referer. A resposta do GET usa
  `Cache-Control: no-store`
- D-08: gerador de senha. Tamanho fixo de 16 caracteres, a partir dos conjuntos
  maiúsculas, minúsculas, números e especiais `! @ # $ % ^ & * ( ) - _ = + [ ] { }`
  (RF-02). Garante pelo menos 1 de cada conjunto, preenche o restante com o
  alfabeto completo e embaralha, usando o gerador aleatório criptográfico da
  biblioteca base do .NET (sem dependência nova). Nunca gera espaço em branco
- D-09: a consulta é de leitura, ilimitada e repetível (RF-03). Cada GET devolve a
  mesma senha e não altera estado

## Modelo de dados
PasswordEntry (tabela `Passwords`; o nome da entidade difere da propriedade
`Password` porque em C# um membro não pode ter o nome do tipo que o contém)
- Id, Guid (único, obrigatório), Password (único, obrigatório)
- Password obrigatório (NOT NULL): o banco impede GUID sem senha vinculada (RN-04)
- Registros nunca são apagados, então GUID e senha continuam ocupados (RN-01, RN-02)

## Contratos
- POST /api/password
  - Sem corpo
  - Saída 201: `{ "guid": "..." }`. A senha não é devolvida
  - 500: falha interna (ex.: colisão ou indisponibilidade), ProblemDetails com
    mensagem em português; nenhum GUID é criado ou reservado
- GET /api/password
  - Header `X-Guid: <guid>`
  - 200: `{ "password": "..." }`
  - 400: header ausente, vazio ou GUID malformado (formato padrão com hífens)
  - 404: GUID inexistente (ProblemDetails, mensagem em português informando que
    o GUID não existe)

## Estratégia de testes
- Teste unitário para cada regra de negócio (RN-01 a RN-04, incluindo a falha de
  criação sem GUID reservado), para a validação do header (RF-06), para o GUID
  inexistente (RF-07) e para o gerador de senha (RF-02)
- Teste de integração de caminho feliz para cada endpoint, mais os casos de
  erro distintos do GET (incluindo GUID sem hífens e com chaves), a falha interna
  da criação (500), a consulta repetida e a persistência após reinício (RF-04)

## Riscos
- Concorrência na criação de guid e password, mitigada por D-03
- Crescimento da tabela de password sem política de limpeza, nem limite de criação
  ou proteção contra abuso: pendência assumida e fora de escopo na spec
- GUID criado por engano permanece ativo indefinidamente (pendência assumida na spec)
- Riscos aceitos na spec: o GUID é segredo de portador (quem o tem lê a senha,
  sem limite de leituras), a senha fica armazenada de forma legível e permanente, e
  colisão de senha ou GUID na geração não tem nova tentativa (falha interna, coberta
  por D-03)
