namespace Application.OrdensServico.UseCases.AprovarOrcamento;

// CallerClienteId: preenchido só quando quem chama logou como Cliente (via
// CPF) — null para Admin. Usado pra impedir um Cliente aprovar orçamento de
// OS de outro Cliente; Admin nunca é restringido por isso.
public record AprovarOrcamentoInput(Guid Id, Guid? CallerClienteId = null);
