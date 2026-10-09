# Tarefas

Toda regra de negócio tem teste unitário e todo endpoint tem ao menos um teste
de integração de caminho feliz (constituição). A coluna "Concluída quando" lista
o critério principal, e esses testes são exigidos além dele.

Commits seguem `tipo(escopo): descrição` com o identificador do requisito
(coluna "Origem").

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-01 | Criar projeto .NET 10 Minimal API, estrutura de pastas e projeto de testes xUnit (com `Microsoft.AspNetCore.Mvc.Testing`) | Constituição (Stack, Arquitetura, Qualidade), plano (Dependências) | | Projeto compila e um teste vazio roda |
| T-02 | Modelar `PasswordEntry` (Id, Guid obrigatório e único, Password obrigatório e único), DbContext SQLite/EF Core, índices únicos em Guid e Password e migration | D-01, D-03, RF-04, RF-05, RN-01, RN-02, RN-04 | T-01 | Migration aplica no SQLite; inserir Guid ou Password duplicado falha por violação do índice; inserir Password nulo falha |
| T-03 | Implementar gerador de senha forte (exatamente 16 caracteres, pelo menos 1 especial da lista permitida, 1 maiúscula, 1 minúscula e 1 número, sem espaço em branco), com gerador aleatório criptográfico | RF-02, D-08 | T-01 | Testes unitários provam cada regra (tamanho 16, cada categoria, só especiais da lista, sem espaço) em muitas gerações |
| T-04 | Tratamento global de erros: erro de validação vira 400 e falha interna vira 500, ambos com ProblemDetails e mensagens em português, sem expor detalhe interno; sem 401 | Constituição (Qualidade, Convenções), D-04, D-05, RN-03 | T-01 | Teste unitário do tratador confirma 400 e 500 com ProblemDetails e mensagem em português (os testes de integração por endpoint ficam em T-07 e T-08) |
| T-05 | `PasswordService.Create`: gerar senha e GUID, persistir de forma atômica e devolver somente o GUID; qualquer falha (colisão, indisponibilidade) propaga erro interno sem retry e sem criar nem reservar GUID | RF-01, RF-05, RN-01, RN-02, RN-03, D-02, D-03, D-05 | T-02, T-03 | Testes unitários: cada criação gera GUID e senha novos e vinculados, a senha não é devolvida; com colisão ou falha de persistência simulada, ocorre erro interno e nenhum registro nem GUID é criado |
| T-06 | `PasswordService.Get`: receber o valor bruto do header, validar (ausente, vazio ou fora do formato padrão com hífens, via `TryParseExact` "D"), devolver a senha pelo GUID de forma repetível e tratar GUID inexistente | RF-03, RF-06, RF-07, D-06, D-09 | T-02 | Testes unitários: GUID existente devolve a mesma senha em consultas repetidas; GUID bem formado inexistente tem resultado próprio; valor ausente, vazio, sem hífens, com chaves ou malformado é recusado como erro de validação |
| T-07 | Endpoint `POST /api/password` (sem corpo; saída 201 com `{ guid }`) chamando só o serviço; falha interna vira 500 ProblemDetails | RF-01, RN-03, D-02, D-05, Constituição (Arquitetura) | T-04, T-05 | Teste de integração de caminho feliz: 201 com GUID e sem senha na resposta; teste de integração com serviço falhando: 500 ProblemDetails em português e nenhum GUID criado |
| T-08 | Endpoint `GET /api/password` repassando o header `X-Guid` bruto ao serviço, mapeando 200 e 404 (inexistente); resultado de validação (ausente, vazio ou GUID malformado) vira 400; resposta com `Cache-Control: no-store` | RF-03, RF-06, RF-07, D-04, D-06, D-07 | T-04, T-06 | Teste de integração de caminho feliz e testes de integração dos casos inexistente, header ausente, header vazio, GUID malformado, GUID sem hífens e GUID com chaves; 404 distinto de 400, com mensagem em português informando que o GUID não existe |
| T-09 | Teste de integração de persistência: criar, reiniciar a aplicação e consultar | RF-04, critérios de aceite | T-07, T-08 | Senha criada antes do reinício continua consultável |
| T-10 | Conferir os critérios de aceite um a um com testes de integração (GUIDs e senhas únicas, formato da senha, consulta repetida) | Critérios de aceite, RN-01, RN-02 | T-07, T-08 | Cada critério da spec tem ao menos um teste verde |
