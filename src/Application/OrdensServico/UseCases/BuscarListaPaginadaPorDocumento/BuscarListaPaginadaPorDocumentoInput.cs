using SharedKernel;

namespace Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;

public record BuscarListaPaginadaPorDocumentoInput(string Documento, PagedRequest Paginacao);