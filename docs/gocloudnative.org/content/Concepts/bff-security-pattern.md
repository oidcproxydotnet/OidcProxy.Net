---
author: Albert Starreveld
title: The Back-end For Front-end Security Pattern
description: The Back-end For Front-end Security pattern is an a stateful BFF. It augments requests to the downstream services by adding Bearer tokens to the requests. 
tags: ["BFF", "Microfrontends", "Microservices", "Authentication"]
---
# Back-end For Front-end Security Pattern

The Back-end For Front-end pattern is a gateway, specifically made for one front-end, which routes traffic to down-stream services. The goal is to improve the user-experience.

## The BFF Pattern

This is what a Back-end For Front-end looks like:

![BFF pattern](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/bff.png)

* A user interacts with a Single-Page Application
* The Single-Page Application interacts with the BFF
* The BFF decides which downstream services to use to fulfill the request
* The BFF aggregates the results and returns it to the Single-Page Application

This improves the user experience because the Single-Page Application invokes fewer HTTP requests.

Read more about the BFF pattern here:
* https://samnewman.io/patterns/architectural/bff/
* https://abstarreveld.medium.com/what-is-a-bff-and-how-to-build-one-e2a2b78cfc43

## The BFF Security Pattern
Initially, the BFF Pattern doesn't solve any security issues. But because when a front-end has a dedicated back-end, it makes sense to authenticate on the server side. 

The Back-end For Front-end Security is a Back-End For Front-End with authentication endpoints added to it. It manages the user-session on the server-side and it augments requests to the downstream services by adding the Bearer token to the requests. This is how it works:

![BFF pattern](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/bff-security-pattern.png)

* A user interacts with a Single-Page Application
* The Single-Page Application interacts with the BFF
* The BFF decides which downstream services to use to fulfill the request
* The BFF forwards the requests to these services and adds a Bearer token to it
* The BFF aggregates the results and returns it to the Single-Page Application

This is how the BFF obtains a Bearer token
* A user interacts with a Single-Page Application
* The Single Page Application navigates the user to the BFF's login-endpoint
* The BFF redirects to the Identity Provider
* The user sees a login-page and logs in
* The user is redirected back to the BFF with a code
* The BFF and the Identity Provider exchange the code for a bearer token
* The user is redirected back to the site

## Implications
Implementing the Back-end for Front-end has some implications:

* The BFF and the SPA must be hosted on the same domain. This means the BFF typically runs on www. It also means the BFF serves the index.html page.
* The BFF is no longer stateless

From an infrastructural perspective, this is what a BFF looks like:

![BFF Infra](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/raw/main/docs/gocloudnative.org/content/Diagrams/architecture.png)

In this diagram there is a:

* Single Page App at the server side
    * Runs in the browser
    * It runs on the same domain as the API's
    * It does not take initiative to authenticate users
* The BFF
    * Serves the resources for the Single Page Application (index.html and the /dist folder)
    * Exposes the API's
    * Has a HTTP-Session
    * Takes initiative to authenticate the user (by redirecting the user to the Identity Provider)
* The Identity Provider
    * This is an OpenId Connect server
    * It produces access tokens
    * It validates the users identity. In other words: Users log in here.
* APIs/microservices
    * Protected with access_tokes. Typically, the resources in this API can only be accessed with a valid access_token.
    * The APIs apply a rule to determine whether a user is authorized to use/see a resource or not. In other words: It applies a policy.

## How to implement it

Read quickstarts to see how to configure the BFF, the APIs. Also refer to the quickstarts for working demos:

- [How to implement a C# BFF with Auth0](/integration-manuals/quickstarts/auth0/quickstart)
- [How to implement a C# BFF with Azure Active Directory](/integration-manuals/quickstarts/azuread/quickstart)
- [How to implement a C# BFF with IdentityServer4](/integration-manuals/quickstarts/identityserver4/quickstart)
