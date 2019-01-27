# Authentication and authorization

Rough Notes:

1. Authentication & authorization
- Uncomment user-id related code in OrdersController - now you can no longer view or place orders
- Introduce cascading parameter for UserState
- Decorating the router with UserStateProvider makes UserState available everywhere
- Write the UserInfo component - the only new feature here is consuming a cascading parameter
- Add the UserInfo component to the layout so you can see sign-in/sign-out from every page
- Adding a new layout ForceSigninLayout that prevents rendering of content when not signed in
- Apply this layout to the MyOrders page and OrderDetails page will trigger a signin on navigation