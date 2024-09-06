// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed record class NotificationSubscription(
    int? NotificationSubscriptionId,
    string? UserId,
    string? Url,
    string? P256dh,
    string? Auth);
