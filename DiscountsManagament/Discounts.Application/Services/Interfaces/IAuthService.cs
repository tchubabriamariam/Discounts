// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs;

namespace Discounts.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerRequestDto request,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDto> RegisterMerchantAsync(RegisterMerchantRequestDto request,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    }
}
