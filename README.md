# blazor-workshop
Blazor workshop


Sessions

1. Intro
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
- Parameters: PizzaSpecial object

Lunch

1. Handling UI events & data binding
- Make special pizza cards clickable
- Clicking on a special brings up the new customize dialog
- Index needs to handle the hide/show of the dialog 
- Index needs to pass in the Pizza object as well as two 'command' delegates
- Using `bind` and `onclick` on the customize dialog to update prices in real time
- explain the difference between `bind` and `bind-value-oninput` on the slider
- cancel button should close the dialog
- confirm button should close the dialog and add to order
- now add the markup for sidebar which will display orders
- add a ConfiguredPizzaItem component
- hook up the order button to do an HTTP POST and clear the order
- (no way to see existing orders yet)
1. DI
- Create a service for interacting with the backend, repository abstraction
- Refactor HttpClient code to use service instead
- Talk to DI scopes
1. Build the order status screen
- Confirmation screen
- Cover `@page`
1. JS interop
- Add order status
- Real status (map location, time to delivery) via polling
- JS interop for the map
- Add payment via browser payment API

End Day 1

1. Templated components
  - Refactor the specials page
  - Generic components
1. Authentication & authorization
- See status after leaving site
- Cascading parameters
- Use some form of thing-that-you-do auth
- Maybe talk to updated guidance for SPA token based auth?

Lunch

1. Publish & deployment
- Publish to Azure
1. Razor Components
- See if we can make flipping it to server-side reasonably trivial
1. Component libraries
- Make a Google Maps components so others can use it
- Consider using SignalR library?
1. Component internals
- Lifecycle events
- Render tree
- StateHasChanged
- Configuring the linker (i.e. how to turn it off, pointer to docs)
1. Q&A

Today we will build:

- Pizza store
  - Order status and map
  - Specials page
  - Ordering page
  - No cart, just one pizza per order
  - Use browser payment apis
  - Login

