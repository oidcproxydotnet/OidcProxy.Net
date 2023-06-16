---
author: Albert Starreveld
title: What is a Back-end For Front-end?
tags: ["BFF"]
---
# What is a Back-end For Front-end?

The BFF pattern originates from SoundCloud. They had an app that drained phone batteries. The cause: their microservices. The app had to invoke requests to so many different APIs that the number of open HTTP-connections ended up draining batteries.

The solution was simple: Build a server-side API specifically for one front-end, which collects all the data from all microservices in one go. That way, there's only one HTTP-connection to be kept open by the app.

So, this is what that looks like:

[![BFF](https://miro.medium.com/v2/resize:fit:640/format:webp/1*D-Cq29GSEVCl8skJUuW3Ug.png)](https://martinfowler.com/articles/micro-frontends.html)

Read more about the BFF pattern here:
* https://samnewman.io/patterns/architectural/bff/
* https://martinfowler.com/articles/micro-frontends.html
* https://medium.com/@abstarreveld/what-is-a-bff-and-how-to-build-one-e2a2b78cfc43

## The BFF Security Pattern
If you have a dedicated back-end for your front-end, a sensable option is to allow the users of your site to log in there. This makes the back-end slightly more complex, but it makes the web application more secure.

Enriching the BFF with authentication is known as the BFF Security Pattern. [Read about what the BFF Security Pattern is exactly, and how to implement it](/concepts/bff-security-pattern/).