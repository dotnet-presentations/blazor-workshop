// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Client;

// This is a client-side AuthenticationStateProvider that determines the user's authentication state by
// looking for data persisted in the page when it was rendered on the server. This authentication state will
// be fixed for the lifetime of the WebAssembly application. So, if the user needs to log in or out, a full
// page reload is required.
//
// This only provides a user name and email for display purposes. It does not actually include any tokens
// that authenticate to the server when making subsequent requests. That works separately using a
// cookie that will be included on HttpClient requests to the server.
internal class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> s_defaultUnauthenticatedTask =
        Task.FromResult(new AuthenticationState(
            user: new ClaimsPrincipal(identity: new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask = s_defaultUnauthenticatedTask;

    public PersistentAuthenticationStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<ClientUserInfo>(nameof(ClientUserInfo), out var userInfo) || userInfo is null)
        {
            return;
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.Email),
            new Claim(ClaimTypes.Email, userInfo.Email)
        ];

        _authenticationStateTask = Task.FromResult(
            new AuthenticationState(
                user: new ClaimsPrincipal(
                    identity: new ClaimsIdentity(
                        claims: claims,
                        authenticationType: nameof(PersistentAuthenticationStateProvider)))));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;
}
