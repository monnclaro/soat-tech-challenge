namespace Application.OrdensServico.UseCases.ReprovarOrcamento;

// CallerClienteId: preenchido só quando quem chama logou como Cliente (via
// CPF) — null para Admin. Usado pra impedir um Cliente reprovar orçamento de
// OS de outro Cliente; Admin nunca é restringido por isso.
public record ReprovarOrcamentoInput(Guid Id, Guid? CallerClienteId = null);
