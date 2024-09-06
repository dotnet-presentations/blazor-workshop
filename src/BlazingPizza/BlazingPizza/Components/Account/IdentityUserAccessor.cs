// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<PizzaStoreUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<PizzaStoreUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
