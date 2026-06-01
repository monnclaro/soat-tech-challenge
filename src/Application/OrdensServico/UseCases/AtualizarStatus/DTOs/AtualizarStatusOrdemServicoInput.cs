using Domain.OrdensServico.Enums;

namespace Application.OrdensServico.UseCases.AtualizarStatus.DTOs;

public record AtualizarStatusOrdemServicoInput(Guid Id, StatusOrdemServico Status);