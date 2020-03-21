# Outline

Day 1

0. Intro
    - Who are we?
    - Machine setup
    - Syllabus
    - What is Blazor/WebAssembly?
    - Roadmap explanation
1. Components + layout
    - Clone repo with ready made backend
    - Setup store branding
    - Create layout and home page
    - Fetch specials list from backend
    - Display list of pizza names
    - Pizza card component (no templates yet)
    - Parameters: Pizza object

Lunch

2. Handling UI events & data binding
    - Make special pizza cards clickable
    - Clicking on a special brings up the new customize dialog
    - Index needs to handle the hide/show of the dialog 
    - Index needs to pass in the Pizza object as well as two 'command' delegates
    - Using `@bind` and `@onclick` on the customize dialog to update prices in real time
    - explain the use of `@bind:event="oninput"` on the slider
    - cancel button should close the dialog
    - confirm button should close the dialog and add to order
    - now add the markup for sidebar which will display orders
    - add a ConfiguredPizzaItem component
    - hook up the order button to do an HTTP POST and clear the order
    - (no way to see existing orders yet)
3. Build the order status screen
    - Add a new page MyOrders with `@page orders`
    - Add a new NavLink to the layout that links to this URL
    - At this point you can appreciate how this page will share layout because that's specified in imports
    - MyOrders should retrieve list of orders and show the past orders
    - Add a new page OrderDetails to show the status of an individual order
    - It should be possible to click from MyOrders->OrderDetails
    - The OrderDetails should poll for updates to the order from the backend
    - Go back to the index and make placing an order navigate you to the MyOrders page
4. DI and AppState pattern
    - Notice that we lose track of any pizzas when you switch between MyOrders and Index, we can fix this by storing the state at a higher level
    - Create the OrderState class
    - Add to DI in Startup (Scoped)
    - Move most of our properties / methods in Index and ConfigurePizza to the OrderState
    - Add a StateChanged event to OrderState
    - Subscribe to StateChanged from Index in OnInitialized
    - Add an implementation of IDisposable to unsubscribe
5. JS interop
    - Add order status
    - Real status (map location, time to delivery) via polling
    - JS interop for the map
    - Add payment via browser payment API

Day 2

6. Templated components
    - Refactor the specials page
    - Generic components
7. Authentication & authorization
    - See status after leaving site
    - Cascading parameters
    - Use some form of thing-that-you-do auth
    - Maybe talk to updated guidance for SPA token based auth?

Lunch

8. Publish & deployment
    - Publish to Azure
9. Advanced components
    - Component libraries
    - Component lifecycle events
    - Render tree
    - StateHasChanged
    - Configuring the linker (i.e. how to turn it off, pointer to docs)
10. Q&A
