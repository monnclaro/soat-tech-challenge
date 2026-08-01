# ADR 0002 — Comunicação Síncrona via API Gateway (HTTP_PROXY), Não Assíncrona

**Status:** Aceito

## Contexto

O API Gateway precisa encaminhar chamadas autenticadas (`/api/{proxy+}`) para a aplicação principal, que roda no EKS exposta via `Service type=NodePort` (sem ALB — ver [ADR 0008](./0008-prioridade-de-custo-aws-academy.md)).

## Decisão

Integração `HTTP_PROXY` síncrona: o API Gateway repassa a requisição HTTP diretamente para o IP público do node do EKS (porta NodePort) e devolve a resposta ao cliente na mesma conexão. Nenhuma fila (SQS/EventBridge) foi introduzida entre o Gateway e a aplicação.

## Alternativas descartadas

- **Fila assíncrona (SQS) entre Gateway e app**: adequada para processamento em lote ou operações longas, mas as rotas protegidas por este Gateway são consultas/mutações request-response simples (ex.: consultar status de OS) — introduzir uma fila obrigaria o cliente a fazer polling por uma resposta que hoje é imediata, sem nenhum ganho de desempenho ou resiliência proporcional à complexidade adicionada.
- **Lambda intermediária fazendo a chamada HTTP à app**: adicionaria um cold start extra a cada requisição sem necessidade — o `HTTP_PROXY` nativo do API Gateway já faz esse encaminhamento sem compute extra.

## Consequências

- Acoplamento temporal: se a aplicação no EKS estiver indisponível ou sob HPA em transição, o Gateway retorna erro ao cliente em vez de enfileirar — aceitável para o perfil de uso atual (oficina, não alta escala transacional).
- O node precisa estar sempre acessível pelo Gateway — força a ordem de deploy documentada no README do repositório lambda (infra-k8s → infra-database → app → lambda). Sem ALB, o IP específico do node pode mudar se ele for substituído, exigindo reaplicar o repositório lambda — trade-off aceito por custo (ver [ADR 0008](./0008-prioridade-de-custo-aws-academy.md)).
