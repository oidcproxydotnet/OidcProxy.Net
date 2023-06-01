---
author: Albert Starreveld
title: Distributed apps
description: A distributed app is an app that runs on a number of machines in a network. A distributed app processes workload by delegating parts of the work to a number of machines which process their workload in parallel.
tags: ["cloud native", "distributed apps", "cloud native apps", "microservices", "microfrontends"]
---
# Distributed apps

A distributed app is an app that runs on several machines in a network. A distributed app processes workload by delegating parts of the work to several machines which process their workload in parallel.

There is no concise definition of what a 'distributed app' is. There are, however, typical applications that are referred to as distributed apps.

Service Oriented Architecture or Microservice Architecture are examples of distributed apps. Different servers process different workloads in parallel. 

## Why build a distributed app?
There are several reasons to choose a distributed architecture:

* _Enormous workloads_\
Assume you want to search the internet for the word 'computer', you want the search results ordered by relevance, and you want this task to be completed in 1 second. Considering the vast number of websites that need to be searched, this task cannot be done by executing this query synchronously on one computer. This requires an architecture with several computers which process parts of these requests asynchronously, and in parallel.

* _Cost/Scalability_\
In the Cloud, chances are, you pay per use. So, if the system is not used, ideally, you don't want to pay for idle resources. So, to save money, it is advisable to scale down parts of the system that aren't used much. This is only possible if the system is a composition of several deployable.

* _Reducing complexity and time to market_\
The average bank, for example, owns millions (if not billions) of lines of code. Imagine the maintenance nightmare if all of those are in a single application.\
\
So, to reduce complexity, automation for such companies manifests in several applications. Each application is maintained by a dedicated team. That way, the teams can work in parallel. This shortens the time to market. Also, the teams can focus on one (sub)domain. This usually manifests in fewer errors in the application.

## What does a distributed app typically look like?

### From a network perspective

Typically, this is what a distributed app looks like from a network perspective:

![The network perspective](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/distributed-app.png)

This image shows different applications deployed in a single network. These applications scale individually. From a user-experience perspective, the users don't know they are interacting with several applications. Their requests are routed to the correct application by a gateway. That way, the user of the application is always interacting with a single entry point.

### From a functional perspective
You don't see it, but the websites you are visiting are often built this way. At Amazon, for example, this might very well be the case:

![The functional perspective](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/micro-frontends.png)

Chances are, the Amazon website is a composition of several applications, presented to you in a single website:

* The shopping basket (owned by the checkout team)
* The search-application (owned by the product team)
* The site itself (owned by the platform team)
* Maybe there's an application you don't see too, the warehouse application? Or the tax application? These applications do work too, but they are only visible to Amazon employees.

### From a team perspective
There's a computer scientist called Melvin Conway, and after a lot of research he came to the following conclusion:

> Any organization that designs a system (defined broadly) will produce a design whose structure is a copy of the organization's communication structure

Building an application this way has an impact on your organization. The way the system is divided into applications must match the organization's communication structure.

This means that you have two options:
* Design the software architecture in such a way that it reflects the organization's communication structure
* Change the communication structure to match your desired software architecture

Assuming, communication is not an issue at Amazon, maybe Amazon has the following teams:

* The platform-team
* The checkout team and the checkout-it-team
* The product team and the product-it-team
* The finance team and the finance-it-team

## Summary
A distributed app delegates work amongst its nodes. This makes it possible to process the work more efficiently. Distributed apps make sense in large IT organizations or applications that have a specific scaling requirement. Distributed apps have an impact on the organization. 

## Links
* https://en.wikipedia.org/wiki/Melvin_Conway
* https://scs-architecture.org/
