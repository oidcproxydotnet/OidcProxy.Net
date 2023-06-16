---
author: Albert Starreveld
title: The Back-end For Front-end Security Pattern
description: The Back-end For Front-end Security pattern is an a stateful BFF. It augments requests to the downstream services by adding Bearer tokens to the requests. 
tags: ["BFF", "Microservices", "Authentication"]
---

# The Back-end For Front-end Security Pattern
In today's digital landscape, delivering exceptional user experiences while ensuring robust security measures is paramount for the success of web applications.

By shifting the responsibility of aggregating data from multiple sources to the BFF, and thereby minimizing HTTP requests made by the front-end, the BFF pattern improves the overall user experience. However, the benefits don't end there. Recognizing the need for stringent security measures, the BFF pattern can be extended to incorporate authentication and authorization mechanisms, making it a powerful tool to safeguard sensitive user information.

This article explores the Back-end For Front-end pattern and its security extension, shedding light on their implementation, benefits, and implications. We will delve into the inner workings of the BFF pattern, illustrating how it optimizes front-end communication with downstream services. Additionally, we will examine the Back-end For Front-end Security pattern, which augments the BFF with authentication endpoints and token-based security measures.

### Overview of the BFF Pattern

* The user interacts with a Single-Page Application (SPA).
SPA communicates with the BFF
* BFF determines the appropriate downstream services to fulfill the request
* BFF aggregates the results and returns them to the SPA
This reduces the number of HTTP requests made by the SPA, thereby improving the user experience.

The following picture illustrates it:

![BFF pattern](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/bff.png)

## The BFF Security Pattern
Initially, the BFF pattern does not address security concerns directly. However, since, with the BFF Pattern, the front-end has a dedicated back-end, it is sensible to use it to authenticate users on the server side.

The Back-end For Front-end Security pattern extends the BFF by adding authentication functionality. It handles user sessions on the server side, allows them to authenticate, and enhances requests to downstream services by including a Bearer token. Here's how it works:

### BFF Security Pattern:

* User interacts with a Single-Page Application (SPA)
* SPA communicates with the BFF
* BFF determines the appropriate downstream services to fulfill the request
* BFF forwards the requests to these services while adding a Bearer token
* APIs process the requests
* BFF aggregates the results and returns them to the SPA

Here's how the BFF obtains a Bearer token:

* User interacts with a Single-Page Application (SPA)
* SPA navigates the user to the BFF's login endpoint
* BFF redirects the user to the Identity Provider
* User encounters a login page and logs in
* User is redirected back to the BFF with a code
* BFF and Identity Provider exchange the code for a Bearer token
* User is redirected back to the site

The image below depicts the process of forwarding HTTP requests to downstream services when the BFF Security Pattern is implemented:

![BFF pattern](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/bff-security-pattern.png)


### Implications
Implementing the Back-end for Front-end has the following implications:

* BFF and SPA must be hosted on the same domain. This means the BFF must serve HTML pages.
* BFF is no longer stateless.

As a result, from a network perspective, this is what the application architecture will look like:

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

To implement the BFF pattern, and investigate how this works in dept, refer to the following resources and quickstarts for configuration and working demos:

- [How to implement a C# BFF with Auth0](/integration-manuals/quickstarts/auth0/quickstart)
- [How to implement a C# BFF with Azure Active Directory](/integration-manuals/quickstarts/azuread/quickstart)
- [How to implement a C# BFF with IdentityServer4](/integration-manuals/quickstarts/identityserver4/quickstart)